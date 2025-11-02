# Clinix – Interview Q&A (Entities + DbContext)

## Architecture & Modeling

Q: What are the main aggregate roots and why?
A: Appointment (with owned DateRange and child FollowUps), User (with 1:1 Patient/Doctor/Staff profiles), InventoryItem (with InventoryTransactions), Provider, OutboxMessage, SymptomKeyword, SeedStatus. This aligns with consistency boundaries: appointments and follow-ups change together, user profiles are specialized, and inventory transactions are attached to an item root.

Q: Why is Appointment.When modeled as a value object instead of separate columns?
A: A **value object** encapsulates invariants (Start <= End) and behavior (Overlaps, Duration). EF Core **owned types** persist value objects into the same table with OwnsOne, keeping write paths simple and rules cohesive[12][9].

Q: What’s the purpose of Provider separate from Doctor?
A: To represent organizational grouping (clinic/department/network) and enable queries (by specialty, tags, working hours). Doctors reference ProviderId for flexible staffing and reporting.

Q: How do domain events flow in this design?
A: Entity base captures domain events (e.g., AppointmentScheduled, FollowUpCreated). Application layer can translate them into **OutboxMessage** entries for reliable external notifications (email/SMS/queues) using the **Transactional Outbox** pattern to avoid dual writes[10][13].

## Concurrency & Consistency

Q: How is optimistic concurrency handled?
A: Doctor uses SQL Server **rowversion** via [Timestamp] and IsRowVersion(). EF includes the token in UPDATE WHERE clauses and throws DbUpdateConcurrencyException if the row changed since read (protects the entire row)[1][2]. This is minimal-effort and database-managed[1][2].

Q: Why not use IsConcurrencyToken on byte[]?
A: IsConcurrencyToken alone maps to varbinary(max), doesn’t auto-increment, and can be null if not initialized. IsRowVersion configures the SQL Server rowversion type, non-null, auto-incrementing, and marks it as a concurrency token automatically[2].

Q: Any caveats when testing concurrency?
A: Use a relational provider like SQL Server in tests; in-memory providers don’t enforce concurrency tokens the same way, so tests may pass incorrectly[8].

Q: Which other entities should have concurrency tokens?
A: Appointments, InventoryItem (and possibly InventoryTransaction) since they’re frequently updated and business-critical. This ensures users see and resolve concurrent updates rather than overwriting silently[1][4].

## Workflows & Invariants

Q: Describe the appointment lifecycle enforced by code.
A: Schedule → Scheduled; Reschedule allowed unless Cancelled/Completed/Rejected; Cancel always transitions to Cancelled; Complete only from Scheduled; Reject prohibited after Completed; Approve blocked when Rejected/Cancelled/Completed; Notes can be appended.

Q: How are follow-ups managed?
A: Created from Appointment, initially Pending; can be Rescheduled while Pending; Complete → Completed (emits event); Cancel → Cancelled unless already Completed; reminder timestamp via MarkRemindedNow.

Q: How do you prevent schedule conflicts?
A: Model includes DoctorSchedule blocks; add overlap checks in services and DB constraints where practical. Implement ScheduleLock to coordinate contention on slot booking.

## Outbox Pattern & Messaging

Q: Why use an OutboxMessage table?
A: To implement the **Transactional Outbox** pattern so database state changes and published messages are consistent and durable, avoiding dual-write inconsistencies when a write succeeds but publish fails (or vice versa)[10][13].

Q: What operational considerations come with the outbox?
A: A dispatcher worker, retries with backoff, dead-lettering after max attempts, deduplication/idempotent consumers, and ordering guarantees where needed[10][13].

Q: Can platform features help with outbox?
A: Yes—some platforms provide transactional outbox support to coordinate DB and broker within one transactional API surface[7].

## EF Core Mapping & Performance

Q: Why use OwnsOne for DateRange instead of a separate table?
A: It keeps the value object co-located with Appointment, reduces joins, and preserves VO semantics; owned types live in the same table and are configured on the owner[12][9].

