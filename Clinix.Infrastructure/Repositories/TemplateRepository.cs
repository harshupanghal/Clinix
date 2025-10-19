using Clinix.Application.Services;
using Clinix.Domain.Entities.FollowUp;
using Clinix.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clinix.Infrastructure.Repositories;

/// <summary>
/// Simple EF-backed template repository.
/// </summary>
public class TemplateRepository : ITemplateRepository
    {
    private readonly ClinixDbContext _db;

    public TemplateRepository(ClinixDbContext db)
        {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        }

    public async Task<MessageTemplate?> GetByIdAsync(long id, CancellationToken ct = default)
        {
        return await _db.MessageTemplates.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, ct);
        }
    }

