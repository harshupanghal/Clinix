# Clinix – Domain & Persistence 

Scope
- This document covers the Clinix domain entities and EF Core persistence mapping reflected in ClinixDbContext, focusing on relationships, workflows, invariants, decisions, and future improvements.
- Features represented here include application users (User, Patient, Doctor, Staff), scheduling (DoctorSchedule, Provider, ScheduleLock), care delivery (Appointment, FollowUp, DateRange), knowledge base (SymptomKeyword), inventory (InventoryItem, InventoryTransaction), system operations (OutboxMessage, SeedStatus).

## Architecture (Domain + Persistence)

- Domain base
  - **Entity** base with Id and **domain events** buffer for eventual publishing.
  - **IDomainEvent** carries an OccurredOn timestamp for auditability.

- Aggregate boundaries (current shape)
  - Appointment (aggregate root) with owned **DateRange** and child **FollowUp** entities (collection).
  - User is a root for role-specific profiles: **Patient**, **Doctor**, **Staff** (1:1).
  - InventoryItem (root) with InventoryTransaction children (1:N).
  - Provider is a separate root referenced by Doctor via ProviderId.
  - OutboxMessage, SymptomKeyword, SeedStatus are standalone roots.

- Persistence container
  - EF Core DbContext: sets for Users, Patients, Doctors, Staffs, Appointments, FollowUps, DoctorSchedules, Providers, Inventory, OutboxMessages, SymptomKeywords, SeedStatus.
  - Mappings use fluent config, owned types, indexes, delete behaviors, and one RowVersion concurrency token.

## Entity Catalog and Relationships

- Application Users
  - **User**: core profile and credentials; fields include FullName, Email, Phone (unique), PasswordHash, Role, IsDeleted, IsProfileCompleted.
  - **Patient** (1:1 User): medical profile, emergency contacts; cascade delete from User.
  - **Doctor** (1:1 User): degree, specialty, license, fees, duty status, and a **RowVersion** for optimistic concurrency; references **Provider** via ProviderId; cascade delete from User.
  - **Staff** (1:1 User): administrative or support staff profile; cascade delete from User.

- Scheduling
  - **DoctorSchedule**: weekly availability per doctor (DayOfWeek, StartTime, EndTime, IsAvailable).
  - **Provider**: logical grouping or organization unit (Name, Specialty, Tags, WorkStart/End); Doctors reference ProviderId.
  - **ScheduleLock**: lightweight lock row per doctor to coordinate contention on scheduling.

- Care Delivery
  - **Appointment**: root with PatientId, ProviderId, Type, Status, owned **DateRange** When, Notes; emits domain events on schedule/reschedule/cancel; has many **FollowUps**.
  - **FollowUp**: child linked to Appointment; status transitions Pending → Completed/Cancelled.

- Knowledge Base
  - **SymptomKeyword**: keyword, synonyms (JSON), specialty, weighting for triage/search.

- Inventory
  - **InventoryItem**: stock definition (type, unit, min stock, location, current stock).
  - **InventoryTransaction**: stock movements (IN/OUT/ADJUSTMENT) with audit metadata.

- System
  - **OutboxMessage**: type, payload JSON, occurred, processed flags, attempts, optional channel — for the **Transactional Outbox** pattern.
  - **SeedStatus**: tracks data seeds (name, version, execution metadata, result).

## Key Mappings and Constraints

- Users
  - Phone is unique; core text fields are length-bounded.
  - Role is normalized to a short string; IsDeleted, IsProfileCompleted default to false.

- Patients/Doctors/Staff
  - 1:1 with User via FK on their own side; cascade on delete.
  - Doctor has indexes on ProviderId and Specialty.

- Appointments
  - Owned **DateRange** mapped via OwnsOne to columns Start/End (datetimeoffset).
  - Indexes on ProviderId, PatientId, and (ProviderId, Status).
  - Has many FollowUps (cascade).

- FollowUps
  - Indexes on AppointmentId and (Status, DueBy) for reminder scans.

- Scheduling
  - DoctorSchedule enforces required fields; cascade on Doctor delete.

- Provider
  - Index on Specialty; datetime2 for WorkStart/End.

- Inventory
  - InventoryItem 1:N InventoryTransaction with Restrict delete (preserves transactions).

- OutboxMessage
  - Index on (Processed, OccurredAtUtc) for efficient dequeue scans.

- Concurrency
  - **Doctor.RowVersion** uses [Timestamp] and IsRowVersion() for **SQL Server rowversion**-based optimistic concurrency; EF includes this token in UPDATE WHERE clauses and throws DbUpdateConcurrencyException on conflicts[1][2]. The rowversion column auto-changes on each update and protects the whole row as a minimal-effort concurrency token[1][2].

- Value Objects
  - **DateRange** is a value object persisted via EF Core **owned entity type** mapping; owned types live in the same table as the owner and are configured with OwnsOne[12][9]. Value objects are ideal for modeling concepts like time ranges without identity[9][6].

## Domain Workflows (from Entities)

- Appointment lifecycle
  - Create via Appointment.Schedule(patientId, providerId, type, when, notes) → Status = Scheduled; raises AppointmentScheduled.
  - Reschedule(newWhen) → validates state; updates When; raises AppointmentRescheduled.
  - Cancel(reason) → sets Cancelled; raises AppointmentCancelled.
  - Complete() → only from Scheduled → Completed.
  - Approve() → guards against invalid states (Rejected/Cancelled/Completed).
  - Reject(reason) → sets Rejected and appends note.

- Follow-up lifecycle
  - Create from Appointment: appointment.CreateFollowUp(dueBy, reason) → initial Pending; raises FollowUpCreated.
  - Complete(notes) → sets Completed; raises FollowUpCompleted.
  - Reschedule(newDueBy) only when Pending.
  - Cancel(notes) → moves to Cancelled unless already Completed.
  - MarkRemindedNow() → stamp for reminder job.

- Scheduling and availability
  - DoctorSchedule defines recurring availability blocks.
  - ScheduleLock rows enable short-lived “claim” windows for doctor slot allocation under contention (Updated by, LockedUntil).

- Inventory operations
  - InventoryTransaction logs each movement; delete restricted to preserve audit.

- Outbox messaging
  - Domain events recorded on entities can be translated into **OutboxMessage** records to deliver side effects reliably using the **Transactional Outbox** pattern, which avoids dual-write inconsistencies when persisting data and publishing messages[10][13].

## Design Decisions and Trade-offs

- Optimistic concurrency with SQL Server rowversion on Doctor
  - Decision: use **rowversion** via [Timestamp]/IsRowVersion for minimal-friction, whole-row protection on updates[1][2].
  - Pros: automatic token management by DB; strong conflict detection; simple EF configuration[1][2].
  - Cons: SQL Server–specific; tests using non-relational providers won’t behave the same; other entities remain unprotected unless extended[1][8].
  - Alternatives: generic IsConcurrencyToken on various types (more manual), or app-level version fields. IsConcurrencyToken on byte[] won’t auto-increment or prevent nulls; IsRowVersion configures proper rowversion and sets concurrency automatically[2].

- Value object for DateRange (OwnsOne)
  - Decision: keep **DateRange** as a value object to centralize time-range rules and map as an owned type with Start/End in the Appointment table[12][9].
  - Pros: cohesive validation (Start <= End), reusable logic (Overlaps, Duration), no separate table, clear boundary[12][9].
  - Cons: owned types have constraints (e.g., identity handling, some limitations vs full entities); for EF Core 8+ you could also consider complex types for VO semantics[12][6].

