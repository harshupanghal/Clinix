using Clinix.Application.Interfaces;
using Clinix.Domain.Entities;
using Clinix.Infrastructure.Data;

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

