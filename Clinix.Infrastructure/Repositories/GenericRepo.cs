using System.Linq.Expressions;
using Clinix.Application.Interfaces.Generic;
using Clinix.Application.Interfaces.UserRepo;
using Clinix.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clinix.Infrastructure.Repositories;

public class GenericRepository<T> : IRepository<T> where T : class
    {
    protected readonly  ClinixDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(ClinixDbContext context)
        {
        _context = context;
        _dbSet = context.Set<T>();
        }

    // ... existing repository methods

    // ----------------------------------------------------
    // New Implementation for CountAsync
    // ----------------------------------------------------

    public virtual async Task<int> CountAsync(CancellationToken ct = default)
        {
        // Counts all records in the table T
        return await _dbSet.CountAsync(ct);
        }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        {
        // Counts records that satisfy the given condition (predicate)
        return await _dbSet.CountAsync(predicate, ct);
        }
    }