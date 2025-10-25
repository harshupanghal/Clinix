// Application/Interfaces/IAppServices.cs
namespace Clinix.Application.Interfaces;

using Clinix.Application.Dtos;
using Clinix.Application.DTOs;

public interface IAppointmentAppService
    {
    Task<AppointmentDto> ScheduleAsync(ScheduleAppointmentRequest request, CancellationToken ct = default);
    Task<AppointmentDto> RescheduleAsync(RescheduleAppointmentRequest request, CancellationToken ct = default);
    Task<bool> CancelAsync(CancelAppointmentRequest request, CancellationToken ct = default);
    Task<bool> CompleteAsync(CompleteAppointmentRequest request, CancellationToken ct = default);
    Task<List<AppointmentSummaryDto>> GetByProviderAsync(long providerId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default);
    Task<List<AppointmentSummaryDto>> GetByPatientAsync(long patientId, CancellationToken ct = default);
    Task<AppointmentDto?> GetByIdAsync(long id, CancellationToken ct = default);
    }

public interface IFollowUpAppService
    {
    Task<FollowUpDto> CreateAsync(CreateFollowUpRequest request, CancellationToken ct = default);
    Task<bool> CompleteAsync(CompleteFollowUpRequest request, CancellationToken ct = default);
    Task<bool> CancelAsync(CancelFollowUpRequest request, CancellationToken ct = default);
    Task<List<FollowUpDto>> GetByAppointmentAsync(long appointmentId, CancellationToken ct = default);
    Task<FollowUpDto?> GetByIdAsync(long id, CancellationToken ct = default);
    }

public interface IProviderAppService
    {
    Task<List<ProviderDto>> RecommendAsync(ProviderRecommendationRequest request, CancellationToken ct = default);
    Task<List<(DateTimeOffset Start, DateTimeOffset End)>> GetAvailableSlotsAsync(AvailableSlotsRequest request, CancellationToken ct = default);
    Task<ProviderDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<bool> UpdateWorkingHoursAsync(UpdateProviderWorkingHoursRequest request, CancellationToken ct = default);
    }

public interface IDoctorActionsAppService
    {
    Task<bool> ApproveAsync(long appointmentId, CancellationToken ct = default);
    Task<bool> RejectAsync(long appointmentId, string? reason = null, CancellationToken ct = default);
    Task<bool> DelayCascadeAsync(long appointmentId, TimeSpan delay, CancellationToken ct = default);
    }

// Cross-cutting interfaces
public interface INotificationSender
    {
    Task SendEmailAsync(string to, string subject, string body, CancellationToken ct = default);
    Task SendSmsAsync(string to, string message, CancellationToken ct = default);
    }

public interface IContactProvider
    {
    Task<(string? Email, string? Phone)> GetPatientContactAsync(long patientId, CancellationToken ct = default);
    }
