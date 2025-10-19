using Clinix.Application.Interfaces.Functionalities;
using Clinix.Domain.Entities.FollowUps;
using Clinix.Domain.Enums;
using Clinix.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clinix.Infrastructure.Repositories;

public class FollowUpTaskRepository : IFollowUpTaskRepository
    {
    private readonly ClinixDbContext _db;

    public FollowUpTaskRepository(ClinixDbContext db)
        {
        _db = db;
        }

    public async Task<FollowUpTask?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        {
        return await _db.FollowUpTasks
            .Include(t => t.FollowUpRecord)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        }

    public async Task AddAsync(FollowUpTask task, CancellationToken cancellationToken = default)
        {
        await _db.FollowUpTasks.AddAsync(task, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        }

    public async Task UpdateAsync(FollowUpTask task, CancellationToken cancellationToken = default)
        {
        _db.FollowUpTasks.Update(task);
        await _db.SaveChangesAsync(cancellationToken);
        }

    public async Task DeleteAsync(FollowUpTask task, CancellationToken cancellationToken = default)
        {
        _db.FollowUpTasks.Remove(task);
        await _db.SaveChangesAsync(cancellationToken);
        }

    public async Task<List<FollowUpTask>> GetAllAsync(CancellationToken cancellationToken = default)
        {
        return await _db.FollowUpTasks
            .Include(t => t.FollowUpRecord)
            .ToListAsync(cancellationToken);
        }
    public async Task AddManyAsync(IEnumerable<FollowUpTask> tasks, CancellationToken cancellationToken = default)
        {
        await _db.FollowUpTasks.AddRangeAsync(tasks, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        }

    public async Task<List<FollowUpTask>> ClaimDueTasksAsync(DateTimeOffset now, int batchSize = 50, CancellationToken cancellationToken = default)
        {
        // Step 1: Select tasks that are due and pending
        var dueTasks = await _db.FollowUpTasks
            .Where(t => t.Status == FollowUpTaskStatus.Pending
                     && t.ScheduledAt <= now
                     && !t.IsClaimed)
            .OrderBy(t => t.ScheduledAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        // Step 2: Claim them (lock for processing)
        foreach (var task in dueTasks)
            {
            task.IsClaimed = true;
            task.ClaimedAt = DateTimeOffset.UtcNow;
            }

        // Step 3: Save changes atomically
        await _db.SaveChangesAsync(cancellationToken);

        //_logger?.LogInformation("Claimed {Count} due follow-up tasks at {Time}.", dueTasks.Count, now);

        return dueTasks;
        }


    }
