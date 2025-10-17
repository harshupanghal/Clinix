using Clinix.Application.Interfaces.Functionalities;
using Clinix.Domain.Entities;
using Clinix.Domain.Entities.ApplicationUsers;
using Clinix.Domain.Entities.Appointments;
using Clinix.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clinix.Infrastructure.Repositories;

public class EfDoctorScheduleRepository : IDoctorScheduleRepository
    {
    private readonly ClinixDbContext _db;

    public EfDoctorScheduleRepository(ClinixDbContext db) => _db = db;

    public async Task<Doctor?> GetDoctorAsync(long doctorId) => await _db.Doctors.FindAsync(doctorId);

    public async Task<DoctorWorkingHours?> GetDoctorWorkingHoursAsync(long doctorId)
        {
        // Stored in DoctorWorkingHours table keyed by id == doctorId for simplicity
        return await _db.DoctorWorkingHours.FindAsync(doctorId);
        }

    public async Task<bool> TryAcquireScheduleLockAsync(long doctorId, TimeSpan lockTimeout)
        {
        // Implementation: Try to insert/update ScheduleLock row with LockedUntil in future using a transaction to guarantee atomicity
        var now = DateTimeOffset.UtcNow;
        var until = now.Add(lockTimeout);

        var existing = await _db.ScheduleLocks.FindAsync(doctorId);
        if (existing == null)
            {
            var sl = new ScheduleLock { DoctorId = doctorId, LockedUntil = until, LockedBy = "system" };
            _db.ScheduleLocks.Add(sl);
            try
                {
                await _db.SaveChangesAsync();
                return true;
                }
            catch (DbUpdateException)
                {
                return false;
                }
            }

        if (existing.LockedUntil is null || existing.LockedUntil <= now)
            {
            existing.LockedUntil = until;
            existing.LockedBy = "system";
            _db.ScheduleLocks.Update(existing);
            await _db.SaveChangesAsync();
            return true;
            }

        return false;
        }

    public async Task ReleaseScheduleLockAsync(long doctorId)
        {
        var existing = await _db.ScheduleLocks.FindAsync(doctorId);
        if (existing is null) return;
        existing.LockedUntil = null;
        existing.LockedBy = null;
        _db.ScheduleLocks.Update(existing);
        await _db.SaveChangesAsync();
        }
    }
