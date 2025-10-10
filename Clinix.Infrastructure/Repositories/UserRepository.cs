using Clinix.Application.Interfaces.RepoInterfaces;
using Clinix.Domain.Entities.ApplicationUsers;
using Clinix.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clinix.Infrastructure.Repositories;

public class UserRepository : IUserRepository
    {
    private readonly ClinixDbContext _db;
    public UserRepository(ClinixDbContext db) => _db = db;

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _db.Users.AsNoTracking().Where(u => !u.IsDeleted && u.Email == email).FirstOrDefaultAsync(ct);

    public async Task<User?> GetByPhoneAsync(string phone, CancellationToken ct = default)
        => await _db.Users.AsNoTracking().Where(u => !u.IsDeleted && u.Phone == phone).FirstOrDefaultAsync(ct);

    public Task AddAsync(User user, CancellationToken ct = default)
        {
        _db.Users.Add(user);
        return Task.CompletedTask;
        }

    Task<User?> IUserRepository.GetByIdAsync(int id, CancellationToken ct)
        {
        throw new NotImplementedException();
        }

    Task IUserRepository.UpdateAsync(User user, CancellationToken ct)
        {
        throw new NotImplementedException();
        }

    Task IUserRepository.DeleteAsync(int id, CancellationToken ct)
        {
        throw new NotImplementedException();
        }

    Task<List<User>> IUserRepository.GetAllAsync()
        {
        throw new NotImplementedException();
        }

    }

    //public async Task<User?> GetByPhoneAsync(string phone, CancellationToken ct = default)
    //    => await _db.Users.AsNoTracking().Where(u => !u.IsDeleted && u.Phone == phone).FirstOrDefaultAsync(ct);
    //}