- Transactional Outbox
  - Decision intent: persist OutboxMessage to later publish notifications, ensuring atomically persisted state and eventual message delivery, avoiding dual-write inconsistencies[10][13].
  - Pros: consistency across DB + broker boundaries; supports retries, idempotency at consumers[10][13].
  - Cons: requires background dispatcher, poison handling, idempotent consumers; additional storage and ops[10][13].

- Delete behaviors
  - Cascade on user → patient/doctor/staff simplifies cleanup but must be aligned with compliance/audit needs.
  - Restrict on inventory transactions prevents data loss in financial/audit trails.

- Provider vs Doctor
  - Doctor references ProviderId to model organization, specialization, or grouping. This allows querying by provider specialty and scaling to multi-provider scenarios.

## Reliability, Testing, and Observability Notes

- Concurrency testing
  - Use a relational provider (SQL Server) in tests to exercise real **rowversion** behavior; in-memory providers won’t enforce concurrency the same way[8].

- Background processing
  - Follow-up reminders and outbox dispatchers should emit structured logs and metrics for visibility (success/failure counts, retries).

- Indexing and performance
  - Appointment indexes support provider and patient filtering, and status scans.
  - FollowUp (Status, DueBy) supports due reminders.

## Security and Data Integrity Hints

- Guard rails
  - Appointment methods already constrain illegal transitions (e.g., reject after completed).
  - Consider validation for DoctorSchedule overlaps and Appointment overlaps per provider.

- PII and audit
  - Outbox payloads should avoid sensitive PII or be redacted; audit key events through domain events and safe logging.

## Improvements and Roadmap

- Concurrency expansion
  - Add **rowversion** tokens to Appointment and InventoryItem (or all mutable aggregates) for consistent conflict detection[1]. Consider surfacing concurrency errors to users with retry/resolution flows[4].

- Strengthen value objects
  - Enforce invariants on DateRange more broadly (e.g., min duration, clinic hours).
  - Consider EF Core 8 **complex types** for value objects where appropriate[6].

- Scheduling integrity
  - Add unique constraints to prevent overlapping DoctorSchedule entries for the same day/time window.
  - Implement slot inventory (discrete capacity) and enforce no overlaps for Appointment.When per Provider.

- Outbox operationalization
  - Add a background dispatcher, exponential backoff, max attempts, and dead-letter handling; ensure consumer idempotency as duplicates can occur[10][13].
  - Optionally use a platform feature to simplify transactional outbox wiring[7].

- Data quality and referential integrity
  - Explicit FK from Appointment.PatientId → Patient and Appointment.ProviderId → Provider for stronger relational guarantees.
  - Enforce uniqueness on Patient.UserId / Doctor.UserId / Staff.UserId at DB level (already enforced by PK mapping, consider additional constraints for clarity).

- Auditing and compliance
  - Introduce CreatedBy/UpdatedBy consistently; add soft-delete where needed; define data retention policies for healthcare data.

- Inventory robustness
  - Add stock-level reconciliation, min-stock alerts, and transactional consistency checks when applying InventoryTransaction to InventoryItem.CurrentStock.

- Observability
  - Add correlation IDs across request, domain events, and outbox processing; track key metrics (e.g., appointment booking latency, reminder delivery rate).

## Open Questions

- Should Appointment and FollowUp also use **rowversion** for concurrency, and how should conflicts surface in the UI[1][4]?
- Is Provider a clinic/department or an external organization? Should Doctors also be related to User and Provider with constraints if Provider changes?
- Do we need explicit FKs from Appointment to Patient and Provider (currently scalar IDs only) for query navigations and cascade policy?
- What is the policy for Appointment overlaps per Provider and per Patient? Hard block vs allow with warnings?
- Which events should be persisted into **OutboxMessage** (e.g., AppointmentScheduled, FollowUpCreated), and what message schema/versioning will we use[10][13]?

# Clinix – Application Layer DTOs

Scope
- This document describes the purpose, shape, and usage of Application-layer DTOs for Patients, Appointments, Scheduling/Admin, Provider, FollowUps, Inventory, Auth, and Admin User Management, including how they map to domain entities and how validation/model binding should be handled in ASP.NET Core. 
- Design notes cover records vs classes for DTOs, use of DataAnnotations, and time types (DateOnly, DateTimeOffset) for correct serialization and model binding.

## Design Principles

- DTOs are transport shapes for UI/API, separate from domain entities, keeping controllers/pages decoupled from persistence and enabling versioning without breaking core domain. 
- Prefer C# `record` for immutable, value-based DTOs used in request/response flows, and `class` when settable properties and incremental binding are desirable in UI forms. 
- Use ASP.NET Core model binding and DataAnnotations for syntactic validation near the edge; deeper invariants live in application/domain services.

## Validation & Model Binding

- DataAnnotations such as `[Required]` and `[MaxLength]` should validate incoming request models at the edge; `[MaxLength]` constrains arrays/strings and is recognized by model validation and EF column sizing when mapped through entities. 
- Record types work well for request/response immutability and value semantics; avoid using records as EF entities due to EF’s reference-equality tracking requirements. 
- Prefer `DateTimeOffset` for wall-clock moments that cross time zones (appointments, slots), and `DateOnly` for day-based filters; if using `DateOnly`, ensure model binding/JSON support or register custom binders/converters in ASP.NET Core.

## Patient DTOs

- RegisterPatientRequest
  - Fields: FullName, Email, Phone, Password (plaintext only transiently; hash server-side). 
  - Maps to: User + Patient creation pipeline. 
  - Validation: required fields; ensure normalized phone and email format.

- CompletePatientProfileRequest
  - Fields to complete post-login: DOB, Gender, BloodGroup, Emergency contacts, Allergies, Conditions. 
  - Maps to: Patient profile enrichment; toggles User.IsProfileCompleted when minimal criteria is met.

- PatientUpdateProfileRequest
  - Allows updating both basic and medical profile fields; enforce partial updates and audit changes. 

- PatientDashboardDto
  - Read model combining User and Patient data for the dashboard; appointments/follow-ups can be added later as projections.

## Appointment DTOs

- ScheduleAppointmentRequest
  - Required: PatientId, ProviderId, Type, Start, End; optional Notes (MaxLength(1000)). 
  - Maps to: Appointment.Schedule with DateRange(Start, End). 
  - Validate: Start < End; provider work hours; overlapping/availability rules.

- RescheduleAppointmentRequest / CancelAppointmentRequest / CompleteAppointmentRequest
  - Target specific state transitions; App layer should translate validation outcomes to user-friendly messages.

- AppointmentDto / AppointmentSummaryDto
  - Read models projecting domain entity: Id, PatientId, ProviderId, Type, Status, Start, End, timestamps.

## FollowUp DTOs

- CreateFollowUpRequest / CompleteFollowUpRequest / CancelFollowUpRequest
  - Flow: create pending, complete, cancel; guard idempotency and illegal transitions in service layer. 
- FollowUpDto
  - Read model for Pending/Completed/Cancelled with DueBy timestamps for reminders.

## Scheduling/Admin DTOs

- AdminScheduleRequest
  - Query parameters for schedule views: date range, doctor/provider filters, specialty, statuses, availability, utilization thresholds. 
  - Use `DateOnly` for Start/End day filters; ensure model binding and JSON converters are configured.

- DoctorScheduleSlotDto
  - Represents a single slot: ProviderId, DoctorName, Specialty, Start/End, SlotStatus (Available/Booked/Blocked), optional AppointmentId/PatientName, Type, AppointmentStatus. 

- DoctorDayViewDto
  - Aggregates day view per doctor: list of slots, totals, booked count, utilization percent, working day flag; utilization is a derived metric.

- AdminScheduleStatsDto
  - Summary metrics across a filter: totals, pending approvals, available slots, average utilization, no-shows today, completed today.

## Provider DTOs

- ProviderDto
  - Flat read model: Id, Name, Specialty, Tags, WorkStart, WorkEnd. 
