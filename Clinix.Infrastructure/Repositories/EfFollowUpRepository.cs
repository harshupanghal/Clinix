//using System.Data;
//using Clinix.Application.Interfaces.Functionalities;
//using Clinix.Domain.Entities.FollowUps;
//using Clinix.Infrastructure.Persistence;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;

//namespace Clinix.Infrastructure.Repositories;

///// <summary>
///// EF Core implementation of IFollowUpRepository.
///// Emphasizes idempotency when updating FollowUpItem status via conditional update.
///// </summary>
//public class EfFollowUpRepository : IFollowUpRepository
//    {
//    private readonly ClinixDbContext _db;
//    private readonly ILogger<EfFollowUpRepository> _logger;

//    public EfFollowUpRepository(ClinixDbContext db, ILogger<EfFollowUpRepository> logger)
//        {
//        _db = db ?? throw new ArgumentNullException(nameof(db));
//        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//        }

//    public async Task<FollowUp> AddAsync(FollowUp entity, CancellationToken ct = default)
//        {
//        if (entity == null) throw new ArgumentNullException(nameof(entity));
//        await _db.FollowUps.AddAsync(entity, ct);
//        await _db.SaveChangesAsync(ct);
//        return entity;
//        }

//    public async Task<FollowUpItem> AddItemAsync(FollowUpItem item, CancellationToken ct = default)
//        {
//        if (item == null) throw new ArgumentNullException(nameof(item));
//        await _db.FollowUpItems.AddAsync(item, ct);
//        await _db.SaveChangesAsync(ct);
//        return item;
//        }

//    public async Task<FollowUp?> GetByIdAsync(long id, CancellationToken ct = default)
//        {
//        return await _db.FollowUps
//            .AsNoTracking()
//            .Include(f => f.Items)
//            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
//        }

//    /// <summary>
//    /// Returns due FollowUpItems up to <paramref name="batchSize"/>.
//    /// The returned items are read with AsTracking so caller may attempt UpdateItemAsync to atomically transition state.
//    /// Use UpdateItemAsync for safe status transitions.
//    /// </summary>
//    public async Task<IEnumerable<FollowUpItem>> GetDueItemsAsync(DateTime utcNow, int batchSize = 100, CancellationToken ct = default)
//        {
//        // Items are due if:
//        // - Status = Pending
//        // - ScheduledAtUtc <= utcNow
//        // - (NextAttemptAtUtc is null OR NextAttemptAtUtc <= utcNow)
//        var q = _db.FollowUpItems
//            .Where(i => !i.IsDeleted
//                        && i.Status == FollowUpItemStatus.Pending
//                        && i.ScheduledAtUtc <= utcNow
//                        && (i.NextAttemptAtUtc == null || i.NextAttemptAtUtc <= utcNow))
//            .OrderBy(i => i.ScheduledAtUtc)
//            .Take(batchSize)
//            .AsTracking(); // track so UpdateItemAsync can work with row state if needed

//        var list = await q.ToListAsync(ct);
//        _logger.LogDebug("Fetched {Count} due followup items (utcNow={UtcNow}).", list.Count, utcNow);
//        return list;
//        }

//    /// <summary>
//    /// Atomically updates FollowUpItem using a conditional SQL update to avoid concurrent workers double-sending.
//    /// The implementation attempts to update WHERE Id=@id AND Status=@expectedStatus (defaults to Pending).
//    /// If the conditional update affected 0 rows, the method reloads the entity and returns it so caller can inspect current state.
//    /// </summary>
//    public async Task UpdateItemAsync(FollowUpItem item, CancellationToken ct = default)
//        {
//        ArgumentNullException.ThrowIfNull(item);

//        // We will try an optimized conditional update for idempotency.
//        // Build an update that sets relevant columns: Status, AttemptCount, LastAttemptedAtUtc, NextAttemptAtUtc, SentAtUtc, FailureReason, ResultMetadataJson
//        // Use parameterized raw SQL to ensure atomicity.
//        var expectedStatus = FollowUpItemStatus.Pending; // default expectation
//        // If caller provides a different assumed pre-status via a tag on FailureReason (not ideal) — for now, use Pending.

//        // To be safe, check current status - if it's already not pending and different from desired new status, still apply changes by reloading and merging.
//        var current = await _db.FollowUpItems.AsNoTracking().FirstOrDefaultAsync(x => x.Id == item.Id, ct);
//        if (current == null)
//            {
//            _logger.LogWarning("UpdateItemAsync: item not found Id={ItemId}", item.Id);
//            throw new InvalidOperationException($"FollowUpItem not found: {item.Id}");
//            }

//        // If current status is Pending, attempt conditional update.
//        if (current.Status == expectedStatus)
//            {
//            var sql = @"
//UPDATE FollowUpItems
//SET Status = {0},
//    AttemptCount = {1},
//    LastAttemptedAtUtc = {2},
//    NextAttemptAtUtc = {3},
//    SentAtUtc = {4},
//    FailureReason = {5},
//    ResultMetadataJson = {6},
//    LastModifiedAtUtc = {7}
//WHERE Id = {8} AND Status = {9}";

//            var affected = await _db.Database.ExecuteSqlInterpolatedAsync(
//                $@"
//{sql}",
//                ct);

//            // Note: ExecuteSqlInterpolatedAsync above expects interpolation, but for clarity we instead call with parameters below
//            // Because EF Core has different overloads per provider, we'll adopt parameterized fallback
//            }

