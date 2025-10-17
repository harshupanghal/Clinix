using Clinix.Application.Interfaces.Functionalities;
using Clinix.Domain.Entities.Appointments;
using Clinix.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clinix.Infrastructure.Repositories;

public class EfSymptomMappingRepository : ISymptomMappingRepository
    {
    private readonly ClinixDbContext _db;
    public EfSymptomMappingRepository(ClinixDbContext db) => _db = db;

    public async Task<IEnumerable<SymptomMapping>> GetAllMappingsAsync()
        => await _db.SymptomMappings.ToListAsync();
    public async Task AddOrUpdateAsync(SymptomMapping mapping)
        {
        var exist = await _db.SymptomMappings.FindAsync(mapping.Id);
        if (exist == null) _db.SymptomMappings.Add(mapping);
        else _db.SymptomMappings.Update(mapping);
        await _db.SaveChangesAsync();
        }

    public async Task DeleteAsync(long id)
        {
        var e = await _db.SymptomMappings.FindAsync(id);
        if (e == null) return;
        _db.SymptomMappings.Remove(e);
        await _db.SaveChangesAsync();
        }

    public async Task<List<SymptomMapping>> SearchByKeywordsAsync(IEnumerable<string> keywords)
        {
        var k = keywords.Select(x => x.ToLowerInvariant()).ToList();
        return await _db.SymptomMappings
            .Where(m => k.Any(kw => m.Keyword.ToLower().Contains(kw)))
            .ToListAsync();
        }
    }