- ProviderRecommendationRequest
  - Query text, AppointmentType, optional DesiredStart for provider suggestions. 
- AvailableSlotsRequest
  - ProviderId and day (DateOnly) to generate slots. 
- UpdateProviderWorkingHoursRequest
  - Update working hours; validate that Start < End and within business rules.

## User Management DTOs (Admin)

- UserListDto / UserDetailDto / UserStatsDto
  - Admin views with role-specific projections: Doctor, Patient, Staff details embedded as sub-DTOs; ensure PII minimization where not needed. 
- UpdateUserRequest
  - Admin updates to basic fields; guard role changes with policy checks and audit trails. 
- CreateDoctorRequest / DoctorScheduleDto / CreateStaffRequest
  - Creation flows for staff and doctors, including initial schedules and operational metadata; validate phone uniqueness and role assignment.

## Security Notes

- Never log or return Password; hash and salt server-side; ensure secure transport. 
- Validate and normalize Email/Phone; throttle registration and sensitive changes; mask PII in logs and projections by default where feasible. 
- Authorize admin-only DTOs and endpoints with role/claim policies.

## Decisions & Trade-offs

- Record vs Class
  - Decision: use `record` for requests/responses favoring immutability and value equality, and `class` where setters are convenient for UI binding. 
  - Pros: concise, safer DTOs; clearer intent; easier equality in tests. 
  - Cons: records aren’t suitable for EF entities; mixed use requires team conventions.

- DateOnly vs DateTimeOffset
  - Decision: `DateOnly` for day filters and `DateTimeOffset` for precise schedule times; configure model binding/JSON converters for `DateOnly`. 
  - Pros: correctness across time zones and clarity for day vs instant. 
  - Cons: requires binder/converter setup; interop with JS clients must be standardized.

- DataAnnotations at the edge
  - Decision: keep syntactic checks on DTOs and business rules in application/domain services. 
  - Pros: clear separation, reusable domain rules. 
  - Cons: duplicated constraints need alignment to avoid drift.

## Improvements

- Add FluentValidation for richer rules and localized messages; keep DataAnnotations for basic constraints. 
- Standardize date/time serialization (ISO 8601), enforce UTC for offsets, and register `DateOnly` converters in both JSON and model binding. 
- Introduce versioned DTOs for external APIs; avoid breaking changes to public shapes. 
- Add pagination/filtering DTOs for list endpoints; avoid over-fetching by tailoring projections. 
- Add problem details responses with error codes for consistent client handling.

## Open Questions

- Should CreateDoctorRequest and CreateStaffRequest accept ProviderId or map later during onboarding? 
- Should phone be the only login identifier, or support email/OTP flows? 
- What error model will clients consume (ProblemDetails with codes vs simple strings)? 


# Clinix – Application Layer Interfaces

## Scope
This document outlines the purpose and responsibilities of each **application service** and **repository interface** within the Clinix application. It explains how these components collaborate to execute workflows and details the expected behaviors around **transactions**, **cancellation**, and **consistency**.

It also highlights key **design decisions** made for repository patterns, **unit of work** management using EF Core, and **cancellation token** usage patterns. Lastly, it identifies potential areas for **improvement** before production deployment.

---

## Architectural Role
The **application layer** coordinates domain logic, persistence, and notifications, ensuring that controllers and Blazor pages remain thin while the domain layer stays isolated from infrastructure concerns — consistent with modern layered architecture practices.

- **EF Core DbContext** acts as a single **unit of work per request**.  
- **IUnitOfWork** wraps this lifecycle to make transactional intent explicit across multiple repository operations when necessary.  
- **Cancellation tokens** propagate through all async APIs to support early termination when clients disconnect or requests timeout, improving responsiveness and resource utilization in ASP.NET Core.

---

## Service Contracts

### IAppointmentAppService
Handles scheduling, rescheduling, cancellation, completion, and queries for appointments.  
Returns rich DTOs optimized for UI consumption and enforces business rules before persisting data.

### IFollowUpAppService
Manages follow-up creation and lifecycle state transitions, maintaining consistency with the appointment aggregate and enabling full auditability.

### IProviderAppService
Recommends providers based on search criteria, computes available slots, and manages provider working hours.  
Bridges scheduling logic with provider calendars.

### IDoctorActionsAppService
Encapsulates doctor-initiated actions such as **Approve**, **Reject**, and **DelayCascade**, ensuring operational decisions remain traceable and auditable.

### INotificationSender
Abstracts outbound email and SMS communication.  
Allows easy swapping or toggling of infrastructure implementations without leaking into the domain logic.

### IContactProvider
Provides read-only contact and name lookups for doctors, patients, and providers, reducing full entity loading and limiting data exposure.

### IAdminScheduleAppService
Aggregates master schedules, computes admin dashboard statistics, and retrieves specialty lists optimized for read performance.

### IAuthenticationService
Validates credentials (phone and password) and returns a `LoginResult`.  
Enforces hashing and lockout policies within implementations.

### IInventoryService
Performs CRUD operations on items and handles stock movements (in/out).  
Transaction boundaries must ensure consistency in stock levels under concurrent updates.

### IPatientDashboardService
Builds the patient dashboard and manages profile updates.  
Returns results with clear success/failure indicators to support UX and validation.

### ISeedStatusRepository
Tracks seed data states to ensure **idempotent** and **versioned** data seeding during migrations and deployments.

### IUnitOfWork
Defines `BeginTransaction`, `Commit`, `Rollback`, and `SaveChanges` methods for explicit transaction control.  
Each request should map to a single, short-lived unit of work.

---

## Domain Repositories

### IAppointmentRepository / IFollowUpRepository
Aggregate-focused repositories supporting domain invariants and core appointment workflows.  
Avoids generic CRUD; emphasizes domain intent.

### IProviderRepository / IDoctorScheduleRepository
Handles provider search, specialty filters, and scheduling templates to support advanced scheduling algorithms.

### IUserRepository / IDoctorRepository / IPatientRepository / IStaffRepository
Manage user and role-based persistence operations, counts, and projections for both admin and user-facing flows.

### IRepository<T> (Generic)
Provides minimal operations (e.g., count).  
Generic repositories are limited in use since DbSet already provides generic data access; over-wrapping EF adds little value.

---

## Typical Workflows

### Schedule Appointment
1. Validate slot availability and provider hours.  
2. Create an `Appointment` domain object via factory.  
3. Persist using the repository.  
4. Commit via `IUnitOfWork`.  
5. Trigger notifications via `INotificationSender` or enqueue in an outbox for background dispatch.

### Reschedule / Cancel / Complete
1. Load the aggregate.  
2. Invoke domain method for state transition.  
3. Persist changes and commit.  
4. Emit side effects (notifications/logs) **after** the transaction to prevent partial effects.

### Follow-Up Lifecycle
Create pending follow-ups, mark complete or canceled with state validation, and schedule reminders via background services that poll due follow-ups.

### Master Schedule
Aggregate provider calendars, doctor schedules, and appointment slices to produce daily views and statistics for administrative dashboards.

---

## Transactions and Unit of Work
- Each request uses one **DbContext instance** as a **unit of work**.  
- Transaction scope via `IUnitOfWork` must remain **short-lived** to avoid long-running locks and connection pinning.  
- Always **commit after invariants pass**; rollback on failure or cancellation.  
- On cancellation, propagate `OperationCanceledException` to maintain proper HTTP semantics.

---

## CancellationToken Policy
- All async methods accept a `CancellationToken`.  
- This allows termination of work when clients disconnect or time out.  
- Optional tokens should appear **only at public API boundaries**; internally, they should always be propagated and respected.  
- Long-running loops should regularly check for cancellation to prevent wasted work.

