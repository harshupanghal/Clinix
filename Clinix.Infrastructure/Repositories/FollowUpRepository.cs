using Clinix.Domain.Entities;
using Clinix.Domain.Enums;
using Clinix.Domain.Interfaces;
using Clinix.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clinix.Infrastructure.Repositories;

public sealed class FollowUpRepository : IFollowUpRepository
    {
    private readonly ClinixDbContext _db;
    public FollowUpRepository(ClinixDbContext db) => _db = db;

    public Task<FollowUp?> GetByIdAsync(long id, CancellationToken ct = default) =>
        _db.FollowUps.FirstOrDefaultAsync(f => f.Id == id, ct);

    public Task<List<FollowUp>> GetByAppointmentAsync(long appointmentId, CancellationToken ct = default) =>
        _db.FollowUps.Where(f => f.AppointmentId == appointmentId).OrderBy(f => f.DueBy).ToListAsync(ct);

    public Task<List<FollowUp>> GetPendingDueAsync(DateTimeOffset upTo, CancellationToken ct = default) =>
        _db.FollowUps.Where(f => f.Status == FollowUpStatus.Pending && f.DueBy <= upTo).OrderBy(f => f.DueBy).ToListAsync(ct);

    public async Task AddAsync(FollowUp f, CancellationToken ct = default)
        { await _db.FollowUps.AddAsync(f, ct); await _db.SaveChangesAsync(ct); }

    public async Task UpdateAsync(FollowUp f, CancellationToken ct = default)
        { _db.FollowUps.Update(f); await _db.SaveChangesAsync(ct); }
    }
