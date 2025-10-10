using Clinix.Domain.Entities.ApplicationUsers;

namespace Clinix.Application.Interfaces.UserRepo;

public interface IUserRepository
    {
    Task<User?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByPhoneAsync(string phone, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);
    Task DeleteAsync(long id, CancellationToken ct = default);
    Task<List<User>> GetAllAsync(CancellationToken ct = default);
    }