---

## Repository vs DbContext Trade-offs
- **EF Core guidance:** `DbContext` already acts as a Unit of Work and `DbSet` as a repository.  
- Adding a broad **generic repository** layer can duplicate EF capabilities and obscure EF features (lazy loading, tracking, etc.).  
- **Keep repositories** when they add **domain meaning**, encapsulate invariants, and prevent arbitrary data access.

**Anti-pattern:**  
Generic repositories that expose full CRUD without domain constraints lead to leaky abstractions.  
**Preferred approach:** Focused repositories aligned with domain concepts (Appointment, Provider, Patient, etc.).

---

## API Shape and Return Types
- Use **rich results** (e.g., `Result` or DTO) instead of bare `bool` to expose validation and conflict details.  
- Async methods should follow standard .NET conventions (`Task` and `Async` suffix).  
- Ensure idempotency for command handlers to tolerate retries safely.  
- Maintain consistent return types for all command-style operations.

---

## Improvements

1. Replace all `bool` returns (`Cancel`, `Approve`, etc.) with `Result` objects to convey explicit failure reasons.  
2. Introduce **paging and filtering DTOs** for list endpoints (`GetByProvider`, `GetAllUsers`, etc.) to reduce over-fetching.  
3. Align `IUnitOfWork` lifetime with per-request `DbContext` and document transaction boundaries.  
4. Make read models **cancellation-aware** and **index-friendly** to improve performance.  
5. Avoid N+1 queries by precomputing projections for schedule reads.  
6. Document consistency guarantees for `DelayCascade` and ensure atomicity via transactions.  

---

## Open Questions

1. Should admin schedules and provider slots be **eventually consistent** (via precomputed views) or computed on demand with strict locking for high contention providers?  
2. Where should notifications be sent — **inline after commit** or **asynchronously via background dispatcher** — balancing responsiveness and fault tolerance?  
3. Do inventory operations require **cross-table transactions** to maintain stock invariants under concurrent in/out operations?  

---

# Clinix – Application Layer Mappings

Scope
- This document explains the application mapping layer: how entities are projected to DTOs and how DTOs are translated into new domain entities for creation flows, with a focus on correctness, performance, and maintainability in .NET 9 and EF Core–backed apps [web:109].  

## Design principles

- Prefer simple, explicit, manual mappings to keep control over transformations, avoid reflection overhead, and make complex rules obvious and debuggable in code reviews and interviews [web:100].  
- Use extension methods and static mapper classes per aggregate/feature to keep mapping discoverable, testable, and reusable across controllers and services without introducing runtime dependencies or heavy frameworks [web:101].  
- Project read paths directly from IQueryable with Select expressions where possible to let EF translate to SQL and avoid N+1 or in‑memory materialization before mapping for performance [web:107].  

## What’s mapped here

- Appointment → AppointmentDto/AppointmentSummaryDto
  - ToDto and ToSummaryDto read the owned DateRange (When.Start/End) into flattened Start/End properties for API/UI consumption, aligning with the practice of shaping data with dedicated DTOs [web:109].  
- FollowUp → FollowUpDto
  - ToDto flattens entity fields into a transport shape for API/UI, keeping domain events and behavior out of response models as recommended for DTO boundaries [web:109].  
- CreateDoctorRequest + User → Doctor (+ DoctorSchedule[])
  - CreateFrom composes a new Doctor from a pre‑created User plus optional schedules, centralizing default values and field normalization in a single place for maintainability and testability [web:109].  
- RegisterPatientRequest + User → Patient
  - CreateFrom seeds Patient defaults, timestamps, and a generated MRN, demonstrating explicit manual logic that would be opaque in convention‑only mappers, which aids debugging and audits [web:100].  
- CreateStaffRequest + User → Staff
  - CreateFrom composes Staff from request plus user, keeping mapping logic local and explicit, which aligns with the guidance for simple manual mapping over generic frameworks for clarity [web:100].  
- User.CreateForRole
  - Factory method normalizes FullName/Email/Phone and stamps CreatedBy/At, illustrating the value of deterministic, auditable object construction at system edges before persistence [web:109].  

## Time handling guidance

- For cross‑time‑zone instants like appointments and slots, prefer DateTimeOffset to capture the moment and offset; .NET guidance highlights DateTimeOffset’s suitability for absolute times and time‑zone correctness [web:112].  
- For “now” instants stored in UTC with offset awareness, favor DateTimeOffset.UtcNow to avoid local time ambiguity; this helps align created/updated timestamps with domain entities that already use DateTimeOffset [web:110].  

## Manual mapping vs libraries

- Manual mapping
  - Pros: best performance, full control, simple debugging, no hidden reflection, explicit handling of edge cases and conditionals as confirmed by multiple practical comparisons and guidance [web:100].  
  - Cons: more boilerplate for large graphs and frequent shape changes; requires discipline to keep mapping code DRY and covered by tests as noted in comparative discussions and articles [web:97].  
- AutoMapper / Mapster / Mapperly
  - Pros: reduces repetitive code and centralizes mapping config; can generate efficient code when source and destination are similar, with code‑gen tools like Mapster/Mapperly reducing runtime reflection [web:103].  
  - Cons: reflection and indirection costs (for reflection‑based tools), risk of configuration drift, and harder debugging for complex, conditional transformations noted by practitioners and tutorials [web:100].  

## Performance tips

- Prefer direct projection mappings in queries (e.g., query.Select(e => new Dto { … })) to enable server‑side execution and smaller payloads, which is a commonly recommended approach when mapping with EF queries [web:107].  
- Keep mapping code branch‑light on hot paths, and centralize expensive conversions (e.g., formatting, parsing) at API edges or background preparation to reduce per‑request overhead, matching general DTO guidance for Web APIs [web:109].  

## Reliability and correctness

- Avoid hidden mutations in mapping utilities; mapping should be a pure transformation without side effects to ease testing and parallelization as recommended for DTO boundaries [web:109].  
- Keep domain validation and invariants out of the mapper; validate with DataAnnotations or FluentValidation at the edge and enforce critical invariants in the domain/application services, consistent with DTO best practices [web:109].  

## Decisions and trade‑offs

- Decision: use explicit manual mappings via static classes and extension methods for entity→DTO and DTO→entity creation flows to maximize control and debuggability [web:100].  
- Alternative: use a mapper library (Mapster/Mapperly/AutoMapper) to cut boilerplate on read‑heavy projections; adopt only where performance and transparency remain acceptable and configuration remains close to the code being mapped [web:103].  
- Consequence: manual mapping increases code volume but keeps codegen/reflection out of the hot path and avoids hidden runtime behaviors, which improves predictability under load and simplifies incident debugging [web:100].  

## Improvements

- Standardize on DateTimeOffset for CreatedAt/UpdatedAt at creation sites (e.g., switch DateTime.UtcNow calls to DateTimeOffset.UtcNow) to align with Appointment/FollowUp timing semantics and prevent Kind/offset inconsistencies [web:110].  
- Add expression‑based projection members for common list/detail queries (e.g., AppointmentDto selector) to plug directly into IQueryable.Select for EF translation and better performance on large datasets [web:107].  
- Add unit tests for each mapping to assert field coverage, null/trim normalization, and generated identifiers (MRN) invariants, following DTO creation practices for Web APIs [web:109].  
- Consider a code‑gen mapper (Mapperly/Mapster) for high‑churn DTOs while keeping manual mappings for complex transforms, balancing maintainability with performance and clarity as suggested in recent reviews of mapping tools [web:103].  

## Open questions

