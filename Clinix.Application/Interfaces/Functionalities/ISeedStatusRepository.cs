namespace Clinix.Application.Interfaces.Functionalities;

using Clinix.Domain.Entities.System;

public interface ISeedStatusRepository
    {
    Task<bool> IsSeedCompletedAsync(string seedName, string version, CancellationToken ct = default);
    Task<SeedStatus?> GetBySeedNameAsync(string seedName, string version, CancellationToken ct = default);
    Task AddAsync(SeedStatus seedStatus, CancellationToken ct = default);
    Task UpdateAsync(SeedStatus seedStatus, CancellationToken ct = default);
    }
