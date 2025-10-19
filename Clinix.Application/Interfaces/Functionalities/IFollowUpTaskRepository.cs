using Clinix.Domain.Entities.FollowUps;

namespace Clinix.Application.Interfaces.Functionalities;

public interface IFollowUpTaskRepository
    {
    Task<FollowUpTask?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task UpdateAsync(FollowUpTask task, CancellationToken cancellationToken = default);
    Task AddAsync(FollowUpTask task, CancellationToken cancellationToken = default);
    Task DeleteAsync(FollowUpTask task, CancellationToken cancellationToken = default);
    Task<List<FollowUpTask>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddManyAsync(IEnumerable<FollowUpTask> tasks, CancellationToken cancellationToken = default);
    Task<List<FollowUpTask>> ClaimDueTasksAsync(DateTimeOffset now, int batchSize = 50, CancellationToken cancellationToken = default);
    Task<IEnumerable<FollowUpTask>> GetTasksForFollowUpAsync(long followUpId, CancellationToken cancellationToken = default);
    Task<IEnumerable<FollowUpTask>> GetTasksForDoctorAsync(long doctorId, CancellationToken cancellationToken = default);
    Task<IEnumerable<FollowUpTask>> GetTasksForPatientAsync(long patientId, CancellationToken cancellationToken = default);
    }