- Should the system unify all persisted timestamps on DateTimeOffset for consistency across aggregates and avoid mixed DateTime/DateTimeOffset conversions at boundaries [web:112].  
- Should Doctor/Staff creation mappers accept ProviderId or other associations now, or defer to dedicated workflow steps to keep creation minimal and explicit in audits, following DTO scoping guidance [web:109].  
- Do we want to introduce queryable projection expressions for all read DTOs to standardize EF‑friendly mapping and reduce regression risk in list endpoints as advised in mapping discussions [web:107].  


# Clinix – Application Services

Scope
- This document explains responsibilities and workflows for AdminScheduleAppService, AppointmentAppService, DoctorActionsAppService, FollowUpAppService, ProviderAppService, and PatientDashboardService, with transaction, cancellation, logging, and time‑zone/DST considerations for production readiness [web:80][web:84].  

## Architecture role

- Application services orchestrate domain aggregates, repositories, and cross‑cutting concerns so controllers/pages stay thin and the domain remains infrastructure‑agnostic, aligning with layered/persistence design guidance [web:80].  
- Each service method should align to a request‑scoped unit of work via a short‑lived DbContext; use explicit transactions when multiple aggregates are modified in one operation, following EF Core lifetime/transaction guidance [web:84].  
- CancellationToken flows through all async signatures so work stops when clients disconnect or time out, which is recommended to conserve resources under load in ASP.NET Core [web:85].  

## Services overview

- AdminScheduleAppService: builds master schedule views and admin dashboard stats by composing providers, doctor weekly schedules, and appointments into day views and utilization metrics [web:80].  
- AppointmentAppService: enforces overlap‑free scheduling/rescheduling, and handles cancel/complete with optional follow‑up creation using the Appointment aggregate’s methods [web:80].  
- DoctorActionsAppService: performs doctor operations (approve, reject, cascade delays) by updating appointment schedules within clinic hours and across days as needed [web:80].  
- FollowUpAppService: manages follow‑up lifecycle (create, complete, cancel) and projections, persisting child entities and updating parent appointment for consistency [web:80].  
- ProviderAppService: returns provider recommendations and available time slots per day, and updates provider working hours as administrative maintenance [web:80].  
- PatientDashboardService: composes a patient dashboard DTO from user/patient stores and updates profile data within a transaction [web:84].  

## Key workflows

- Master schedule (GetMasterScheduleAsync): filter providers by specialty/provider/doctor, iterate date range, pick an on‑duty doctor per provider, load doctor day schedule, compute slots at 30‑minute steps, label Booked if any overlap, otherwise Available unless ShowOnlyAvailable is true, then aggregate utilization and return ordered results [web:80].  
- Dashboard stats (GetDashboardStatsAsync): scan providers, gather day appointments, compute pending/completed/no‑show counts, derive total possible slots from doctor weekly schedules with 30‑minute granularity, and calculate utilization and availability [web:80].  
- Schedule/Reschedule: check Start < End, fetch overlapping provider appointments and deny conflicts, then create or reschedule using Appointment methods and persist [web:80].  
- Cancel/Complete: load aggregate, execute state transition, and persist; Complete additionally creates an automatic follow‑up window in 48 hours, which downstream jobs can use for reminders [web:80].  
- Doctor delay cascade: find all later appointments that day for the provider, shift each by accumulated delay while preventing overlaps, and spill to next day within provider hours when running out of time [web:80].  
- Patient profile update: validate uniqueness for email, create or update Patient record, recompute IsProfileCompleted, wrap changes in a transaction, and log outcome [web:84].  

## Time and time‑zone guidance

- Use DateTimeOffset for appointment instants and slot boundaries so the absolute moment is preserved across time zones and DST transitions, which .NET recommends for scheduling domains [web:112].  
- Prefer DateTimeOffset.UtcNow for created/updated stamps in services for consistency with offset‑aware domain timestamps and to avoid local time ambiguity at boundaries [web:110].  
- Beware DST when generating “every 30 minutes” slices; repeated or skipped times occur on transition days, so schedules should use a consistent zone and rules or normalize in UTC to avoid gaps/overlaps during DST switches [web:123][web:125].  
- If using DateOnly for calendar filters, ensure model binding/serialization support is configured in ASP.NET Core to avoid binding errors and inconsistent formats in APIs [web:65].  

## Logging and observability

- Replace Console.WriteLine with the built‑in ILogger to produce structured logs routed through configured providers (Console, Debug, EventSource, Application Insights, etc.) and tuned via appsettings Logging configuration [web:116][web:119].  
- For hot paths or high‑volume logs, prefer the high‑performance LoggerMessage pattern and avoid unnecessary string interpolation to reduce allocations and overhead [web:128].  
- Consider enabling HTTP logging selectively for diagnostics of request/response behavior in admin endpoints where throughput allows [web:130].  

## Transactions, unit of work, and cancellation

- Map BeginTransaction/Commit/Rollback to a single request‑scoped DbContext and ensure all repository calls for a workflow are part of the same unit; avoid long transactions that span user interactions to prevent connection pinning and deadlocks [web:84].  
- Always pass and honor CancellationToken into repository/EF calls, and check it in loops like slot generation or delay cascades to avoid wasting CPU after client cancelation [web:85].  

## Design observations and trade‑offs

- Provider‑level vs doctor‑level scheduling: appointments reference ProviderId while daily schedules are doctor‑specific; current logic picks the first active/on‑duty doctor for a provider per day view, which simplifies views but can reduce fidelity when multiple doctors serve the same provider concurrently [web:80].  
- Fixed 30‑minute slotting: using a constant step is simple to reason about but assumes uniform durations; if procedures vary, utilization and availability derived from counts may be inaccurate relative to total minutes [web:80].  
- Inline side effects vs outbox: services currently update state synchronously; consider a transactional outbox and background dispatch for notifications to avoid dual‑write risks when integrating downstream channels [web:80].  

## Known gaps and fixes

- Approve() doesn’t update status: the Appointment.Approve method updates UpdatedAt only; update the method to set Status to Confirmed (or the chosen approved state) so downstream views and filters reflect approval [web:80].  
- ShowOnlyAvailable logic: BuildTimeSlots skips adding Available slots when ShowOnlyAvailable is true, which ends up returning Booked‑only views; invert the check to include only Available when requested [web:80].  
- Cascade delay atomicity: DelayCascade updates each appointment as it iterates without an explicit transaction, which risks partial updates on failure; wrap the batch in a transaction and surface a Result if the cascade cannot be applied atomically [web:84].  
- Time‑zone correctness: constructing DateTimeOffset with DateTimeOffset.Now.Offset ties slot times to the server’s local zone, which may not match provider or clinic zones; derive offsets from provider time zones or normalize to UTC consistently to avoid DST pitfalls [web:112][web:123].  
- Stats accuracy: AdminScheduleStats uses appointment counts as bookedSlots, which mismatches utilization when durations are not uniform; compute booked minutes from actual appointment ranges for precise utilization [web:80].  
- Logging hygiene: ProviderAppService uses Console.WriteLine; switch to ILogger with categories and scopes for correlation and production routing via providers [web:116][web:119].  
- Transaction rollback path: PatientDashboardService rolls back only on DbUpdateException; ensure rollback also occurs on unexpected exceptions to avoid in‑doubt transactions [web:84].  

## Improvements and roadmap

- Standardize Result returns for command methods (Approve/Reject/Cancel/Complete/UpdateWorkingHours) so user feedback includes reasons (conflicts, invalid state, concurrency) and can map to ProblemDetails [web:80].  
- Add concurrency tokens (rowversion) to Appointment to prevent lost updates during cascading delays or concurrent reschedules, aligning with EF concurrency practices applied elsewhere in the model [web:84].  
- Introduce time‑zone strategy per provider (IANA/Windows zone stored on Provider) and convert slot generation to that zone or to UTC with display conversion to handle DST deterministically [web:112].  
- Add cancellation checks in long loops (slot generation, cascade) and early‑exit when ct.IsCancellationRequested is signaled to reduce wasted work under load [web:85].  
- Replace fixed slot length with provider/doctor configurable slot durations and compute utilization by minutes rather than counts to better reflect real capacity [web:80].  
- Switch provider recommendation logs to ILogger and remove PII from logs; instrument critical workflows with structured events and correlation IDs for incident triage [web:116][web:118].  

