using Clinix.Application.Interfaces;
using Clinix.Domain.Entities.ApplicationUsers;
using Clinix.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clinix.Infrastructure.Contacts;

/// <summary>
/// Production contact provider that fetches real patient and doctor email/phone from database.
/// Used by notification system to deliver messages to correct recipients.
/// </summary>
public sealed class DbContactProvider : IContactProvider
    {
    private readonly ClinixDbContext _db;

    public DbContactProvider(ClinixDbContext db) => _db = db;

    /// <summary>
    /// Gets patient contact information (email & phone) from User table via Patient relationship.
    /// </summary>
    public async Task<(string? Email, string? Phone)> GetPatientContactAsync(long patientId, CancellationToken ct = default)
        {
        var patient = await _db.Patients
            .Include(p => p.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PatientId == patientId, ct);

        return patient?.User != null
            ? (patient.User.Email, patient.User.Phone)
            : (null, null);
        }

    /// <summary>
    /// Gets doctor contact information (email & phone) from User table via Doctor relationship.
    /// </summary>
    public async Task<(string? Email, string? Phone)> GetDoctorContactAsync(long doctorId, CancellationToken ct = default)
        {
        var doctor = await _db.Doctors
            .Include(d => d.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DoctorId == doctorId, ct);

        return doctor?.User != null
            ? (doctor.User.Email, doctor.User.Phone)
            : (null, null);
        }

    /// <summary>
    /// Gets provider name for display in notifications.
    /// </summary>
    public async Task<string> GetProviderNameAsync(long providerId, CancellationToken ct = default)
        {
        var provider = await _db.Providers
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == providerId, ct);

        return provider?.Name ?? "Doctor";
        }

    /// <summary>
    /// Gets patient name for display in notifications.
    /// </summary>
    public async Task<string> GetPatientNameAsync(long patientId, CancellationToken ct = default)
        {
        var patient = await _db.Patients
            .Include(p => p.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PatientId == patientId, ct);

        return patient?.User?.FullName ?? "Patient";
        }

    /// <summary>
    /// Finds doctor by ProviderId (1:1 relationship).
    /// Used to send notifications to doctors when appointments are booked/cancelled.
    /// </summary>
    public async Task<Doctor?> GetDoctorByProviderIdAsync(long providerId, CancellationToken ct = default)
        {
        return await _db.Doctors
            .Include(d => d.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.ProviderId == providerId, ct);
        }
    }
