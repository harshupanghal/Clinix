// Infrastructure/Repositories/SeedStatusRepository.cs
using Clinix.Domain.Entities.System;
using Clinix.Domain.Interfaces;
using Clinix.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clinix.Infrastructure.Repositories;

public sealed class SeedStatusRepository : ISeedStatusRepository
    {
    private readonly ClinixDbContext _db;

    public SeedStatusRepository(ClinixDbContext db) => _db = db;

    public Task<SeedStatus?> GetBySeedNameAsync(string seedName, string version, CancellationToken ct = default) =>
        _db.SeedStatuses
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SeedName == seedName && s.Version == version, ct);

    public async Task<bool> IsSeedCompletedAsync(string seedName, string version, CancellationToken ct = default)
        {
        var status = await GetBySeedNameAsync(seedName, version, ct);
        return status?.IsCompleted == true;
        }

    public async Task AddAsync(SeedStatus status, CancellationToken ct = default)
        {
        await _db.SeedStatuses.AddAsync(status, ct);
        await _db.SaveChangesAsync(ct);
        }

    public async Task UpdateAsync(SeedStatus status, CancellationToken ct = default)
        {
        _db.SeedStatuses.Update(status);
        await _db.SaveChangesAsync(ct);
        }
    }
