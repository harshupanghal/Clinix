// Infrastructure/Persistence/Repositories/ProviderRepository.cs
using Clinix.Domain.Entities;
using Clinix.Domain.Interfaces;
using Clinix.Infrastructure.Persistence;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;

namespace Clinix.Infrastructure.Repositories;

public sealed class ProviderRepository : IProviderRepository
    {
    private readonly ClinixDbContext _db;
    public ProviderRepository(ClinixDbContext db) => _db = db;

    public Task<Provider?> GetByIdAsync(long id, CancellationToken ct = default) =>
        _db.Providers.FirstOrDefaultAsync(p => p.Id == id, ct);

    // Infrastructure/Repositories/ProviderRepository.cs
    public async Task<List<Provider>> SearchAsync(string[] keywords, CancellationToken ct = default)
        {
        Console.WriteLine($"[ProviderRepo] SearchAsync called with {keywords?.Length ?? 0} keywords");

        if (keywords == null || keywords.Length == 0)
            {
            Console.WriteLine($"[ProviderRepo] No keywords - returning all providers");
            var all = await _db.Providers.AsNoTracking().OrderBy(p => p.Name).ToListAsync(ct);
            Console.WriteLine($"[ProviderRepo] Returning {all.Count} providers");
            return all;
            }

        Console.WriteLine($"[ProviderRepo] Keywords: {string.Join(", ", keywords)}");

        // Get all providers first
        var allProviders = await _db.Providers.AsNoTracking().ToListAsync(ct);
        Console.WriteLine($"[ProviderRepo] Total providers in DB: {allProviders.Count}");

        if (allProviders.Count == 0)
            {
            Console.WriteLine($"[ProviderRepo] WARNING: No providers in database!");
            return new List<Provider>();
            }

        // Log each provider
        foreach (var p in allProviders)
            {
            Console.WriteLine($"[ProviderRepo] Provider: {p.Name} | Specialty: {p.Specialty} | Tags: {p.Tags ?? "NULL"}");
            }

        // Filter in memory
        var results = allProviders.Where(p =>
        {
            var searchText = $"{p.Name} {p.Specialty} {p.Tags}".ToLowerInvariant();
            var match = keywords.Any(k => searchText.Contains(k.ToLowerInvariant()));

            if (match)
                {
                Console.WriteLine($"[ProviderRepo] MATCH: {p.Name} matched keyword");
                }

            return match;
        })
        .OrderBy(p => p.Name)
        .ToList();

        Console.WriteLine($"[ProviderRepo] Returning {results.Count} matching providers");
        return results;
        }


    // Infrastructure/Persistence/Repositories/ProviderRepository.cs (add method)
    public async Task UpdateAsync(Provider provider, CancellationToken ct = default)
        {
        _db.Providers.Update(provider);
        await _db.SaveChangesAsync(ct);
        }
    public async Task AddAsync(Provider provider, CancellationToken ct = default)
        {
        await _db.Providers.AddAsync(provider, ct);
        await _db.SaveChangesAsync(ct);
        }

    }
