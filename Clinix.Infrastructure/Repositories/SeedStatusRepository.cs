using Clinix.Application.Interfaces.Functionalities;
using Clinix.Domain.Entities.System;
using Clinix.Domain.Interfaces;
using Clinix.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clinix.Infrastructure.Data;

public class SeedStatusRepository : ISeedStatusRepository
    {
    private readonly ClinixDbContext _dbContext;

    public SeedStatusRepository(ClinixDbContext dbContext) => _dbContext = dbContext;

    public async Task<bool> IsSeedCompletedAsync(string seedName, string version, CancellationToken ct = default)
        {
        return await _dbContext.SeedStatuses
            .AnyAsync(s => s.SeedName == seedName && s.Version == version && s.IsCompleted, ct);
        }

    public async Task<SeedStatus?> GetBySeedNameAsync(string seedName, string version, CancellationToken ct = default)
        {
        return await _dbContext.SeedStatuses
            .FirstOrDefaultAsync(s => s.SeedName == seedName && s.Version == version, ct);
        }

    public async Task AddAsync(SeedStatus seedStatus, CancellationToken ct = default)
        {
        await _dbContext.SeedStatuses.AddAsync(seedStatus, ct);
        await _dbContext.SaveChangesAsync(ct);
        }

    public async Task UpdateAsync(SeedStatus seedStatus, CancellationToken ct = default)
        {
        _dbContext.SeedStatuses.Update(seedStatus);
        await _dbContext.SaveChangesAsync(ct);
        }
    }
