using Clinix.Application.Interfaces.Generic;
using Clinix.Domain.Entities.ApplicationUsers;

namespace Clinix.Application.Interfaces.UserRepo;

public interface IPatientRepository : IRepository<Patient>
    {
    Task AddAsync(Patient patient, CancellationToken ct = default);
    Task<Patient?> GetByUserIdAsync(long id, CancellationToken ct = default);
    Task UpdateAsync(Patient patient, CancellationToken ct = default);
    Task<IEnumerable<Patient>> GetAllPatientsAsync(CancellationToken ct = default);
    Task<List<Patient>> GetAllAsync(CancellationToken ct = default);
    }

