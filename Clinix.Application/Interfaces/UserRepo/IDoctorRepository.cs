using Clinix.Application.Interfaces.Generic;
using Clinix.Domain.Entities.ApplicationUsers;

namespace Clinix.Application.Interfaces.UserRepo;

public interface IDoctorRepository : IRepository<Doctor>
    {
    Task AddAsync(Doctor doctor, CancellationToken ct = default);
    Task<Doctor?> GetByUserIdAsync(long userId, CancellationToken ct = default);
    Task UpdateAsync(Doctor doctor, CancellationToken ct = default);
    Task DeleteAsync(long userId, CancellationToken ct = default);
    Task<List<Doctor>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<Doctor>> GetBySpecialtyAsync(string specialty, CancellationToken ct = default);
    }

