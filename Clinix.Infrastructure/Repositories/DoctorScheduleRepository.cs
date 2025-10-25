// Infrastructure/Repositories/DoctorScheduleRepository.cs
using Clinix.Domain.Entities.ApplicationUsers;
using Clinix.Domain.Interfaces;
using Clinix.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clinix.Infrastructure.Repositories;

public sealed class DoctorScheduleRepository : IDoctorScheduleRepository
    {
    private readonly ClinixDbContext _db;

    public DoctorScheduleRepository(ClinixDbContext db) => _db = db;

    public Task<List<DoctorSchedule>> GetByDoctorAsync(long doctorId, CancellationToken ct = default) =>
        _db.DoctorSchedules
            .Where(s => s.DoctorId == doctorId)
            .OrderBy(s => s.DayOfWeek)
            .AsNoTracking()
            .ToListAsync(ct);

    public Task<DoctorSchedule?> GetByDoctorAndDayAsync(long doctorId, DayOfWeek day, CancellationToken ct = default) =>
        _db.DoctorSchedules
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.DoctorId == doctorId && s.DayOfWeek == day, ct);

    public Task<List<DoctorSchedule>> GetByProviderAndDayAsync(long providerId, DayOfWeek day, CancellationToken ct = default) =>
        _db.DoctorSchedules
            .Include(s => s.Doctor)
            .AsNoTracking()
            .Where(s => s.Doctor.ProviderId == providerId && s.DayOfWeek == day && s.IsAvailable)
            .ToListAsync(ct);

    public async Task AddRangeAsync(List<DoctorSchedule> schedules, CancellationToken ct = default)
        {
        await _db.DoctorSchedules.AddRangeAsync(schedules, ct);
        await _db.SaveChangesAsync(ct);
        }

    public async Task UpdateAsync(DoctorSchedule schedule, CancellationToken ct = default)
        {
        _db.DoctorSchedules.Update(schedule);
        await _db.SaveChangesAsync(ct);
        }
    }