## Open questions

- Should appointments bind to a specific DoctorId instead of only ProviderId to align with doctor schedules, availability, and approvals, and reduce ambiguity in multi‑doctor providers [web:80].  
- Should admin schedule prefer all eligible doctors per provider/day rather than a single on‑duty pick to present a fuller capacity picture [web:80].  
- What is the canonical time zone for scheduling and reporting, and how should DST transitions be represented to users on affected days (skip/duplicate hours policy) [web:112][web:123].  


# Clinix – Registration & User Management Services

Scope
- This document explains the registration and user‑management workflows implemented by RegistrationService and UserManagementService, with notes on security (password hashing), phone normalization, transactions, soft delete, logging, and production‑ready improvements for .NET 9 + EF Core + Blazor Server [memory:21][memory:22].  

## Services and responsibilities

- RegistrationService
  - Registers patients, staff, and doctors by creating a User with a role, hashing the password, and creating the corresponding profile (Patient/Staff/Doctor), including provider creation and specialty tags for doctors [memory:22][web:140].  
  - Completes patient profile after login by creating Patient data and marking the User profile as completed in a transaction for atomicity [memory:22][web:84].  
  - Normalizes phone numbers before uniqueness checks and storage, to reduce duplicates from format variations and to align with E.164 practices for downstream SMS and integrations [memory:22][web:153].  

- UserManagementService
  - Provides admin stats (counts by role, active, profile‑completed), list with role‑specific projections, per‑user detail, update, soft delete, and reactivation, using repositories and a unit of work for consistency [memory:22][web:84].  

## Key workflows

- RegisterPatientAsync
  - Validate input, normalize phone, ensure uniqueness, instantiate User for role “Patient”, hash password using ASP.NET Core Identity’s PasswordHasher<TUser>, save to get User.Id, create Patient row, commit, and log an informational audit event [web:140][web:137].  

- CreateStaffAsync
  - Validate input and phone, ensure uniqueness, create User with “Staff”, hash password, create Staff from DTO, commit as a unit to avoid partial creation, and log the action [web:84][web:140].  

- CreateDoctorAsync
  - Validate input and phone, ensure uniqueness, create User with “Doctor”, hash password, create Doctor (with optional schedules), create Provider from the doctor’s specialty and schedule window, assign ProviderId to the doctor, and commit [web:140][web:84].  

- CompletePatientProfileAsync
  - Guard against duplicate completion, create Patient record with medical profile fields, mark User.IsProfileCompleted = true, update timestamps, wrap in a transaction and log success or failures [web:84][web:116].  

- User stats, list, detail, update, delete, reactivate
  - Aggregate in‑memory counts for stats, compose list projections from users/doctors/patients, fetch role‑specific detail, update basic user attributes inside a transaction, soft delete non‑admins, and reactivate by flipping IsDeleted flags and auditing via ILogger [web:84][web:116].  

## Security and data handling

- Password hashing
  - Uses ASP.NET Core Identity PasswordHasher<TUser>, which implements the standard Identity hashing pipeline, typically PBKDF2 by default, and supports rehash‑when‑parameters‑change semantics for forward security upgrades [web:137][web:140].  

- Phone normalization
  - The current normalizer strips punctuation and preserves a leading plus, but E.164 requires country code and up to 15 digits, so prefer a library like libphonenumber‑csharp to parse/format as E.164 and apply a default region when “+” is missing to avoid duplicates and delivery issues [web:151][web:148].  

- Soft delete
  - DeleteUserAsync treats deletes as “soft”, so enforce a global query filter on IsDeleted in EF Core to exclude soft‑deleted rows from queries by default, and use IgnoreQueryFilters when necessary (e.g., audits) [web:150][web:141].  

## Transactions, unit of work, and cancellation

- Each command method that modifies data begins a transaction, writes related entities, commits on success, and rolls back on failure, mapping to a request‑scoped DbContext lifetime and EF Core’s unit‑of‑work guidance [web:84][web:81].  
- CancellationToken should be propagated through repository calls so upstream cancellations (client disconnects/timeouts) can stop work promptly and free resources in ASP.NET Core [web:85][web:84].  

## Logging and observability

- Prefer ILogger with structured messages over console output, configure sinks via appsettings, and add correlation info/scopes for multi‑step flows like registration and profile completion to improve diagnostics [web:116][web:118].  
- Use high‑performance logging patterns (e.g., LoggerMessage) for hot paths or high‑volume logs to minimize allocations and overhead [web:128].  

## Design decisions and trade‑offs

- Identity hashing vs custom hashing
  - Decision: use built‑in PasswordHasher<TUser> for well‑vetted hashing behavior and automatic rehash on policy changes; avoids custom crypto mistakes and integrates cleanly with Identity pipelines [web:137][web:139].  
- Soft delete strategy
  - Decision: use IsDeleted + EF Core global query filters to preserve audit and enable reactivation, balancing simplicity with the need for occasional IgnoreQueryFilters access in admin/reporting scenarios [web:150][web:144].  
- Phone format policy
  - Decision: normalize and store E.164 to reduce duplicates, ease uniqueness checks, and interoperate with providers (e.g., Twilio expecting E.164), while requiring a default region or explicit country code on input [web:153][web:151].  

## Known gaps and fixes

- UpdateUserAsync uniqueness
  - Add checks for email and phone uniqueness (similar to registration) to avoid conflicts and ensure consistent invariants in admin edits [web:80][web:84].  
- Transaction rollback coverage
  - Ensure rollback in a finally or broader catch for all exceptions, not just DbUpdateException, to avoid lingering in‑doubt transactions under unexpected faults [web:84][web:116].  
- Provider creation coupling
  - CreateDoctorAsync creates a Provider named after the doctor and fills tags from specialty, which may duplicate providers or misrepresent clinic vs practitioner; clarify the model and consider selecting an existing Provider instead [web:80][web:84].  
- Timestamp consistency
  - Prefer DateTimeOffset.UtcNow for created/updated stamps to align with other offset‑aware entities and avoid local time ambiguity and DST issues in audit trails [web:110][web:112].  
- Query efficiency in GetAllUsersAsync
  - Joining separate user/doctor/patient lists in memory can be heavy; prefer a repository projection (GetAllWithRoleDetailsAsync) or server‑side projections to reduce N+1 and payload size [web:80][web:84].  

## Improvements and roadmap

- Consistent Result return types
  - Replace bool returns in admin/user flows with Result to surface explicit failure reasons (uniqueness, policy, concurrency), making UIs and logs clearer [web:80][web:84].  
- Enforce soft delete globally
  - Add HasQueryFilter(e => !e.IsDeleted) for User and related aggregates, and document how to bypass for audits and reactivation workflows [web:150][web:141].  
- Harden phone strategy
  - Use libphonenumber‑csharp to parse/format/validate with a default region and store normalized E.164; reject invalid numbers and re‑normalize on updates [web:148][web:151].  
- Strengthen auditing
  - Add CreatedBy/UpdatedBy and timestamps consistently across writes, aligned with ILogger logs, and consider event‑based outbox for notification or audit streams [web:116][web:118].  
- Validation depth
  - Keep DataAnnotations for syntax and add FluentValidation for richer business rules (e.g., role change policies), with consistent ProblemDetails mapping at the API edge [web:80][web:65].  

