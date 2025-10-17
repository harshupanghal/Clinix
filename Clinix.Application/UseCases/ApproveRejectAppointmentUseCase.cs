using Clinix.Application.Interfaces.Functionalities;
using Clinix.Domain.Exceptions;

namespace Clinix.Application.UseCases;

public class ApproveRejectAppointmentUseCase
    {
    private readonly IAppointmentRepository _appointments;
    private readonly INotificationService _notifications;

    public ApproveRejectAppointmentUseCase(IAppointmentRepository appointments, INotificationService notifications)
        {
        _appointments = appointments;
        _notifications = notifications;
        }

    public async Task ApproveAsync(long appointmentId, string actor)
        {
        var appt = await _appointments.GetByIdAsync(appointmentId) ?? throw new SchedulingException("Appointment not found");
        appt.Approve(actor);
        await _appointments.UpdateAsync(appt);
        await _notifications.NotifyPatientAsync(appt.PatientId, "Appointment approved", $"Your appointment {appt.Id} has been approved by the doctor.");
        }

    public async Task RejectAsync(long appointmentId, string actor, string? reason = null)
        {
        var appt = await _appointments.GetByIdAsync(appointmentId) ?? throw new SchedulingException("Appointment not found");
        appt.Reject(actor, reason);
        await _appointments.UpdateAsync(appt);
        await _notifications.NotifyPatientAsync(appt.PatientId, "Appointment rejected", $"Your appointment {appt.Id} was rejected. Reason: {reason}");
        }
    }

