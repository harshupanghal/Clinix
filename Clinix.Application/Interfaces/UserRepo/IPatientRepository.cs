using Clinix.Domain.Entities.ApplicationUsers;

namespace Clinix.Application.Interfaces.UserRepo;

public interface IPatientRepository
    {
    Task AddAsync(Patient patient, CancellationToken ct = default);
    Task<Patient?> GetByUserIdAsync(long id, CancellationToken ct = default);
    Task UpdateAsync(Patient patient, CancellationToken ct = default);
    }

