using System.Linq.Expressions;

namespace Clinix.Application.Interfaces.Generic;

public interface IRepository<T> where T : class
    {
    // ... other methods like GetByIdAsync, AddAsync, etc.

    /// <summary>
    /// Gets the count of all entities in the table.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The total count of entities.</returns>
    Task<int> CountAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the count of entities matching a specific predicate (condition).
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The count of matching entities.</returns>
    Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    }