## Open questions

- Should providers be independent clinic/department entities selected during doctor creation, rather than created per doctor, to avoid fragmentation and reflect real organizational structure [web:80][web:84]?  
- What is the canonical time zone policy for auditing and scheduling, and should services enforce UTC with offset for storage and convert for display only to avoid DST pitfalls [web:112][web:125]?  
- Should admin updates support email verification flows and phone OTP re‑verification when changing contact data to protect account integrity and notifications [web:80][web:84]?  

# Clinix – Background Services, Outbox, and Notifications

## Scope
This document explains how **background workers**, **domain event dispatch**, the **outbox processor**, and **notification handlers** collaborate to deliver reliable reminders and appointment notifications.  

It also documents **scheduling cadence**, **graceful shutdown**, **transactional guarantees**, **logging**, **configuration**, and **operational safety** considerations before production deployment.

---

## Components

### FollowUpReminderWorker
Runs periodically to find due follow-ups and send email/SMS reminders using:
- Repository reads  
- IContactProvider for contact details  
- INotificationSender for delivery  

### OutboxProcessorWorker
Polls the `OutboxMessages` table and invokes handlers to deliver event-driven notifications, ensuring **reliable asynchronous delivery** through the **transactional outbox pattern**.

### DomainEventDispatcher
Extracts domain events from tracked entities and writes them to `OutboxMessages` **before EF Core SaveChanges**, guaranteeing atomic persistence and event capture.

### NotificationHandlers
Translate domain events into **email/SMS notifications** for patients and doctors, fully decoupled from domain logic.

### RealNotificationSender
Sends real emails via SMTP in production and logs SMS messages.  
Optional Twilio integration is enabled through configuration for real SMS delivery.

---

## End-to-End Workflow

1. A command (e.g., *schedule appointment*) raises a **domain event** inside the aggregate.  
2. `DomainEventDispatcher` serializes and stores the event in the **OutboxMessages** table as part of the same transaction.  
3. `OutboxProcessorWorker` wakes on a `PeriodicTimer`, fetches pending messages, and invokes handlers.  
4. `NotificationHandlers` send notifications through `INotificationSender`.  
5. The worker updates attempt counts and marks messages as **processed or failed** atomically.  
6. `FollowUpReminderWorker` runs independently to scan and send patient reminders.

---

## Background Workers

- Both workers inherit from **BackgroundService** and use **PeriodicTimer** inside `ExecuteAsync` to perform periodic work.  
- Each iteration creates a **new DI scope** via `IServiceScopeFactory`, ensuring scoped services and DbContexts are properly disposed.  
- **CancellationToken** is passed through all async operations and `WaitForNextTickAsync` calls for **graceful shutdown** and cooperative cancellation.

---

## Graceful Shutdown

- ASP.NET Core calls `StopAsync()` on hosted services during shutdown, propagating a **shared cancellation token**.  
- Code should observe this token to **complete or abort** work promptly.  
- Use `HostOptions.ShutdownTimeout` to extend shutdown time if batches need more time to complete.  
- After the timeout, any unfinished work should be **abandoned** safely.

---

## Transactional Outbox

- Ensures **database updates** and **event publishing** happen **atomically**.  
- `OutboxProcessorWorker` polls unprocessed messages, invokes handlers, and marks success or failure after retries.  
- Guarantees **at-least-once delivery** to avoid inconsistencies between database and message systems.  
- Prevents **dual-write problems** where writes succeed but notifications fail.

---

## Notifications Pipeline

- `NotificationHandlers` process domain events like:
  - AppointmentScheduled  
  - AppointmentCancelled  
  - FollowUpCreated  

- Handlers:
  - Load contextual data  
  - Format templates  
  - Send via `INotificationSender`  

- Failed deliveries:
  - Retries handled by the **Outbox Processor**  
  - Max attempts → mark as **terminal** for operator review  

---

## Email/SMS Delivery

- **RealNotificationSender**
  - Uses **SMTP** for emails when notifications are enabled.
  - Logs full message content in development mode.  
  - Uses **ILogger** for structured logging.  
- **SMS**
  - Logs message details by default for visibility.  
  - Twilio integration can be toggled for production environments.

---

## Configuration and Options

- Background cadences, look-ahead windows, and notification toggles are injected using **IOptions** from `appsettings.json`.  
- Validate SMTP and Twilio configuration **at startup** for fail-fast behavior.  
- Use **environment-specific** configurations and **secret stores** to safely manage production credentials.  

---

## Observability

- Use **structured logging** with `ILogger` across workers and handlers.  
- Prefer **LoggerMessage patterns** for high-volume logs to minimize allocations.  
- Log:
  - Batch sizes  
  - Attempt counts  
  - Message types  
  - Correlation IDs  

- Recommended metrics:
  - Processed / failed message counts  
  - Retry rates  
  - Queue lag  

---

## Decisions and Trade-offs

- **PeriodicTimer** replaces legacy timers for cleaner async and cancellation handling.  
- **Polling** with retries is simpler and reliable; message brokers can reduce latency but increase complexity.  
- **Feature toggles** enable safe dev environments and consistent abstractions for notification delivery.

---

## Known Gaps and Fixes

- Use `DateTimeOffset.UtcNow` consistently to avoid timezone and DST issues.  
- Add **backoff and jitter** to outbox polling to prevent thundering herd effects.  
- Persist **dead-lettered messages** for failed deliveries instead of silently marking them as processed.  

---

## Improvements and Roadmap

- Add **per-tenant / per-provider timezone handling** for reminders.  
- **Parallelize outbox batch processing** with bounded concurrency and idempotent handlers.  
- Export **notification outcomes to metrics and traces** for observability.  
- Add **health checks** for worker liveness and backlog readiness.

---

## Open Questions

- Should outbox retries adopt **exponential backoff** and a **dead-letter queue** for failed messages?  
- What is the acceptable **shutdown timeout** to safely finish batches during host stop?  
- Should Follow-Up reminders respect **provider clinic hours** to avoid inappropriate send times?

---

# Repository Query Review (EF Core + SQL Server)

This document reviews current repository queries and provides high‑impact, production‑grade improvements for performance, correctness, and maintainability.  
The focus areas are server‑side filtering, tracking policy, unit‑of‑work boundaries, compiled/split queries, case‑insensitive search/collations, and indexing for time‑range scans.  

## Executive summary

- Push all filtering/search to the database and replace in‑memory scans to reduce memory pressure and improve selectivity and latency.  
- Standardize a “no‑tracking by default” read policy and enable tracking only when updating aggregates to cut CPU/memory overhead.  
- Centralize SaveChanges at the unit‑of‑work boundary so multi‑aggregate operations are atomic and round‑trips are minimized.  
- Apply compiled queries on hot read paths and use split queries when Include graphs risk row explosion, validating with measurements.  
- Make case‑insensitive search explicit via collations or full‑text search, avoiding client‑side ToLower/Contains patterns.  
- Add composite/covering indexes aligned to equality‑then‑range predicates (e.g., ProviderId then Start) for time‑window queries.  

## Cross‑cutting improvements

- **SaveChanges placement:** Repositories attach/track entities and let the application service or Unit of Work call SaveChanges/Commit once per request to preserve transactional integrity.  
- **Read tracking strategy:** Use AsNoTracking for read‑only queries and opt‑in tracking for updates to reduce EF Core change tracker overhead.  
- **Efficient querying:** Project in the database with IQueryable.Select to return only necessary fields and avoid post‑materialization filtering or joins.  
- **Compiled queries:** Precompile frequently executed, parameterized queries (e.g., appointment time ranges) to reduce repeated query translation overhead.  
- **Split queries:** When multiple Includes create large Cartesian results, prefer AsSplitQuery to reduce duplicated rows and memory pressure.  
- **Case‑insensitive search:** Prefer database/column collations or EF.Functions.Collate and consider SQL Server Full‑Text Search for keyword scenarios.  
- **Logging:** Replace Console writes in repositories with the built‑in logger so logs are structured and routed via configured providers.  
- **Cancellation:** Propagate CancellationToken through all async EF calls to support scalable request cancellation.  

