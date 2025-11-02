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

    public async Task HandleAppointmentScheduledAsync(AppointmentScheduled evt, CancellationToken ct)
        {
        try
            {
            _logger.LogInformation("🔔 Handling AppointmentScheduled for ID: {AppointmentId}", evt.AppointmentId);

            var appt = await _appointments.GetByIdAsync(evt.AppointmentId, ct);
            if (appt == null)
                {
                _logger.LogWarning("⚠️ Appointment #{AppointmentId} not found", evt.AppointmentId);
                return;
                }

            var (patientEmail, patientPhone) = await _contacts.GetPatientContactAsync(appt.PatientId, ct);
            var patientName = await _contacts.GetPatientNameAsync(appt.PatientId, ct);
            var doctorName = await _contacts.GetProviderNameAsync(appt.ProviderId, ct);

            // NOTIFY PATIENT
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

            // NOTIFY DOCTOR
            var doctor = await _contacts.GetDoctorByProviderIdAsync(appt.ProviderId, ct);
            if (doctor != null)
                {
                var (docEmail, docPhone) = await _contacts.GetDoctorContactAsync(doctor.DoctorId, ct);

                if (!string.IsNullOrWhiteSpace(docEmail))
                    {
                    var (subject, body) = AppointmentScheduled_Doctor(doctorName, patientName, appt.When.Start, appt.When.End, appt.Type);
                    await _sender.SendEmailAsync(docEmail, subject, body, ct);
                    }

                if (!string.IsNullOrWhiteSpace(docPhone))
                    {
                    var sms = AppointmentScheduled_SMS_Doctor(doctorName, patientName, appt.When.Start);
                    await _sender.SendSmsAsync(docPhone, sms, ct);
                    }
                }

            _logger.LogInformation("✅ Appointment scheduled notifications sent to both parties for appointment {AppointmentId}", evt.AppointmentId);
            }
        catch (Exception ex)
            {
            _logger.LogError(ex, "❌ Failed to handle AppointmentScheduled event for {AppointmentId}", evt.AppointmentId);
            throw; // Re-throw to trigger retry
            }
        }

    public async Task HandleAppointmentRescheduledAsync(AppointmentRescheduled evt, CancellationToken ct)
        {
        try
            {
            var appt = await _appointments.GetByIdAsync(evt.AppointmentId, ct);
            if (appt == null) return;

            var (patientEmail, patientPhone) = await _contacts.GetPatientContactAsync(appt.PatientId, ct);
            var patientName = await _contacts.GetPatientNameAsync(appt.PatientId, ct);
            var doctorName = await _contacts.GetProviderNameAsync(appt.ProviderId, ct);

            // NOTIFY PATIENT
            if (!string.IsNullOrWhiteSpace(patientEmail))
                {
                var (subject, body) = AppointmentRescheduled_Patient(
                    patientName, doctorName, evt.PreviousStart, evt.NewStart, appt.When.End);
                await _sender.SendEmailAsync(patientEmail, subject, body, ct);
                }

            if (!string.IsNullOrWhiteSpace(patientPhone))
                {
                var sms = AppointmentRescheduled_SMS_Patient(patientName, doctorName, evt.NewStart);
                await _sender.SendSmsAsync(patientPhone, sms, ct);
                }

            // NOTIFY DOCTOR
            var doctor = await _contacts.GetDoctorByProviderIdAsync(appt.ProviderId, ct);
            if (doctor != null)
                {
                var (docEmail, docPhone) = await _contacts.GetDoctorContactAsync(doctor.DoctorId, ct);

                if (!string.IsNullOrWhiteSpace(docEmail))
                    {
                    var (subject, body) = AppointmentRescheduled_Doctor(
                        doctorName, patientName, evt.PreviousStart, evt.NewStart, appt.When.End);
                    await _sender.SendEmailAsync(docEmail, subject, body, ct);
                    }

                if (!string.IsNullOrWhiteSpace(docPhone))
                    {
                    var sms = AppointmentRescheduled_SMS_Doctor(patientName, evt.NewStart);
                    await _sender.SendSmsAsync(docPhone, sms, ct);
                    }
                }

            _logger.LogInformation("✅ Appointment rescheduled notifications sent to both parties for appointment {AppointmentId}", evt.AppointmentId);
            }
        catch (Exception ex)
            {
            _logger.LogError(ex, "❌ Failed to handle AppointmentRescheduled event for {AppointmentId}", evt.AppointmentId);
            throw;
            }
        }

    public async Task HandleAppointmentCancelledAsync(AppointmentCancelled evt, CancellationToken ct)
        {
        try
            {
            var appt = await _appointments.GetByIdAsync(evt.AppointmentId, ct);
            if (appt == null) return;

            var (patientEmail, patientPhone) = await _contacts.GetPatientContactAsync(appt.PatientId, ct);
            var patientName = await _contacts.GetPatientNameAsync(appt.PatientId, ct);
            var doctorName = await _contacts.GetProviderNameAsync(appt.ProviderId, ct);

            // NOTIFY PATIENT
            if (!string.IsNullOrWhiteSpace(patientEmail))
                {
                var (subject, body) = AppointmentCancelled_Patient(patientName, doctorName, appt.When.Start, evt.Reason);
                await _sender.SendEmailAsync(patientEmail, subject, body, ct);
                }

            if (!string.IsNullOrWhiteSpace(patientPhone))
                {
                var sms = AppointmentCancelled_SMS_Patient(patientName, appt.When.Start);
                await _sender.SendSmsAsync(patientPhone, sms, ct);
                }

            // NOTIFY DOCTOR
            var doctor = await _contacts.GetDoctorByProviderIdAsync(appt.ProviderId, ct);
            if (doctor != null)
                {
                var (docEmail, docPhone) = await _contacts.GetDoctorContactAsync(doctor.DoctorId, ct);

                if (!string.IsNullOrWhiteSpace(docEmail))
                    {
                    var (subject, body) = AppointmentCancelled_Doctor(doctorName, patientName, appt.When.Start);
                    await _sender.SendEmailAsync(docEmail, subject, body, ct);
                    }

                if (!string.IsNullOrWhiteSpace(docPhone))
                    {
                    var sms = AppointmentCancelled_SMS_Doctor(patientName, appt.When.Start);
                    await _sender.SendSmsAsync(docPhone, sms, ct);
                    }
                }

            _logger.LogInformation("✅ Appointment cancelled notifications sent to both parties for appointment {AppointmentId}", evt.AppointmentId);
            }
        catch (Exception ex)
            {
            _logger.LogError(ex, "❌ Failed to handle AppointmentCancelled event for {AppointmentId}", evt.AppointmentId);
            throw;
            }
        }

    public async Task HandleAppointmentCompletedAsync(AppointmentCompleted evt, CancellationToken ct)
        {
        try
            {
            var appt = await _appointments.GetByIdAsync(evt.AppointmentId, ct);
            if (appt == null) return;

            var (patientEmail, patientPhone) = await _contacts.GetPatientContactAsync(appt.PatientId, ct);
            var patientName = await _contacts.GetPatientNameAsync(appt.PatientId, ct);
            var doctorName = await _contacts.GetProviderNameAsync(appt.ProviderId, ct);

            // NOTIFY PATIENT
            if (!string.IsNullOrWhiteSpace(patientEmail))
                {
                var (subject, body) = AppointmentCompleted_Patient(patientName, doctorName, appt.UpdatedAt ?? DateTimeOffset.UtcNow);
                await _sender.SendEmailAsync(patientEmail, subject, body, ct);
                }

            if (!string.IsNullOrWhiteSpace(patientPhone))
                {
                var sms = AppointmentCompleted_SMS_Patient(patientName, doctorName);
                await _sender.SendSmsAsync(patientPhone, sms, ct);
                }

            // NOTIFY DOCTOR
            var doctor = await _contacts.GetDoctorByProviderIdAsync(appt.ProviderId, ct);
            if (doctor != null)
                {
                var (docEmail, docPhone) = await _contacts.GetDoctorContactAsync(doctor.DoctorId, ct);

                if (!string.IsNullOrWhiteSpace(docEmail))
                    {
                    var (subject, body) = AppointmentCompleted_Doctor(doctorName, patientName, appt.UpdatedAt ?? DateTimeOffset.UtcNow);
                    await _sender.SendEmailAsync(docEmail, subject, body, ct);
                    }

                if (!string.IsNullOrWhiteSpace(docPhone))
                    {
                    var sms = AppointmentCompleted_SMS_Doctor(patientName);
                    await _sender.SendSmsAsync(docPhone, sms, ct);
                    }
                }

            _logger.LogInformation("✅ Appointment completed notifications sent to both parties for appointment {AppointmentId}", evt.AppointmentId);
            }
        catch (Exception ex)
            {
            _logger.LogError(ex, "❌ Failed to handle AppointmentCompleted event for {AppointmentId}", evt.AppointmentId);
            throw;
            }
        }

    public async Task HandleAppointmentApprovedAsync(AppointmentApproved evt, CancellationToken ct)
        {
        try
            {
            var appt = await _appointments.GetByIdAsync(evt.AppointmentId, ct);
            if (appt == null) return;

            var (patientEmail, patientPhone) = await _contacts.GetPatientContactAsync(appt.PatientId, ct);
            var patientName = await _contacts.GetPatientNameAsync(appt.PatientId, ct);
            var doctorName = await _contacts.GetProviderNameAsync(appt.ProviderId, ct);

            // NOTIFY PATIENT
            if (!string.IsNullOrWhiteSpace(patientEmail))
                {
                var (subject, body) = AppointmentApproved_Patient(patientName, doctorName, appt.When.Start, appt.When.End);
                await _sender.SendEmailAsync(patientEmail, subject, body, ct);
                }

            if (!string.IsNullOrWhiteSpace(patientPhone))
                {
                var sms = AppointmentApproved_SMS_Patient(patientName, doctorName, appt.When.Start);
                await _sender.SendSmsAsync(patientPhone, sms, ct);
                }

            // NOTIFY DOCTOR (confirmation)
            var doctor = await _contacts.GetDoctorByProviderIdAsync(appt.ProviderId, ct);
            if (doctor != null)
                {
                var (docEmail, _) = await _contacts.GetDoctorContactAsync(doctor.DoctorId, ct);

                if (!string.IsNullOrWhiteSpace(docEmail))
                    {
                    var (subject, body) = AppointmentApproved_Doctor(doctorName, patientName, appt.When.Start, appt.When.End);
                    await _sender.SendEmailAsync(docEmail, subject, body, ct);
                    }
                }

            _logger.LogInformation("✅ Appointment approved notifications sent to both parties for appointment {AppointmentId}", evt.AppointmentId);
            }
        catch (Exception ex)
            {
            _logger.LogError(ex, "❌ Failed to handle AppointmentApproved event for {AppointmentId}", evt.AppointmentId);
            throw;
            }
        }

    public async Task HandleAppointmentRejectedAsync(AppointmentRejected evt, CancellationToken ct)
        {
        try
            {
            var appt = await _appointments.GetByIdAsync(evt.AppointmentId, ct);
            if (appt == null) return;

            var (patientEmail, patientPhone) = await _contacts.GetPatientContactAsync(appt.PatientId, ct);
            var patientName = await _contacts.GetPatientNameAsync(appt.PatientId, ct);
            var doctorName = await _contacts.GetProviderNameAsync(appt.ProviderId, ct);

            // NOTIFY PATIENT
            if (!string.IsNullOrWhiteSpace(patientEmail))
                {
                var (subject, body) = AppointmentRejected_Patient(patientName, doctorName, appt.When.Start, evt.Reason);
                await _sender.SendEmailAsync(patientEmail, subject, body, ct);
                }

            if (!string.IsNullOrWhiteSpace(patientPhone))
                {
                var sms = AppointmentRejected_SMS_Patient(patientName, doctorName, appt.When.Start);
                await _sender.SendSmsAsync(patientPhone, sms, ct);
                }

            _logger.LogInformation("✅ Appointment rejected notification sent to patient for appointment {AppointmentId}", evt.AppointmentId);
            }
        catch (Exception ex)
            {
            _logger.LogError(ex, "❌ Failed to handle AppointmentRejected event for {AppointmentId}", evt.AppointmentId);
            throw;
            }
        }

    // ✅ FOLLOW-UP: Send ONCE with retry on failure
    public async Task HandleFollowUpCreatedAsync(FollowUpCreated evt, CancellationToken ct)
        {
        try
            {
            var followUp = await _followUps.GetByIdAsync(evt.FollowUpId, ct);
            if (followUp == null) return;

            // ✅ CHECK FLAG: Skip if already notified
            if (followUp.InitialNotificationSent)
                {
                _logger.LogInformation("⏭️ Follow-up #{FollowUpId} initial notification already sent, skipping", evt.FollowUpId);
                return;
                }

            var appt = await _appointments.GetByIdAsync(followUp.AppointmentId, ct);
            if (appt == null) return;

            var (patientEmail, _) = await _contacts.GetPatientContactAsync(appt.PatientId, ct);
            var patientName = await _contacts.GetPatientNameAsync(appt.PatientId, ct);
            var doctorName = await _contacts.GetProviderNameAsync(appt.ProviderId, ct);

            // NOTIFY PATIENT (email only, no SMS to avoid spam)
            if (!string.IsNullOrWhiteSpace(patientEmail))
                {
                var (subject, body) = FollowUpCreated_Patient(patientName, doctorName, followUp.DueBy, followUp.Reason);
                await _sender.SendEmailAsync(patientEmail, subject, body, ct);
                }

            // ✅ MARK AS NOTIFIED to prevent duplicates
            followUp.MarkInitialNotificationSent();
            await _followUps.UpdateAsync(followUp, ct);

            _logger.LogInformation("✅ Follow-up #{FollowUpId} initial notification sent ONCE", evt.FollowUpId);
            }
        catch (Exception ex)
            {
            _logger.LogError(ex, "❌ Failed to handle FollowUpCreated event for {FollowUpId} - will retry", evt.FollowUpId);
            throw; // Re-throw to trigger retry
            }
        }
    }
