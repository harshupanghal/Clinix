using Clinix.Application.Dtos.FollowUps;
using Clinix.Domain.Entities.Appointments;

namespace Clinix.Application.Interfaces.Services;

public interface IFollowUpService
    {
    Task<IEnumerable<FollowUpListItemDto>> GetAllForAdminAsync(CancellationToken cancellationToken = default);
    Task<FollowUpDetailDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<FollowUpDto> CreateManualFollowUpAsync(CreateManualFollowUpRequest request, CancellationToken cancellationToken = default);
    Task RescheduleTaskAsync(long taskId, DateTimeOffset scheduledAt, long actorUserId, CancellationToken cancellationToken = default);
    Task PauseTaskAsync(long taskId, long actorUserId, CancellationToken cancellationToken = default);
    Task CancelTaskAsync(long taskId, long actorUserId, string? reason, CancellationToken cancellationToken = default);
    Task<IEnumerable<FollowUpTaskDto>> GetTasksForDoctorAsync(long doctorId, CancellationToken cancellationToken = default);
    Task<IEnumerable<FollowUpTaskDto>> GetTasksForPatientAsync(long patientId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(DateTimeOffset from, CancellationToken cancellationToken = default);
    }

