using Clinix.Application.Interfaces.RepoInterfaces;
using Clinix.Domain.Entities.ApplicationUsers;
using Clinix.Infrastructure.Persistence;

namespace Clinix.Infrastructure.Repositories;

public class PatientRepository : IPatientRepository
    {
    private readonly ClinixDbContext _db;
    public PatientRepository(ClinixDbContext db) => _db = db;

    public Task AddAsync(Patient patient, CancellationToken ct = default)
        {
        _db.Patients.Add(patient);
        return Task.CompletedTask;
        }
    }