Q: What indexes matter for runtime load?
A: Appointment indexes on ProviderId, PatientId, and (ProviderId, Status) for provider schedules and inbox/outbox views. FollowUp (Status, DueBy) accelerates reminder scans. Doctor indexes on ProviderId and Specialty benefit directory/scheduling.

Q: Why Restrict delete on InventoryTransaction?
A: To preserve immutable stock movement history and prevent accidental loss of audit/financial records.

## Design Alternatives & Trade-offs

Q: Why not flatten DateRange into two columns without a VO?
A: You lose encapsulated rules and behavior and risk scattered validation. VO centralizes constraints and makes domain logic reusable[9].

Q: Could you use complex types instead of owned types?
A: In EF Core 8+, **complex types** better match VO semantics; evaluate based on need for collections and nullability constraints in your model[6].

Q: Why not handle notifications inline after SaveChanges?
A: Inline publishing risks dual-write inconsistencies; the outbox ensures both state and message are part of one reliable workflow, with retries and backpressure control[10][13].

## Future Improvements

Q: What are the top changes to harden data integrity?
A: Add FKs from Appointment → Patient/Provider; enforce unique 1:1 constraints on Patient.UserId/Doctor.UserId/Staff.UserId; add rowversion to Appointment and InventoryItem; add DB constraints to prevent overlapping DoctorSchedule; implement slot-level checks.

Q: What are the top operational improvements?
A: Build an outbox dispatcher with retry/backoff and DLQ; structured logs and metrics for appointments/reminders; correlation IDs across HTTP → domain → outbox.

Q: What about compliance and PII?
A: Redact sensitive fields in logs/outbox payloads; add audit trails for sensitive changes; define retention and archival policies.

# Clinix – Interview Q&A (Application DTOs)

## DTO Design

Q: Why use `record` for most DTOs but `class` for some like LoginModel and InventoryItemDto?
A: Records emphasize immutability and value equality which suit request/response payloads, while classes with settable properties make incremental UI binding simpler in some forms; records are great for DTOs but not for EF entities because EF relies on reference equality and change tracking.

Q: How do DataAnnotations on DTOs help and where do you stop?
A: They provide fast, built-in model validation for shape/format (Required, MaxLength), but business rules (like schedule overlaps) belong in services; this separation keeps controllers thin and rules testable.

Q: Why choose `DateTimeOffset` for Appointment times and `DateOnly` for schedule filters?
A: `DateTimeOffset` captures absolute instants safely across time zones, while `DateOnly` models calendar days; when using `DateOnly`, register JSON converters and model binders or stick to strings with standardized ISO formatting if interop is problematic.

Q: Any pitfalls when relying on `[MaxLength]` and `[Required]`?
A: `[MaxLength]` applies to strings/arrays and guides both model validation and database length when mapped, but UI validators might need explicit handling; ensure server-side validation runs even if client-side is disabled.

Q: How do you avoid leaking sensitive data in DTOs?
A: Never expose Password; minimize PII in list DTOs; redact/mask logs; prefer identifiers over raw contact details unless necessary; apply policy checks for admin-only projections.

## Scheduling & Appointments

Q: What’s the rationale for AdminScheduleRequest filters and DoctorDayViewDto metrics?
A: They enable targeted capacity analysis and staffing decisions; utilization is derived from booked vs total slots; SlotStatus distinguishes Available, Booked, and Blocked for operational control.

Q: How do DTOs ensure appointment validity?
A: ScheduleAppointmentRequest carries Start/End and enforces basic constraints via annotations, while services enforce domain rules (no overlaps, work hours, provider availability) before mapping to Appointment.DateRange.

Q: Why include both AppointmentDto and AppointmentSummaryDto?
A: Summary improves list performance and avoids over-fetching; full DTO is used for detail views; separating projections keeps endpoints fast and tailored.

## Providers & Users

Q: Why flatten ProviderDto and embed role-specific info in UserDetailDto?
A: Flattening simplifies client consumption; embedding role-specific sub-DTOs avoids multiple round-trips and keeps admin screens cohesive, while still allowing selective fetching when needed.

