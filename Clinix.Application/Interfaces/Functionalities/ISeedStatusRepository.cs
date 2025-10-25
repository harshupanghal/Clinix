// Domain/Interfaces/ISeedStatusRepository.cs
namespace Clinix.Domain.Interfaces;

using Clinix.Domain.Entities.System;

public interface ISeedStatusRepository
    {
    Task<SeedStatus?> GetBySeedNameAsync(string seedName, string version, CancellationToken ct = default);
    Task<bool> IsSeedCompletedAsync(string seedName, string version, CancellationToken ct = default);
    Task AddAsync(SeedStatus status, CancellationToken ct = default);
    Task UpdateAsync(SeedStatus status, CancellationToken ct = default);
    }