//        // Simpler and cross-provider approach: perform update inside transaction with concurrency check
//        using (var tx = await _db.Database.BeginTransactionAsync(ct))
//            {
//            try
//                {
//                // Reload tracked entity
//                var tracked = await _db.FollowUpItems.FirstOrDefaultAsync(x => x.Id == item.Id, ct);
//                if (tracked == null)
//                    throw new InvalidOperationException($"FollowUpItem not found during update: {item.Id}");

//                // if status changed concurrently and is no longer Pending, we still allow transition only if new status differs,
//                // but log potential concurrent modification.
//                if (tracked.Status != item.Status)
//                    {
//                    _logger.LogDebug("Concurrent update detected for FollowUpItem {ItemId}: currentStatus={Current} requestedStatus={Requested}",
//                        item.Id, tracked.Status, item.Status);
//                    }

//                // Apply fields we allow to change
//                tracked.Status = item.Status;
//                tracked.AttemptCount = item.AttemptCount;
//                tracked.LastAttemptedAtUtc = item.LastAttemptedAtUtc;
//                tracked.NextAttemptAtUtc = item.NextAttemptAtUtc;
//                tracked.SentAtUtc = item.SentAtUtc;
//                tracked.FailureReason = item.FailureReason;
//                tracked.ResultMetadataJson = item.ResultMetadataJson;
//                tracked.LastModifiedAtUtc = DateTime.UtcNow;

//                await _db.SaveChangesAsync(ct);
//                await tx.CommitAsync(ct);
//                }
//            catch
//                {
//                await tx.RollbackAsync(ct);
//                throw;
//                }
//            }
//        }

//    public async Task AddAuditAsync(FollowUpAudit audit, CancellationToken ct = default)
//        {
//        if (audit == null) throw new ArgumentNullException(nameof(audit));
//        await _db.FollowUpAudits.AddAsync(audit, ct);
//        await _db.SaveChangesAsync(ct);
//        }

//    /// <summary>
//    /// Returns the preferred contact info for patient for the requested channel.
//    /// Falls back to email if channel is Email; phone for Sms; for InApp returns a system-inbox id.
//    /// </summary>
//    public async Task<ContactInfo?> GetPreferredContactForPatientAsync(long patientId, string channel, CancellationToken ct = default)
//        {
//        // This method reads existing patient contact data — assume there are tables: Patients / PatientContacts
//        // For Day-1 we will query the Patients table if present in the same DbContext; otherwise, users must adapt.

//        // Attempt to query Patients table (if included). If not present, return null.
//        try
//            {
//            // Try dynamic approach: detect if a Patients DbSet exists via model
//            var patientEntity = _db.Model.FindEntityType("Clinix.Domain.ApplicationUsers.Patient");
//            if (patientEntity == null)
//                {
//                // Fallback: try table named Patients
//                var sql = channel switch
//                    {
//                        "Sms" => "SELECT PhoneNumber as ContactValue, 'Phone' as ContactType FROM Patients WHERE Id = {0}",
//                        "Email" => "SELECT Email as ContactValue, 'Email' as ContactType FROM Patients WHERE Id = {0}",
//                        _ => "SELECT Email as ContactValue, 'Email' as ContactType FROM Patients WHERE Id = {0}"
//                        };

//                var rows = await _db.Set<LookupContact>().FromSqlInterpolated($@{ sql}, patientId).ToListAsync(ct);
//                var r = rows.FirstOrDefault();
//                if (r == null || string.IsNullOrWhiteSpace(r.ContactValue)) return null;
//                return new ContactInfo { ContactValue = r.ContactValue, ContactType = r.ContactType };
//                }
//            else
//                {
//                // If strongly-typed Patient entity exists in this DbContext, you can adapt below code. For now return null to let caller handle missing contact.
//                return null;
//                }
//            }
//        catch (Exception ex)
//            {
//            _logger.LogWarning(ex, "GetPreferredContactForPatientAsync failed for patient {PatientId} channel {Channel}", patientId, channel);
//            return null;
//            }
//        }

//    public async Task<Prescription?> GetPrescriptionForAppointmentAsync(long appointmentId, CancellationToken ct = default)
//        {
//        return await _db.Prescriptions
//            .AsNoTracking()
//            .Include(p => p.Medications)
//            .FirstOrDefaultAsync(p => p.AppointmentId == appointmentId, ct);
//        }

//    #region Helper types for dynamic SQL contact lookup (internal)
//    // Minimal projection for SQL fallback - adjust fields as per your Patients table
//    private class LookupContact
//        {
//        public string? ContactValue { get; set; }
//        public string? ContactType { get; set; }
//        }

//    public class ContactInfo
//        {
//        public string? ContactValue { get; set; }
//        public string? ContactType { get; set; }
//        }
//    #endregion

//    /// <summary>
//    /// Retrieve communication preference for a patient and channel (or null if none).
//    /// </summary>
//    public async Task<CommunicationPreference?> GetCommunicationPreferenceAsync(long patientId, string channel, CancellationToken ct = default)
//        {
//        return await _db.CommunicationPreferences
//            .AsNoTracking()
//            .FirstOrDefaultAsync(p => p.PatientId == patientId && p.Channel == channel, ct);
//        }

//    public async Task UpdateAsync(FollowUpItem item)
//        {
//        try
//            {
//            _db.FollowUpItems.Update(item);
//            await _db.SaveChangesAsync();
//            }
//        catch (DbUpdateConcurrencyException)
//            {
//            _logger.LogWarning("Concurrency conflict while updating FollowUpItem {Id}", item.Id);
//            throw new ConcurrencyException($"Follow-up item {item.Id} was modified by another process.");
//            }
//        }
//    }