Q: What safeguards exist for UpdateUserRequest?
A: Validate uniqueness (email/phone), restrict role changes to admins via policies, and audit mutations; reject partial updates that break invariants (e.g., empty required fields).

## Time & Binding

Q: Any considerations for `DateOnly` in ASP.NET Core?
A: Ensure your project targets a framework with first-class `DateOnly` support and register converters; otherwise, add a custom model binder or use standardized string formats to avoid binding issues.

Q: Why prefer `DateTimeOffset` over `DateTime` for appointments?
A: Offsets preserve the moment-in-time plus local context, avoiding DST bugs; always store in UTC with offsets normalized at the edge and render for users’ locales in the UI.

## Extensibility & Versioning

Q: How will DTOs evolve without breaking clients?
A: Use additive changes, versioned endpoints for breaking changes, and clear deprecation policies; keep DTOs decoupled from domain so internal refactors don’t leak.

Q: What library or pattern helps reduce manual mapping?
A: A mapper (e.g., extension methods or a mapping library) can centralize translations, enforce null/format rules, and log discrepancies; keep mapping deterministic and covered by tests.

## Hardening & Ops

Q: What’s the error response strategy?
A: Standardize on RFC 7807 ProblemDetails with codes and correlation IDs; clients can branch on codes and show localized messages; include validation details for DTO failures.

Q: How do you test DTOs effectively?
A: Model binding tests, JSON round-trip tests (including DateOnly/DateTimeOffset), validation attribute tests, and mapping tests from DTOs to domain commands/entities.

# Clinix – Application Interfaces Q&A Guide

This document summarizes key **interview-ready insights**, **design justifications**, and **architecture decisions** behind the **Clinix Application Layer Interfaces**.

It is structured as a Q&A reference for developers explaining **why** certain patterns were chosen, **how** they align with Clean Architecture, and **what trade-offs** were considered.

---

## 1. Architecture and Responsibilities

### Q: How do these interfaces support Clean Architecture goals?
They define *application orchestration boundaries* — controllers or Blazor pages call these interfaces, while implementations handle domain operations, persistence, and notifications.  
This keeps infrastructure details out of the domain and aligns with Clean Architecture’s principle of **separation of concerns**.

---

### Q: Why keep INotificationSender and IContactProvider separate?
- INotificationSender abstracts **outbound transport** (email/SMS) and allows easy swapping or toggling of providers.  
- IContactProvider offers minimal, read-only contact access, ensuring notification flows don’t require full entity loads.  
This separation **reduces coupling** and **improves testability** while following layered design best practices.

---

### Q: Where do transactions start and end?
Transactions start at the **application service level** — typically, a single service method that modifies state (e.g., scheduling or canceling an appointment).  
Each service call maps to **one request-scoped DbContext**, and the transaction commits only after all domain invariants pass.  
This aligns with **EF Core’s unit-of-work guidance**.

---

## 2. Repositories and Unit of Work

### Q: Why still use repositories if DbSet is “like a repository”?
While EF Core’s DbSet already acts as a repository, *domain-focused repositories* add value by:
- Exposing **aggregate-specific operations**
- Encoding **business intent**
- Protecting domain invariants  
This aligns with **DDD (Domain-Driven Design)** and avoids arbitrary data access patterns.

---

### Q: Are generic repositories an anti-pattern?
Generic repositories that merely wrap DbSet often:
- Duplicate EF functionality  
- Hide advanced EF features (like tracking, query composition)  
Hence, **prefer focused repositories** for aggregates, or use direct DbContext access for simple use cases.  
This follows both **Microsoft and DDD community recommendations**.

---

### Q: What’s the role of IUnitOfWork here?
It provides explicit **transactional intent** when multiple repositories participate in a single workflow.  
IUnitOfWork should remain a **thin wrapper** over DbContext to preserve EF’s lifecycle and transactional semantics.

---

## 3. Cancellation and Async Design

### Q: Why do all methods accept CancellationToken?
To allow **ASP.NET Core to cancel in-flight work** when clients disconnect or time out.  
This saves compute and improves scalability.  
Internally, tokens should always be passed through and checked in long operations.

---

