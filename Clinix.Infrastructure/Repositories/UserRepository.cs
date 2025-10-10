using Clinix.Application.Interfaces.UserRepo;
using Clinix.Domain.Entities.ApplicationUsers;
using Clinix.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Clinix.Infrastructure.Repositories;

public class UserRepository : IUserRepository
    {
    private readonly ClinixDbContext _db;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(ClinixDbContext db, ILogger<UserRepository> logger)
        {
        _db = db;
        _logger = logger;
        }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var normalized = email.Trim().ToLowerInvariant();
        return await _db.Users
            .AsNoTracking()
            .Where(u => !u.IsDeleted && u.Email.ToLower() == normalized)
            .FirstOrDefaultAsync(ct);
        }

    public async Task<User?> GetByPhoneAsync(string phone, CancellationToken ct = default)
        {
        if (string.IsNullOrWhiteSpace(phone))
            return null;

        var normalized = NormalizePhone(phone);
        return await _db.Users
            .AsNoTracking()
            .Where(u => !u.IsDeleted && u.Phone == normalized)
            .FirstOrDefaultAsync(ct);
        }

    public async Task AddAsync(User user, CancellationToken ct = default)
        {
        if (user == null) throw new ArgumentNullException(nameof(user));
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.Users.AddAsync(user, ct);
        }

    public async Task<User?> GetByIdAsync(long id, CancellationToken ct = default)
        {
        return await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => !u.IsDeleted && u.Id == id, ct);
        }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
        {
        var existingUser = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == user.Id && !u.IsDeleted, ct);

        if (existingUser == null)
            {
            throw new KeyNotFoundException($"User with ID {user.Id} not found.");
            }

        existingUser.FullName = user.FullName;
        existingUser.Email = user.Email;
        existingUser.Phone = user.Phone;
        existingUser.Role = user.Role;
        existingUser.UpdatedAt = DateTime.UtcNow;
        existingUser.UpdatedBy = user.UpdatedBy;
        existingUser.IsProfileCompleted = user.IsProfileCompleted;

        _db.Users.Update(existingUser);
        }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
        {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, ct);
        if (user == null)
            throw new KeyNotFoundException($"User with ID {id} not found.");

        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;

        _db.Users.Update(user);
        }

    public async Task<List<User>> GetAllAsync(CancellationToken ct = default)
        {
        return await _db.Users
            .AsNoTracking()
            .Where(u => !u.IsDeleted)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync(ct);
        }

    private static string NormalizePhone(string phone)
        {
        var trimmed = phone?.Trim() ?? string.Empty;
        var keepPlus = trimmed.StartsWith("+");
        var digitsOnly = new string(trimmed.Where(char.IsDigit).ToArray());
        return keepPlus ? "+" + digitsOnly : digitsOnly;
        }
    }

