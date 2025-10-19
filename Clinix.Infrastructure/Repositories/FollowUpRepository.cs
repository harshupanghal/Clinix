using Clinix.Application.Interfaces.Functionalities;
using Clinix.Domain.Entities.FollowUps;
using Clinix.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clinix.Infrastructure.Repositories;

public class FollowUpRepository : IFollowUpRepository, IFollowUpRepositoryExtended
    {
    private readonly ClinixDbContext _db;
    public FollowUpRepository(ClinixDbContext db) => _db = db;

    public async Task AddAsync(FollowUpRecord followUp)
        {
        if (followUp == null) throw new ArgumentNullException(nameof(followUp));
        // EF will track snapshots via navigation
        await _db.FollowUpRecords.AddAsync(followUp);
        await _db.SaveChangesAsync();
        }

    public async Task<FollowUpRecord?> GetByIdAsync(long id)
        {
        var entity = await _db.FollowUpRecords
            .Include(x => x.MedicationSnapshots)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
        return entity;
        }

    public async Task UpdateAsync(FollowUpRecord followUp)
        {
        _db.FollowUpRecords.Update(followUp);
        await _db.SaveChangesAsync();
        }

    // Additional methods for admin listing/search can be added here
    }

