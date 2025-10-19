using Clinix.Application.Dtos.FollowUps;

namespace Clinix.Application.UseCases;

    /// <summary>
     /// Use-case to create a followup from an appointment. This is the primary Day-1 use case.
     /// </summary>
public interface ICreateFollowUpFromAppointmentUseCase
    {
    Task<FollowUpDto> HandleAsync(long appointmentId, long createdByUserId, bool consentGiven, CancellationToken ct = default);
    }

    /// <summary>
     /// Background worker entry point: process due followup items and trigger sends.
     /// </summary>
public interface IProcessDueFollowUpItemsUseCase
    {
    Task<int> HandleAsync(int maxItems, CancellationToken ct = default);
    Task HandleAsync();
    }
