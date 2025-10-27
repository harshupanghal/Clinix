using Clinix.Application.Interfaces.UserRepo;
using Clinix.Domain.Entities.ApplicationUsers;
using Clinix.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Clinix.Infrastructure.Repositories;

public class PatientRepository : IPatientRepository
    {
    private readonly ClinixDbContext _db;
    private readonly ILogger<PatientRepository> _logger;

    public PatientRepository(ClinixDbContext db, ILogger<PatientRepository> logger)
        {
        _db = db;
        _logger = logger;
        }

    public async Task<int> CountAsync(CancellationToken ct = default)
        {
        _logger.LogTrace("Counting all Patient records.");
        return await _db.Patients.CountAsync(ct);
        }

    public async Task<List<Patient>> GetAllAsync(CancellationToken ct = default)
        {
        return await _db.Patients.ToListAsync(ct);
        }

    public async Task<int> CountAsync(Expression<Func<Patient, bool>> predicate, CancellationToken ct = default)
        {
        _logger.LogTrace("Counting Patient records with predicate.");
        return await _db.Patients.CountAsync(predicate, ct);
        }

    public async Task AddAsync(Patient patient, CancellationToken ct = default)
        {
        if (patient == null) throw new ArgumentNullException(nameof(patient));
        patient.CreatedAt = DateTime.UtcNow;
        patient.UpdatedAt = DateTime.UtcNow;
        await _db.Patients.AddAsync(patient, ct);
        }

    public async Task<Patient?> GetByUserIdAsync(long id, CancellationToken ct = default)
        {
        return await _db.Patients
            .Include(p => p.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == id && !p.User.IsDeleted, ct);
        }
    public async Task UpdateAsync(Patient patient, CancellationToken ct = default)
        {
        if (patient == null) throw new ArgumentNullException(nameof(patient));

        var existing = await _db.Patients.FirstOrDefaultAsync(p => p.PatientId == patient.PatientId, ct);
        if (existing == null)
            throw new KeyNotFoundException($"Patient with ID {patient.PatientId} not found.");

        existing.DateOfBirth = patient.DateOfBirth;
        existing.Gender = patient.Gender;
        existing.BloodGroup = patient.BloodGroup;
        existing.KnownAllergies = patient.KnownAllergies;
        existing.ExistingConditions = patient.ExistingConditions;
        existing.EmergencyContactName = patient.EmergencyContactName;
        existing.EmergencyContactNumber = patient.EmergencyContactNumber;
        existing.UpdatedAt = DateTime.UtcNow;
        existing.UpdatedBy = patient.UpdatedBy;

        _db.Patients.Update(existing);
        }

    public async Task<IEnumerable<Patient>> GetAllPatientsAsync(CancellationToken ct = default)
        {
        return await _db.Patients.ToListAsync();
        }
    public async Task DeletePatientAsync(long id)
        {
        var patient = await GetByUserIdAsync(id);
        if (patient != null)
            {
            _db.Patients.Remove(patient);
            await _db.SaveChangesAsync();
            }
        }
    }