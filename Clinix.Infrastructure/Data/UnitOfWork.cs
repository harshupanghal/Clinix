using Clinix.Application.Interfaces.Functionalities;
using Clinix.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace Clinix.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
    {
    private readonly ClinixDbContext _dbContext;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(ClinixDbContext dbContext) => _dbContext = dbContext;

    public async Task BeginTransactionAsync(CancellationToken ct = default)
        {
        if (_transaction != null) return;
        _transaction = await _dbContext.Database.BeginTransactionAsync(ct);
        }

    public async Task CommitAsync(CancellationToken ct = default)
        {
        if (_transaction == null)
            {
            await _dbContext.SaveChangesAsync(ct);
            return;
            }

        await _dbContext.SaveChangesAsync(ct);
        await _transaction.CommitAsync(ct);
        await _transaction.DisposeAsync();
        _transaction = null;
        }

    public async Task RollbackAsync(CancellationToken ct = default)
        {
        if (_transaction == null) return;
        await _transaction.RollbackAsync(ct);
        await _transaction.DisposeAsync();
        _transaction = null;
        }
    public async Task SaveChangesAsync(CancellationToken ct = default)
        {
        await _dbContext.SaveChangesAsync(ct);
        }
    }

