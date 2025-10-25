// Infrastructure/Persistence/Repositories/AppointmentRepository.cs
using Clinix.Domain.Entities;
using Clinix.Domain.Interfaces;
using Clinix.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clinix.Infrastructure.Repositories;

public sealed class AppointmentRepository : IAppointmentRepository
    {
    private readonly ClinixDbContext _db;
    public AppointmentRepository(ClinixDbContext db) => _db = db;

    public Task<Appointment?> GetByIdAsync(long id, CancellationToken ct = default) =>
        _db.Appointments.Include(a => a.FollowUps).FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<List<Appointment>> GetByPatientAsync(long patientId, CancellationToken ct = default) =>
        _db.Appointments.Where(a => a.PatientId == patientId).OrderBy(a => a.When.Start).ToListAsync(ct);

    public Task<List<Appointment>> GetByProviderAsync(long providerId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default) =>
        _db.Appointments.Where(a => a.ProviderId == providerId && a.When.Start < to && a.When.End > from).OrderBy(a => a.When.Start).ToListAsync(ct);

    public async Task AddAsync(Appointment appointment, CancellationToken ct = default)
        { await _db.Appointments.AddAsync(appointment, ct); await _db.SaveChangesAsync(ct); }

    public async Task UpdateAsync(Appointment appointment, CancellationToken ct = default)
        { _db.Appointments.Update(appointment); await _db.SaveChangesAsync(ct); }
    }