### Q: Any pitfalls with optional tokens?
Optional tokens are acceptable at **public entry points**, but internal implementations must always **propagate and honor** them.  
Failing to do so can cause **hanging work** and **resource leaks** under high load.

---

## 4. Command and Query Patterns

### Q: Why do some methods return bool while others return Result?
Returning bool hides *why* an operation failed.  
Adopting a Result or **Problem Details** pattern ensures:
- Explicit error reasons  
- Improved UX and observability  
- Consistent error handling across the API  
This approach keeps controllers **thin and predictable**.

---

### Q: How should idempotency be handled?
Commands should safely handle **retries**.  
For example:
- Cancel on an already canceled entity should return a success or specific “AlreadyCanceled” result.  
This ensures stability under **transient failures or duplicate client requests**.

---

## 5. Scheduling and Providers

### Q: How do GetAvailableSlots and AdminSchedule work efficiently?
By composing **provider hours**, **doctor schedules**, and **appointment slots** into optimized **read models** with:
- Proper indexing  
- Cancellation support  
- Avoidance of N+1 queries  
This approach supports **scalable, responsive admin dashboards**.

---

### Q: What does DelayCascade imply?
It likely shifts a chain of appointments forward when a doctor is delayed.  
This must be done **transactionally**, so either *all related slots* update or *none do*, ensuring data consistency.

---

## 6. Inventory and Consistency

### Q: Is InventoryService safe without explicit transactions?
No.  
Stock updates and transaction inserts must occur **atomically** within the same unit of work.  
This guarantees **stock count accuracy** under concurrent access, following EF Core’s transactional best practices.

---

### Q: Why restrict deletes on inventory transactions at the mapping level?
To ensure **auditability** and prevent **history loss**.  
APIs should expose explicit failure results (e.g., “Cannot delete audited transaction”) instead of silently cascading deletes.

---

## 7. Improvement Roadmap

### Q: What are the top API refinements before production?
1. Replace bool returns with Result or ProblemDetails.  
2. Add **paging and filtering** DTOs to list endpoints.  
3. Clearly document **transactional boundaries** per method.  
4. Enforce **consistent cancellation** propagation.  
5. Improve **error modeling** and observability.

---

### Q: How to decide when to use a repository vs direct DbContext?
- Use **direct DbContext** for simple, one-off reads/writes.  
- Use **repository** for aggregate-specific behaviors and queries.  
Repositories should express **business intent**, not just act as generic CRUD wrappers.  
This keeps the persistence layer **purposeful and maintainable**.

---

## 8. Summary

The Clinix Application Layer achieves:
- **Separation of concerns** between layers  
- **Transactional safety** with minimal boilerplate  
- **Cancellation and async robustness**  
- **Scalable query performance** through read model design  
- **Testability** via clean abstractions

