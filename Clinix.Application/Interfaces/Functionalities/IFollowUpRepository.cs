using System.Threading.Tasks;
using Clinix.Domain.Entities.FollowUps;

namespace Clinix.Application.Interfaces.Functionalities;

public interface IFollowUpRepository
    {
    Task AddAsync(FollowUpRecord followUp);
    Task<FollowUpRecord?> GetByIdAsync(long id);
    Task<IEnumerable<FollowUpRecord>> GetAllAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(FollowUpRecord followUp, CancellationToken cancellationToken = default);

    // add methods for queries as needed (GetForPatientAsync, Search, etc.)
    }

public interface IFollowUpRepositoryExtended : IFollowUpRepository
    {
    Task UpdateAsync(FollowUpRecord followUp);
    }
