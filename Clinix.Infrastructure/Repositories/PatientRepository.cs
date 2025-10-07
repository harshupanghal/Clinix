using Clinix.Application.Interfaces;
using Clinix.Domain.Entities;
using Clinix.Infrastructure.Data;

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

