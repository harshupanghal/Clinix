using Clinix.Application.Interfaces;
using Clinix.Domain.Entities;
using Clinix.Infrastructure.Data;

namespace Clinix.Infrastructure.Repositories;

public class DoctorRepository : IDoctorRepository
    {
    private readonly ClinixDbContext _db;
    public DoctorRepository(ClinixDbContext db) => _db = db;

    public Task AddAsync(Doctor doctor, CancellationToken ct = default)
        {
        _db.Doctors.Add(doctor);
        return Task.CompletedTask;
        }
    }

