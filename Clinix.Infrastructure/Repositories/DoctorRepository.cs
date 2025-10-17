using Clinix.Application.Interfaces.UserRepo;
using Clinix.Domain.Entities.ApplicationUsers;
using Clinix.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions; // Required for the predicate overload

namespace Clinix.Infrastructure.Repositories;

public class DoctorRepository : IDoctorRepository
    {
    private readonly ClinixDbContext _db;
    private readonly ILogger<DoctorRepository> _logger;

    public DoctorRepository(ClinixDbContext db, ILogger<DoctorRepository> logger)
        {
        _db = db;
        _logger = logger;
        }

    // ----------------------------------------------------
    // START: Implementation of IRepository<Doctor>.CountAsync
    // ----------------------------------------------------

    public async Task<int> CountAsync(CancellationToken ct = default)
        {
        _logger.LogTrace("Counting all Doctor records.");
        return await _db.Doctors.CountAsync(ct);
        }

    public async Task<int> CountAsync(Expression<Func<Doctor, bool>> predicate, CancellationToken ct = default)
        {
        _logger.LogTrace("Counting Doctor records with predicate.");
        return await _db.Doctors.CountAsync(predicate, ct);
        }

    // ----------------------------------------------------
    // END: Implementation of IRepository<Doctor>.CountAsync
    // ----------------------------------------------------

    public async Task AddAsync(Doctor doctor, CancellationToken ct = default)
        {
        if (doctor == null) throw new ArgumentNullException(nameof(doctor));
        await _db.Doctors.AddAsync(doctor, ct);
        }

    public async Task DeleteAsync(long userId, CancellationToken ct = default)
        {
        var doc = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId, ct);
        if (doc == null) throw new KeyNotFoundException($"Doctor for userId {userId} not found.");
        _db.Doctors.Remove(doc);
        }

    public async Task<IEnumerable<Doctor>> GetBySpecialtyAsync(string specialty, CancellationToken ct = default)
        {
        if (string.IsNullOrWhiteSpace(specialty))
            return Enumerable.Empty<Doctor>();

        // match case-insensitively; include User navigation for display
        return await _db.Doctors
            .Include(d => d.User)
            .Where(d => !string.IsNullOrEmpty(d.Specialty) &&
                        d.Specialty.ToLower() == specialty.Trim().ToLower())
            .AsNoTracking()
            .ToListAsync(ct);
        }

    public async Task<Doctor?> GetByUserIdAsync(long userId, CancellationToken ct = default)
        {
        return await _db.Doctors
            .Include(d => d.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.UserId == userId, ct);
        }

    public async Task<List<Doctor>> GetAllAsync(CancellationToken ct = default)
        {
        return await _db.Doctors
            .Include(d => d.User)
            .AsNoTracking()
            .OrderByDescending(d => d.DoctorId)
            .ToListAsync(ct);
        }

    public Task UpdateAsync(Doctor doctor, CancellationToken ct = default)
        {
        _db.Doctors.Update(doctor);
        return Task.CompletedTask;
        }
    }