## Repository reviews and actions

### AppointmentRepository

- Reads: Add AsNoTracking to GetByPatientAsync and GetByProviderAsync since these are list queries and do not modify state.  
- Hot path optimization: Consider EF.CompileAsyncQuery for GetByProviderAsync (range by ProviderId/Start/End) after profiling.  
- Indexing: Add a nonclustered index on (ProviderId, Start) and consider including End/Status/Type to cover list projections and accelerate range seeks.  
- Commit policy: Remove SaveChanges from AddAsync/UpdateAsync and commit in the service/UoW to keep multi‑entity changes atomic.  

### DoctorRepository

- Reads: Retain AsNoTracking in GetByUserIdAsync/GetByProviderIdAsync/GetAllAsync to minimize tracker overhead for admin views.  
- Large graphs: If deeper Include chains are added, validate AsSplitQuery to prevent row explosion in wide joins.  
- Commit policy: Continue avoiding SaveChanges here and let the coordinator commit once per operation.  

### DoctorScheduleRepository

- Reads: AsNoTracking is correct for schedule lookups by doctor/day/provider and supports high‑read workloads.  
- Indexing: Add (DoctorId, DayOfWeek) and (ProviderId, DayOfWeek) indexes for day‑view queries to minimize scans.  
- Commit policy: Remove SaveChanges from AddRangeAsync/UpdateAsync for consistency with the unit‑of‑work pattern.  

### FollowUpRepository

- Reads: Add AsNoTracking to GetByIdAsync/GetByAppointmentAsync/GetPendingDueAsync, especially valuable in background reminder scans.  
- Indexing: Ensure the (Status, DueBy) index exists to serve due‑by scans efficiently.  
- Commit policy: Remove SaveChanges in Add/Update and rely on the transaction boundary in services.  

### PatientRepository

- Reads: Add AsNoTracking to GetAllAsync/GetAllPatientsAsync and pass CancellationToken everywhere for scalability.  
- Admin views: Prefer server‑side projections that shape DTOs in a single query to avoid in‑memory joins across lists.  
- Commit policy: Keep SaveChanges at the UoW to batch with related writes and preserve consistency.  

### ProviderRepository

- Eliminate in‑memory filtering: Rewrite SearchAsync to use database predicates with EF.Functions.Like and explicit CI collation via EF.Functions.Collate.  
- At scale: Consider SQL Server Full‑Text Search (CONTAINS/FREETEXT) via FromSql for better tokenization and ranking across Name/Specialty/Tags.  
- Logging: Replace Console.WriteLine with the logger to integrate with structured providers and environment‑level filtering.  

### UserRepository

- Case‑insensitive email: Avoid ToLower in LINQ; rely on CI collations at DB/column level or use EF.Functions.Collate in predicates for sargability.  
- Soft delete: Maintain IsDeleted filters and consider a global query filter to consistently exclude soft‑deleted rows by default.  
- Lists: Project to lightweight DTOs directly in queries for admin lists to reduce materialization and network I/O.  

## Index recommendations (SQL Server)

- **Appointments:** Nonclustered index on (ProviderId ASC, Start ASC) INCLUDE (End, Status, Type).  
- **FollowUps:** Nonclustered index on (Status ASC, DueBy ASC).  
- **DoctorSchedules:** Nonclustered indexes on (DoctorId, DayOfWeek) and (ProviderId, DayOfWeek).  
- **Users:** Index on (Role, CreatedAt DESC) and collations on Email/Phone aligned to lookup behavior.  

## Example rewrites

### ProviderRepository.SearchAsync — database‑side LIKE with explicit collation
```csharp
public async Task<List<Provider>> SearchAsync(string[] keywords, CancellationToken ct = default)
{
    var q = _db.Providers.AsNoTracking();

    if (keywords is { Length: > 0 })
    {
        foreach (var kw in keywords)
        {
            var pat = $"%{kw}%";
            q = q.Where(p =>
                EF.Functions.Like(EF.Functions.Collate(p.Name, "SQL_Latin1_General_CP1_CI_AS"), pat) ||
                EF.Functions.Like(EF.Functions.Collate(p.Specialty, "SQL_Latin1_General_CP1_CI_AS"), pat) ||
                EF.Functions.Like(EF.Functions.Collate(p.Tags, "SQL_Latin1_General_CP1_CI_AS"), pat));
        }
    }

    return await q.OrderBy(p => p.Name).ToListAsync(ct);
}
ProviderRepository.SearchAsync — full‑text search (optional)
csharp
Always show details

Copy code
public async Task<List<Provider>> SearchAsync(string[] keywords, CancellationToken ct = default)
{
    if (keywords == null || keywords.Length == 0)
        return await _db.Providers.AsNoTracking().OrderBy(p => p.Name).ToListAsync(ct);

    var phrase = string.Join(" OR ", keywords.Select(k => $"\"{k}\""));
    var sql = "SELECT * FROM Providers WHERE CONTAINS((Name, Specialty, Tags), {0}) ORDER BY Name";
    return await _db.Providers.FromSqlRaw(sql, phrase).AsNoTracking().ToListAsync(ct);
}
AppointmentRepository — no‑tracking and compiled query
csharp
Always show details

Copy code
private static readonly Func<ClinixDbContext, long, DateTimeOffset, DateTimeOffset, IAsyncEnumerable<Appointment>>
    s_getByProviderRange =
        EF.CompileAsyncQuery((ClinixDbContext db, long providerId, DateTimeOffset from, DateTimeOffset to) =>
            db.Appointments
              .AsNoTracking()
              .Where(a => a.ProviderId == providerId && a.When.Start < to && a.When.End > from)
              .OrderBy(a => a.When.Start));

public async Task<List<Appointment>> GetByProviderAsync(long providerId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default)
{
    var list = new List<Appointment>();
    await foreach (var a in s_getByProviderRange(_db, providerId, from, to).WithCancellation(ct))
        list.Add(a);
    return list;
}
Split queries for wide Include graphs
csharp
Always show details

Copy code
var doctor = await _db.Doctors
    .AsSplitQuery()
    .Include(d => d.User)
    .Include(d => d.Schedules)
    .FirstOrDefaultAsync(d => d.UserId == userId, ct);
Commit pattern (Unit of Work)
Repositories should not call SaveChanges; instead, the application layer or Unit of Work coordinates a single SaveChanges/Commit for all related operations to ensure atomicity and better performance.

Case‑insensitive email/phone strategy
Prefer column/database CI collations for Email/Phone to keep predicates index‑friendly, or use EF.Functions.Collate in specific queries when mixed collations are unavoidable.

Tracking identity resolution (optional)
When returning graphs without tracking but needing single instances per key, consider AsNoTrackingWithIdentityResolution and validate overhead vs benefit.

What to measure next
Compare latency and allocations before/after AsNoTracking on list queries.

Benchmark compiled queries on the appointment time‑range path and split queries on Include‑heavy graphs to justify complexity.

Implementation checklist
Add AsNoTracking to read‑only queries and propagate CancellationToken consistently.

Remove SaveChanges from repositories and commit once per request in services/UoW.

Rewrite ProviderRepository search to SQL predicates or FTS and replace Console with ILogger.

Add composite/covering indexes for appointment ranges and day‑based schedule lookups.

Use compiled queries for hot paths and AsSplitQuery where wide Includes cause row duplication.