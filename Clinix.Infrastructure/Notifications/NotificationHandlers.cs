using Clinix.Application.Interfaces;
using Clinix.Domain.Events;
using Clinix.Domain.Interfaces;
using Clinix.Infrastructure.Contacts;
using Microsoft.Extensions.Logging;
using static Clinix.Infrastructure.Notifications.NotificationTemplates;

namespace Clinix.Infrastructure.Notifications;

public sealed class NotificationHandlers
    {
    private readonly IAppointmentRepository _appointments;
    private readonly IFollowUpRepository _followUps;
    private readonly DbContactProvider _contacts;
    private readonly INotificationSender _sender;
    private readonly ILogger<NotificationHandlers> _logger;

    public NotificationHandlers(
        IAppointmentRepository appointments,
        IFollowUpRepository followUps,
        DbContactProvider contacts,
        INotificationSender sender,
        ILogger<NotificationHandlers> logger)
        {
        _appointments = appointments;
        _followUps = followUps;
        _contacts = contacts;
        _sender = sender;
        _logger = logger;
        }

    /// <summary>
    /// Handles AppointmentScheduled event - sends confirmation to BOTH patient and doctor.
    /// </summary>
    public async Task HandleAppointmentScheduledAsync(AppointmentScheduled evt, CancellationToken ct)
        {
        try
            {
            var appt = await _appointments.GetByIdAsync(evt.AppointmentId, ct);
            if (appt == null) return;

            // Get contact details
            var (patientEmail, patientPhone) = await _contacts.GetPatientContactAsync(appt.PatientId, ct);
            var patientName = await _contacts.GetPatientNameAsync(appt.PatientId, ct);
            var doctorName = await _contacts.GetProviderNameAsync(appt.ProviderId, ct);

            // Send to PATIENT
            if (!string.IsNullOrWhiteSpace(patientEmail))
                {
                var (subject, body) = AppointmentScheduled_Patient(patientName, doctorName, appt.When.Start, appt.When.End);
                await _sender.SendEmailAsync(patientEmail, subject, body, ct);
                }

            if (!string.IsNullOrWhiteSpace(patientPhone))
                {
                var sms = AppointmentScheduled_SMS_Patient(patientName, doctorName, appt.When.Start);
                await _sender.SendSmsAsync(patientPhone, sms, ct);
                }

            // Send to DOCTOR
            // ✅ Use DbContactProvider method instead of local method
            var doctor = await _contacts.GetDoctorByProviderIdAsync(appt.ProviderId, ct);
            if (doctor != null)
                {
                var (docEmail, docPhone) = await _contacts.GetDoctorContactAsync(doctor.DoctorId, ct);
                if (!string.IsNullOrWhiteSpace(docEmail))
                    {
                    var (subject, body) = AppointmentScheduled_Doctor(doctorName, patientName, appt.When.Start, appt.When.End, appt.Type);
                    await _sender.SendEmailAsync(docEmail, subject, body, ct);
                    }
                }

            _logger.LogInformation("Appointment scheduled notifications sent for appointment {AppointmentId}", evt.AppointmentId);
            }
        catch (Exception ex)
            {
            _logger.LogError(ex, "Failed to handle AppointmentScheduled event for {AppointmentId}", evt.AppointmentId);
            }
        }

    /// <summary>
    /// Handles AppointmentCancelled event - notifies BOTH patient and doctor.
    /// </summary>
    public async Task HandleAppointmentCancelledAsync(AppointmentCancelled evt, CancellationToken ct)
        {
        try
            {
            var appt = await _appointments.GetByIdAsync(evt.AppointmentId, ct);
            if (appt == null) return;

            var (patientEmail, patientPhone) = await _contacts.GetPatientContactAsync(appt.PatientId, ct);
            var patientName = await _contacts.GetPatientNameAsync(appt.PatientId, ct);
            var doctorName = await _contacts.GetProviderNameAsync(appt.ProviderId, ct);

            // Notify PATIENT
            if (!string.IsNullOrWhiteSpace(patientEmail))
                {
                var (subject, body) = AppointmentCancelled_Patient(patientName, doctorName, appt.When.Start, evt.Reason);
                await _sender.SendEmailAsync(patientEmail, subject, body, ct);
                }

            if (!string.IsNullOrWhiteSpace(patientPhone))
                {
                var sms = AppointmentCancelled_SMS(patientName, appt.When.Start);
                await _sender.SendSmsAsync(patientPhone, sms, ct);
                }

            // Notify DOCTOR
            var doctor = await _contacts.GetDoctorByProviderIdAsync(appt.ProviderId, ct);
            if (doctor != null)
                {
                var (docEmail, _) = await _contacts.GetDoctorContactAsync(doctor.DoctorId, ct);
                if (!string.IsNullOrWhiteSpace(docEmail))
                    {
                    var (subject, body) = AppointmentCancelled_Doctor(doctorName, patientName, appt.When.Start);
                    await _sender.SendEmailAsync(docEmail, subject, body, ct);
                    }
                }

            _logger.LogInformation("Appointment cancelled notifications sent for appointment {AppointmentId}", evt.AppointmentId);
            }
        catch (Exception ex)
            {
            _logger.LogError(ex, "Failed to handle AppointmentCancelled event for {AppointmentId}", evt.AppointmentId);
            }
        }

    /// <summary>
    /// Handles AppointmentRescheduled event - notifies patient and doctor of time change.
    /// </summary>
    public async Task HandleAppointmentRescheduledAsync(AppointmentRescheduled evt, CancellationToken ct)
        {
        try
            {
            var appt = await _appointments.GetByIdAsync(evt.AppointmentId, ct);
            if (appt == null) return;

            var (patientEmail, patientPhone) = await _contacts.GetPatientContactAsync(appt.PatientId, ct);
            var patientName = await _contacts.GetPatientNameAsync(appt.PatientId, ct);
            var doctorName = await _contacts.GetProviderNameAsync(appt.ProviderId, ct);

            // Notify PATIENT
            if (!string.IsNullOrWhiteSpace(patientEmail))
                {
                var (subject, body) = AppointmentRescheduled_Patient(
                    patientName, doctorName, evt.PreviousStart, evt.NewStart, appt.When.End);
                await _sender.SendEmailAsync(patientEmail, subject, body, ct);
                }

            _logger.LogInformation("Appointment rescheduled notifications sent for appointment {AppointmentId}", evt.AppointmentId);
            }
        catch (Exception ex)
            {
            _logger.LogError(ex, "Failed to handle AppointmentRescheduled event for {AppointmentId}", evt.AppointmentId);
            }
        }

    /// <summary>
    /// Handles FollowUpCreated event - sends initial follow-up notification to patient.
    /// </summary>
    public async Task HandleFollowUpCreatedAsync(FollowUpCreated evt, CancellationToken ct)
        {
        try
            {
            var followUp = await _followUps.GetByIdAsync(evt.FollowUpId, ct);
            if (followUp == null) return;

            var appt = await _appointments.GetByIdAsync(followUp.AppointmentId, ct);
            if (appt == null) return;

            var (patientEmail, patientPhone) = await _contacts.GetPatientContactAsync(appt.PatientId, ct);
            var patientName = await _contacts.GetPatientNameAsync(appt.PatientId, ct);
            var doctorName = await _contacts.GetProviderNameAsync(appt.ProviderId, ct);

            if (!string.IsNullOrWhiteSpace(patientEmail))
                {
                var (subject, body) = FollowUpCreated_Patient(patientName, doctorName, followUp.DueBy, followUp.Reason);
                await _sender.SendEmailAsync(patientEmail, subject, body, ct);
                }

            _logger.LogInformation("Follow-up created notification sent for follow-up {FollowUpId}", evt.FollowUpId);
            }
        catch (Exception ex)
            {
            _logger.LogError(ex, "Failed to handle FollowUpCreated event for {FollowUpId}", evt.FollowUpId);
            }
        }

    }
