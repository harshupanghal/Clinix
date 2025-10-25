// Infrastructure/Persistence/Repositories/ProviderRepository.cs
using Clinix.Domain.Entities;
using Clinix.Domain.Interfaces;
using Clinix.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clinix.Infrastructure.Repositories;

public sealed class ProviderRepository : IProviderRepository
    {
    private readonly ClinixDbContext _db;
    public ProviderRepository(ClinixDbContext db) => _db = db;

    public Task<Provider?> GetByIdAsync(long id, CancellationToken ct = default) =>
        _db.Providers.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<List<Provider>> SearchAsync(IEnumerable<string> tokens, CancellationToken ct = default)
        {
        var q = _db.Providers.AsQueryable();
        foreach (var t in tokens)
            {
            var term = t.Trim();
            q = q.Where(p => EF.Functions.Like(p.Specialty, $"%{term}%")
                          || p.Tags != null && EF.Functions.Like(p.Tags, $"%{term}%")
                          || EF.Functions.Like(p.Name, $"%{term}%"));
            }
        return await q.Take(20).ToListAsync(ct);
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
