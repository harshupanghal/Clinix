using Clinix.Application.Interfaces.UserRepo;
using Clinix.Domain.Entities.ApplicationUsers;
using Clinix.Infrastructure.Persistence;

namespace Clinix.Infrastructure.Repositories;

public class StaffRepository : IStaffRepository
    {
    private readonly ClinixDbContext _db;
    public StaffRepository(ClinixDbContext db) => _db = db;

    public Task AddAsync(Staff staff, CancellationToken ct = default)
        {
        _db.Staffs.Add(staff);
        return Task.CompletedTask;
        }
    }