Before production, focus on:
- Unifying return types (Result)  
- Strengthening cancellation enforcement  
- Clarifying transactional scopes  
- Documenting repository vs DbContext usage boundaries  
"""


# Clinix – Interview Q&A (Mappings)

## Why manual mappings over AutoMapper here?
Manual mapping provides full control, predictable performance, and straightforward debugging, which is often preferable for complex or security‑sensitive transforms, as multiple sources note about performance and clarity benefits compared to reflection‑based mappers [web:100].  

## When would you reach for a mapping library?
When DTO churn is high and mappings are mostly 1:1, a tool like Mapster/Mapperly can reduce boilerplate with code generation, while keeping overhead low and configuration local to the feature, which several comparative posts recommend for productivity [web:103].  

## How do you keep EF queries efficient with DTOs?
Expose expression‑based projections and use IQueryable.Select to map on the server, letting EF translate to SQL and preventing in‑memory materialization, which is a widely recommended pattern for clean and fast DTO mapping with LINQ [web:107].  

## Why flatten DateRange into Start/End in AppointmentDto?
DTOs should represent the shape the client needs, not the domain’s internal modeling; flattening avoids over‑fetching and keeps serialization trivial, which aligns with Microsoft’s DTO guidance for Web APIs [web:109].  

## How do you prevent “logic leak” into mappers?
Keep mappers pure and side‑effect free; perform validation at the edge and enforce invariants in domain/application services, consistent with DTO best practices materials from Microsoft docs [web:109].  

## What about time types—why recommend DateTimeOffset?
DateTimeOffset represents an absolute moment with offset and avoids local time ambiguity, which is recommended for cross‑time‑zone correctness in scheduling domains, per .NET time guidance [web:112].  

## Should creation timestamps use DateTime.UtcNow or DateTimeOffset.UtcNow?
Favor DateTimeOffset.UtcNow so created/updated stamps carry offset‑aware semantics and match Appointment/FollowUp semantics, as reflected in the .NET API guidance and time recommendations [web:110].  

## How do you test mappings effectively?
Write unit tests that assert field coverage, trimming/normalization, and default values; add round‑trip tests for JSON on DTOs and use projection tests on IQueryable to ensure EF can translate selectors as recommended in DTO documentation [web:109].  

## What are the main risks with AutoMapper‑style tools?
Hidden reflection costs, configuration drift, and harder debugging for conditional transforms; several articles advise manual mapping in performance‑critical or complex mappings where clarity matters most [web:100].  

## How do you avoid N+1 problems while mapping?
Project directly to DTOs at the query level and avoid accessing navigation properties after enumeration; use expression selectors and include only necessary fields, which is consistent with LINQ mapping patterns discussed by practitioners [web:107].  


# Clinix – Interview Q&A (Application Services)

## Architecture and boundaries

Q: Why are application services used instead of controllers calling repositories directly?  
A: They centralize orchestration, enforce invariants, compose domain and infrastructure, and keep the web layer thin while following Microsoft’s guidance on persistence layer design and clean separation of concerns [web:80].  

Q: How are transactions scoped in these services?  
A: Each command aligns to a request‑scoped DbContext and commits once all validations and writes succeed, with explicit transactions when multiple aggregates are changed in one operation, per EF Core lifetime/transaction guidance [web:84].  

Q: Why do all methods accept CancellationToken?  
A: It allows upstream cancelation to propagate and stop work when clients disconnect or time out, which improves scalability and is a recommended practice in ASP.NET Core [web:85].  

## Scheduling and slots

Q: Why use DateTimeOffset for appointment instants and slots?  
A: It preserves the absolute moment across time zones and mitigates DST ambiguity, which .NET documentation recommends for time‑aware domains like scheduling [web:112].  

Q: How are DST transitions handled in 30‑minute slot generation?  
A: Transition days can repeat or skip times, so using a consistent time zone or UTC normalization is required to avoid overlap/gap bugs, and policies must define behavior during repeated or invalid hours [web:123][web:125].  

Q: The current ShowOnlyAvailable logic returns no Available slots; why?  
A: The code only adds Available when ShowOnlyAvailable is false; flip the condition or add a separate branch to include Available‑only results when requested to match the flag’s intent [web:80].  

Q: Why does utilization sometimes look off?  
A: Utilization in AdminScheduleStats uses counts rather than minutes; if appointment durations differ from 30 minutes, compute booked minutes over total scheduled minutes for accuracy [web:80].  

## Provider vs doctor modeling

Q: Appointments use ProviderId, but schedules are per Doctor; is that consistent?  
A: It simplifies lookups but loses precision in multi‑doctor providers; consider adding DoctorId to appointments or explicitly modeling doctor selection during booking to align with weekly schedules and approvals [web:80].  

Q: Why does AdminSchedule pick the first on‑duty doctor only?  
A: It’s a pragmatic default but hides capacity of additional doctors; iterating all active doctors per provider/day yields a fuller capacity picture at the cost of larger results [web:80].  

## Logging and observability

Q: Why replace Console.WriteLine with ILogger?  
A: ILogger integrates with structured, provider‑based logging configured via appsettings and supports categories/scopes and high‑performance patterns, whereas console output is ephemeral and not production‑grade [web:116][web:119].  

Q: How to improve logging in hot paths?  
A: Use LoggerMessage for source‑generated logging with minimal allocations and guard expensive formatting behind log‑level checks to keep overhead low [web:128].  

## Reliability and correctness

Q: Approve doesn’t change status; how to fix?  
A: Update the domain method to set Status to an approved state (e.g., Confirmed) so downstream filters and UI reflect approval, and add tests for invalid state transitions [web:80].  

Q: Is DelayCascade atomic?  
A: Not currently; wrap the batch in a transaction and consider adding concurrency tokens to Appointment so concurrent reschedules or updates don’t lead to lost updates or partial shifts [web:84].  

Q: Patient profile update rolls back only for DbUpdateException; should it roll back on all errors?  
A: Yes, ensure a rollback in the outer catch or use a try/finally to guarantee rollback on any exception, which aligns with robust unit‑of‑work handling [web:84].  

## Time and binding

Q: Why prefer DateTimeOffset.UtcNow for created/updated stamps in services?  
A: It aligns with offset‑aware storage and avoids ambiguities from local time and DST, consistent with .NET guidance for time handling in distributed systems [web:110][web:112].  

Q: Any concerns with DateOnly in requests?  
A: Ensure ASP.NET Core model binding/JSON converters are registered; otherwise, prefer ISO‑formatted strings or DateTime to avoid binding issues across clients [web:65].  

## API shape and errors

Q: Several methods return bool; what’s the downside?  
A: Bool hides failure reasons; use a Result or ProblemDetails mapping so clients get actionable errors and UIs can display precise messages without extra lookups, which improves resilience and debuggability [web:80].  

Q: Where should notifications be sent in these flows?  
A: Prefer post‑commit via a background dispatcher or transactional outbox to avoid dual‑write inconsistencies and to provide retries and dead‑letter handling without blocking user requests [web:80].  

# Clinix – Interview Q&A (Registration & User Management)

## Security and hashing

Q: Why use ASP.NET Core Identity’s PasswordHasher instead of a custom hasher?  
A: It implements the standard Identity hashing pipeline (PBKDF2 by default), supports versioning and SuccessRehashNeeded, and integrates with Identity policies, avoiding cryptographic mistakes and easing future upgrades [web:137][web:140].  

Q: How do you plan to improve hashing over time?  
A: Increase iterations or migrate algorithms and rely on SuccessRehashNeeded to transparently rehash on successful login, which the Identity hasher supports to keep hashes modern [web:137][web:139].  

## Phone normalization

Q: Why target E.164 for phone numbers?  
A: E.164 ensures a global, unique, and provider‑friendly format (up to 15 digits with country code), reduces duplicate accounts from formatting differences, and matches expectations of SMS providers like Twilio [web:151][web:153].  

Q: How will you normalize and validate phones?  
A: Use libphonenumber‑csharp to parse/format to E.164 with a default region when “+” is missing, and reject invalid numbers at registration and updates to maintain consistency and deliverability [web:148][web:151].  

## Transactions and soft delete

Q: How are transactions handled in registration flows?  
A: Begin a transaction, persist User, then profile entity (Patient/Doctor/Staff), and commit once both succeed; on any error, roll back to avoid partial creation, following EF Core unit‑of‑work guidance [web:84][web:81].  

Q: Why soft delete users instead of hard delete?  
A: Soft delete preserves auditability and enables reactivation; EF Core global query filters can exclude soft‑deleted rows by default and IgnoreQueryFilters can be used for admin recovery scenarios [web:150][web:141].  

## Logging and observability

Q: Why favor ILogger over Console.WriteLine?  
A: ILogger supports structured logging, multiple providers, appsettings configuration, and high‑performance patterns (LoggerMessage), making it production‑grade and observable at scale [web:116][web:128].  

Q: What would you log in registration/profile updates?  
A: Success/failure with correlation IDs, normalized phone/email (masked where needed), actor, and entity IDs, plus error categories for DbUpdate vs unexpected exceptions to speed up triage [web:116][web:118].  

## Data integrity and validation

Q: What checks are needed when admin updates a user?  
A: Validate uniqueness of email/phone (normalized), enforce role change policies, and apply transactional updates with clear Result messages, mirroring registration invariants to prevent conflicts [web:80][web:84].  

Q: How do you keep timestamps consistent?  
A: Use DateTimeOffset.UtcNow for created/updated in services to align with other offset‑aware entities and avoid local time/DST ambiguity, as recommended by .NET time guidance [web:110][web:112].  

## Design choices and evolution

Q: Why is a Provider created during doctor creation, and is that ideal?  
A: It ensures scheduling readiness but risks provider proliferation; a better model is selecting an existing Provider or creating one explicitly with admin intent to reflect organizational reality [web:80][web:84].  

Q: How can GetAllUsersAsync be optimized?  
A: Move to server‑side projections (e.g., GetAllWithRoleDetailsAsync) to avoid cross‑joining large lists in memory and reduce round‑trips, following persistence layer design best practices [web:80][web:84].  


Q&A.md
# Clinix – Interview Q&A (Background Services, Outbox, Notifications)

## BackgroundService and timing
- Why use BackgroundService with PeriodicTimer for recurring tasks?  
It provides a clean async loop, integrates cancellation, and avoids timer callback reentrancy issues, making periodic work reliable and easy to reason about. [web:157]  

- How is DI used safely in a long‑running worker?  
Create a scope per tick with IServiceScopeFactory so scoped dependencies (DbContext, repositories) are disposed at the end of each iteration. [web:157]  

- How do you ensure cancellation is honored?  
Always pass the CancellationToken to WaitForNextTickAsync and downstream calls so a host stop or shutdown timeout promptly stops work. [web:162]  

## Graceful shutdown
- What happens during a graceful host shutdown?  
The host calls StopAsync with a shared cancellation token and waits up to the shutdown timeout for services to finish or cancel remaining work. [web:170]  

- How do you tune shutdown time?  
Set HostOptions.ShutdownTimeout to extend the default and ensure ExecuteAsync observes cancellation at loop boundaries to exit quickly. [web:163]  

## Transactional outbox
- Why implement an outbox processor instead of publishing inline?  
It guarantees that state changes and message publication are atomic and retriable, avoiding dual‑write inconsistencies. [web:48]  

- How do retries and failures work?  
The worker increments AttemptCount, retries up to a limit, and marks messages terminal on exhaustion to expose failures for remediation. [web:164]  

- What consistency level does outbox provide?  
At‑least‑once delivery with ordering per source if processed in occurred‑at order; handlers must be idempotent. [web:48]  

## Notification handlers
- Why keep NotificationHandlers separate from aggregates?  
To decouple IO and delivery concerns from domain logic while enabling environment toggles and independent evolution of transports. [web:157]  

- How are recipients resolved?  
Handlers query appointments and contact providers to assemble patient/doctor data, then format templates and dispatch via the sender abstraction. [web:157]  

## Email/SMS delivery
- Why use ILogger for delivery logs instead of Console?  
ILogger routes through configured providers, supports filtering, scopes, and high‑performance patterns suitable for production. [web:117]  

- How is dev vs prod handled?  
Options toggle delivery; in dev, messages are logged for visibility, while prod uses SMTP and optional Twilio activation. [web:116]  

## Reliability and operations
- How do you avoid tight polling loops?  
Use PeriodicTimer with a reasonable interval and consider backoff and jitter to reduce contention after outages. [web:157]  

- How do you observe system health?  
Emit logs, metrics, and traces for batch sizes, success/failure counts, and backlog, and add health checks for liveness/readiness. [web:118]  

## Time and DST
- Why prefer DateTimeOffset.UtcNow in services and workers?  
It avoids local time ambiguities and supports consistent storage and comparison across time zones. [web:170]  

- How do DST transitions affect reminders?  
Repeated or skipped local times can shift windows, so normalize to a consistent zone or UTC and communicate policies in UX. [web:170]  

## Hardening and scale
- How would you scale the outbox processor?  
Increase batch size, add bounded parallelism, ensure idempotent handlers, and consider partitioning by message type or provider. [web:164]  

- What’s next to improve failure handling?  
Introduce exponential backoff, DLQ tables, and operator tools to replay or resolve failed messages. [web:164]
