//interface layer -background services and other :

//prepare it properly, please, create a whole workflow also

//using Clinix.Domain.Interfaces;
//using Clinix.Infrastructure.Contacts;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using Clinix.Application.Options;
//using Clinix.Application.Interfaces;
//using static Clinix.Infrastructure.Notifications.NotificationTemplates;

//namespace Clinix.Infrastructure.Background;

///// <summary>
///// Background worker that sends follow-up reminders to patients.
///// Runs every 60 seconds and checks for follow-ups due within configured window.
///// </summary>
//public sealed class FollowUpReminderWorker : BackgroundService
//    {
//    private readonly IServiceScopeFactory _scopeFactory;
//    private readonly ILogger<FollowUpReminderWorker> _logger;
//    private readonly ReminderOptions _opts;
//    private readonly NotificationsOptions _notify;
//    private int _remindersSent = 0;
//    private int _remindersFailed = 0;

//    public FollowUpReminderWorker(
//        IServiceScopeFactory scopeFactory,
//        IOptions<ReminderOptions> opts,
//        IOptions<NotificationsOptions> notify,
//        ILogger<FollowUpReminderWorker> logger)
//        {
//        _scopeFactory = scopeFactory;
//        _logger = logger;
//        _opts = opts.Value;
//        _notify = notify.Value;
//        }

//    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//        _logger.LogInformation(
//            "\n" +
//            "╔═══════════════════════════════════════════════════════════════╗\n" +
//            "║  🔔 FOLLOW-UP REMINDER WORKER STARTED                        ║\n" +
//            "║  ⏱️  Check Interval: {Interval,-41} ║\n" +
//            "║  🔍 Look Ahead Window: {LookAhead,-37} ║\n" +
//            "║  📧 Notifications Enabled: {Enabled,-34} ║\n" +
//            "╚═══════════════════════════════════════════════════════════════╝",
//            $"{_opts.IntervalSeconds} seconds",
//            $"{_opts.LookAheadMinutes} minutes",
//            _notify.Enabled ? "Yes" : "No (Dev Mode)");

//        var timer = new PeriodicTimer(TimeSpan.FromSeconds(_opts.IntervalSeconds));

//        try
//            {
//            while (await timer.WaitForNextTickAsync(stoppingToken))
//                {
//                if (!_notify.Enabled)
//                    {
//                    _logger.LogDebug("⏸️  Reminders paused (Notifications disabled in config)");
//                    continue;
//                    }

//                using var scope = _scopeFactory.CreateScope();
//                var followUps = scope.ServiceProvider.GetRequiredService<IFollowUpRepository>();
//                var appointments = scope.ServiceProvider.GetRequiredService<IAppointmentRepository>();
//                var contacts = scope.ServiceProvider.GetRequiredService<DbContactProvider>();
//                var sender = scope.ServiceProvider.GetRequiredService<INotificationSender>();

//                var dueUntil = DateTimeOffset.UtcNow.AddMinutes(_opts.LookAheadMinutes);
//                var pending = await followUps.GetPendingDueAsync(dueUntil, stoppingToken);

//                if (pending.Any())
//                    {
//                    _logger.LogInformation(
//                        "\n🔍 [FOLLOW-UP SCAN]\n" +
//                        "   Found {Count} pending follow-ups due by {DueUntil}\n" +
//                        "   Scan Time: {ScanTime}",
//                        pending.Count,
//                        dueUntil.ToString("yyyy-MM-dd HH:mm"),
//                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
//                    }

//                foreach (var fu in pending)
//                    {
//                    if (fu.LastRemindedAt.HasValue &&
//                        fu.LastRemindedAt >= DateTimeOffset.UtcNow.AddMinutes(-_opts.LookAheadMinutes))
//                        {
//                        _logger.LogDebug("   ⏭️  Follow-up #{Id} already reminded recently, skipping", fu.Id);
//                        continue;
//                        }

//                    try
//                        {
//                        _logger.LogInformation("   📤 Sending reminder for Follow-up #{Id}...", fu.Id);

//                        var appt = await appointments.GetByIdAsync(fu.AppointmentId, stoppingToken);
//                        if (appt == null)
//                            {
//                            _logger.LogWarning("   ⚠️  Appointment #{AppointmentId} not found, skipping", fu.AppointmentId);
//                            continue;
//                            }

//                        var (email, phone) = await contacts.GetPatientContactAsync(appt.PatientId, stoppingToken);
//                        var patientName = await contacts.GetPatientNameAsync(appt.PatientId, stoppingToken);
//                        var doctorName = await contacts.GetProviderNameAsync(appt.ProviderId, stoppingToken);

//                        bool emailSent = false, smsSent = false;

//                        if (!string.IsNullOrWhiteSpace(email))
//                            {
//                            var (subject, body) = FollowUpReminder_Patient(patientName, doctorName, fu.DueBy, fu.Reason);
//                            await sender.SendEmailAsync(email, subject, body, stoppingToken);
//                            emailSent = true;
//                            }

//                        if (!string.IsNullOrWhiteSpace(phone))
//                            {
//                            var sms = FollowUpReminder_SMS(patientName, doctorName, fu.DueBy);
//                            await sender.SendSmsAsync(phone, sms, stoppingToken);
//                            smsSent = true;
//                            }

//                        fu.MarkRemindedNow();
//                        await followUps.UpdateAsync(fu, stoppingToken);
//                        _remindersSent++;

//                        _logger.LogInformation(
//                            "   ✅ Reminder sent | Email: {Email} | SMS: {Sms}\n",
//                            emailSent ? "✓" : "✗",
//                            smsSent ? "✓" : "✗");
//                        }
//                    catch (Exception ex)
//                        {
//                        _remindersFailed++;
//                        _logger.LogError(
//                            "   ❌ Failed to send reminder for Follow-up #{FollowUpId}\n" +
//                            "      Error: {Error}\n",
//                            fu.Id, ex.Message);
//                        }
//                    }

//                if (pending.Any())
//                    {
//                    _logger.LogInformation(
//                        "📊 [REMINDER STATS] Sent: {Sent} | Failed: {Failed} | Total: ✅ {TotalSent} ❌ {TotalFailed}\n",
//                        pending.Count - _remindersFailed,
//                        _remindersFailed,
//                        _remindersSent,
//                        _remindersFailed);
//                    }
//                }
//            }
//        catch (OperationCanceledException)
//            {
//            _logger.LogInformation("\n🛑 FollowUpReminderWorker stopping gracefully...");
//            }
//        catch (Exception ex)
//            {
//            _logger.LogCritical(ex, "\n💥 FATAL ERROR in FollowUpReminderWorker: {Error}", ex.Message);
//            }
//        finally
//            {
//            _logger.LogInformation(
//                "\n" +
//                "╔═══════════════════════════════════════════════════════════════╗\n" +
//                "║  🛑 FOLLOW-UP REMINDER WORKER STOPPED                        ║\n" +
//                "║  📊 Final Stats - Sent: {Sent,-40} ║\n" +
//                "║                   Failed: {Failed,-37} ║\n" +
//                "╚═══════════════════════════════════════════════════════════════╝",
//                _remindersSent, _remindersFailed);
//            }
//        }
//    }

//using Clinix.Domain.Events;
//using Clinix.Infrastructure.Notifications;
//using Clinix.Infrastructure.Persistence;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using System.Text.Json;

//namespace Clinix.Infrastructure.Background;

///// <summary>
///// Background worker that processes notification events from OutboxMessages table.
///// Runs every 10 seconds to ensure reliable, asynchronous notification delivery.
///// </summary>
//public sealed class OutboxProcessorWorker : BackgroundService
//    {
//    private readonly IServiceScopeFactory _scopeFactory;
//    private readonly ILogger<OutboxProcessorWorker> _logger;
//    private int _processedCount = 0;
//    private int _failedCount = 0;

//    public OutboxProcessorWorker(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessorWorker> logger)
//        {
//        _scopeFactory = scopeFactory;
//        _logger = logger;
//        }

//    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//        _logger.LogInformation(
//            "\n" +
//            "╔═══════════════════════════════════════════════════════════════╗\n" +
//            "║  🚀 OUTBOX PROCESSOR WORKER STARTED                          ║\n" +
//            "║  📦 Polling Interval: 10 seconds                             ║\n" +
//            "║  🔄 Max Retries: 3 attempts per message                      ║\n" +
//            "╚═══════════════════════════════════════════════════════════════╝");

//        var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));

//        try
//            {
//            while (await timer.WaitForNextTickAsync(stoppingToken))
//                {
//                using var scope = _scopeFactory.CreateScope();
//                var db = scope.ServiceProvider.GetRequiredService<ClinixDbContext>();
//                var handlers = scope.ServiceProvider.GetRequiredService<NotificationHandlers>();

//                var messages = await db.OutboxMessages
//                    .Where(m => !m.Processed && m.Channel == "Notification" && m.AttemptCount < 3)
//                    .OrderBy(m => m.OccurredAtUtc)
//                    .Take(20)
//                    .ToListAsync(stoppingToken);

//                if (messages.Any())
//                    {
//                    _logger.LogInformation(
//                        "\n📨 [OUTBOX PROCESSING BATCH]\n" +
//                        "   Found {Count} pending messages\n" +
//                        "   Timestamp: {Timestamp}",
//                        messages.Count,
//                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
//                    }

//                foreach (var msg in messages)
//                    {
//                    try
//                        {
//                        _logger.LogInformation(
//                            "   ⚙️  Processing Message #{Id} | Type: {Type} | Attempt: {Attempt}",
//                            msg.Id, msg.Type, msg.AttemptCount + 1);

//                        await ProcessEventAsync(msg, handlers, stoppingToken);

//                        msg.Processed = true;
//                        msg.ProcessedAtUtc = DateTime.UtcNow;
//                        _processedCount++;

//                        _logger.LogInformation(
//                            "   ✅ Message #{Id} processed successfully\n",
//                            msg.Id);
//                        }
//                    catch (Exception ex)
//                        {
//                        msg.AttemptCount++;
//                        _failedCount++;

//                        _logger.LogError(
//                            "   ❌ Message #{Id} failed (Attempt {Attempt}/3)\n" +
//                            "      Error: {Error}\n",
//                            msg.Id, msg.AttemptCount, ex.Message);

//                        if (msg.AttemptCount >= 3)
//                            {
//                            msg.Processed = true;
//                            _logger.LogWarning(
//                                "   ⚠️  Message #{Id} marked as failed (max retries exceeded)\n",
//                                msg.Id);
//                            }
//                        }
//                    }

//                if (messages.Any())
//                    {
//                    await db.SaveChangesAsync(stoppingToken);

//                    _logger.LogInformation(
//                        "📊 [BATCH COMPLETE] Processed: {Processed} | Failed: {Failed} | Total Stats: ✅ {TotalSuccess} ❌ {TotalFailed}\n",
//                        messages.Count(m => m.Processed && m.AttemptCount < 3),
//                        messages.Count(m => !m.Processed || m.AttemptCount >= 3),
//                        _processedCount,
//                        _failedCount);
//                    }
//                }
//            }
//        catch (OperationCanceledException)
//            {
//            _logger.LogInformation("\n🛑 OutboxProcessorWorker stopping gracefully...");
//            }
//        catch (Exception ex)
//            {
//            _logger.LogCritical(ex, "\n💥 FATAL ERROR in OutboxProcessorWorker: {Error}", ex.Message);
//            }
//        finally
//            {
//            _logger.LogInformation(
//                "\n" +
//                "╔═══════════════════════════════════════════════════════════════╗\n" +
//                "║  🛑 OUTBOX PROCESSOR WORKER STOPPED                          ║\n" +
//                "║  📊 Final Stats - Processed: {Processed,-31} ║\n" +
//                "║                   Failed: {Failed,-34} ║\n" +
//                "╚═══════════════════════════════════════════════════════════════╝",
//                _processedCount, _failedCount);
//            }
//        }

//    private async Task ProcessEventAsync(
//        Clinix.Domain.Entities.OutboxMessage msg,
//        NotificationHandlers handlers,
//        CancellationToken ct)
//        {
//        switch (msg.Type)
//            {
//            case nameof(AppointmentScheduled):
//                var scheduled = JsonSerializer.Deserialize<AppointmentScheduled>(msg.PayloadJson);
//                if (scheduled != null)
//                    await handlers.HandleAppointmentScheduledAsync(scheduled, ct);
//                break;

//            case nameof(AppointmentCancelled):
//                var cancelled = JsonSerializer.Deserialize<AppointmentCancelled>(msg.PayloadJson);
//                if (cancelled != null)
//                    await handlers.HandleAppointmentCancelledAsync(cancelled, ct);
//                break;

//            case nameof(AppointmentRescheduled):
//                var rescheduled = JsonSerializer.Deserialize<AppointmentRescheduled>(msg.PayloadJson);
//                if (rescheduled != null)
//                    await handlers.HandleAppointmentRescheduledAsync(rescheduled, ct);
//                break;

//            case nameof(FollowUpCreated):
//                var followUpCreated = JsonSerializer.Deserialize<FollowUpCreated>(msg.PayloadJson);
//                if (followUpCreated != null)
//                    await handlers.HandleFollowUpCreatedAsync(followUpCreated, ct);
//                break;

//            default:
//                _logger.LogWarning("      ⚠️  Unknown event type: {Type}", msg.Type);
//                break;
//            }
//        }
//    }

//using Clinix.Domain.Abstractions;
//using Clinix.Domain.Entities;
//using Clinix.Infrastructure.Persistence;
//using Microsoft.EntityFrameworkCore;
//using System.Text.Json;

//namespace Clinix.Infrastructure.Events;

///// <summary>
///// Captures domain events from entities and serializes them to OutboxMessages.
///// Does NOT call SaveChanges - that's handled by EF Core after this runs.
///// </summary>
//public sealed class DomainEventDispatcher
//    {
//    private readonly ClinixDbContext _db;

//    public DomainEventDispatcher(ClinixDbContext db) => _db = db;

//    /// <summary>
//    /// Flag to disable event dispatch during seeding.
//    /// Set to true in DataSeeder to prevent notifications for demo data.
//    /// </summary>
//    public static bool IsSeeding { get; set; } = false;

//    /// <summary>
//    /// Extracts domain events and adds them to OutboxMessages (in-memory).
//    /// EF Core will save everything together in one transaction.
//    /// </summary>
//    public void DispatchEvents()
//        {
//        // Skip event dispatch during seeding to avoid demo notifications
//        if (IsSeeding) return;

//        var entities = _db.ChangeTracker.Entries<Entity>()
//            .Where(e => e.Entity.DomainEvents.Any())
//            .Select(e => e.Entity)
//            .ToList();

//        if (!entities.Any()) return;

//        var events = entities.SelectMany(e => e.DomainEvents).ToList();

//        foreach (var evt in events)
//            {
//            var outboxMsg = new OutboxMessage
//                {
//                Type = evt.GetType().Name,
//                PayloadJson = JsonSerializer.Serialize(evt, evt.GetType()),
//                OccurredAtUtc = DateTime.UtcNow,
//                Processed = false,
//                Channel = "Notification"
//                };

//            // Add to context (in-memory) - SaveChanges called by EF Core later
//            _db.OutboxMessages.Add(outboxMsg);
//            }

//        // Clear events after adding to outbox
//        entities.ForEach(e => e.ClearDomainEvents());
//        }
//    }

//using System.Net;
//using System.Net.Mail;
//using Clinix.Application.Interfaces;
//using Clinix.Application.Options;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;

//namespace Clinix.Infrastructure.Messaging;

///// <summary>
///// Production notification sender with detailed SMS logging for development.
///// Email: Sends via SMTP when configured
///// SMS: Logs detailed message content (ready for Twilio integration)
///// </summary>
//public sealed class RealNotificationSender : INotificationSender
//    {
//    private readonly NotificationsOptions _opts;
//    private readonly ILogger<RealNotificationSender> _logger;
//    private readonly bool _twilioConfigured;

//    public RealNotificationSender(IOptions<NotificationsOptions> opts, ILogger<RealNotificationSender> logger)
//        {
//        _opts = opts.Value;
//        _logger = logger;

//        // Check if Twilio is fully configured
//        _twilioConfigured = !string.IsNullOrWhiteSpace(_opts.Twilio.AccountSid)
//                          && !string.IsNullOrWhiteSpace(_opts.Twilio.AuthToken)
//                          && !string.IsNullOrWhiteSpace(_opts.Twilio.FromPhone);
//        }

//    /// <summary>
//    /// Sends email via SMTP. Logs to console if notifications disabled.
//    /// </summary>
//    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken ct = default)
//        {
//        try
//            {
//            if (!_opts.Enabled)
//                {
//                _logger.LogInformation(
//                    "📧 [DEV MODE - EMAIL NOT SENT]\n" +
//                    "   To: {To}\n" +
//                    "   Subject: {Subject}\n" +
//                    "   Body Preview: {BodyPreview}",
//                    to, subject, body.Length > 100 ? body.Substring(0, 100) + "..." : body);
//                return;
//                }

//            // Validate SMTP configuration
//            if (string.IsNullOrWhiteSpace(_opts.Smtp.Host) || string.IsNullOrWhiteSpace(_opts.Smtp.User))
//                {
//                _logger.LogWarning("⚠️ SMTP not configured. Email to {To} not sent.", to);
//                return;
//                }

//            using var client = new SmtpClient(_opts.Smtp.Host, _opts.Smtp.Port)
//                {
//                EnableSsl = _opts.Smtp.EnableSsl,
//                Credentials = new NetworkCredential(_opts.Smtp.User, _opts.Smtp.Password)
//                };

//            using var msg = new MailMessage(
//                new MailAddress(_opts.Smtp.FromEmail, _opts.Smtp.FromName),
//                new MailAddress(to))
//                {
//                Subject = subject,
//                Body = body,
//                IsBodyHtml = false
//                };

//            await client.SendMailAsync(msg, ct);

//            _logger.LogInformation(
//                "✅ [EMAIL SENT SUCCESSFULLY]\n" +
//                "   To: {To}\n" +
//                "   Subject: {Subject}\n" +
//                "   Timestamp: {Timestamp}",
//                to, subject, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
//            }
//        catch (Exception ex)
//            {
//            _logger.LogError(ex,
//                "❌ [EMAIL SEND FAILED]\n" +
//                "   To: {To}\n" +
//                "   Subject: {Subject}\n" +
//                "   Error: {Error}",
//                to, subject, ex.Message);
//            throw;
//            }
//        }

//    /// <summary>
//    /// Sends SMS via Twilio (if configured) or logs detailed message for development.
//    /// Perfect for testing before purchasing Twilio numbers.
//    /// </summary>
//    public async Task SendSmsAsync(string to, string message, CancellationToken ct = default)
//        {
//        try
//            {
//            // Always log SMS content for development/debugging
//            _logger.LogInformation(
//                "📱 [SMS MESSAGE DETAILS]\n" +
//                "   ╔════════════════════════════════════════════════════════════╗\n" +
//                "   ║ TO: {To,-54} ║\n" +
//                "   ║ MESSAGE: {MessagePreview,-48} ║\n" +
//                "   ║ LENGTH: {Length,-51} ║\n" +
//                "   ║ TIMESTAMP: {Timestamp,-47} ║\n" +
//                "   ╠════════════════════════════════════════════════════════════╣\n" +
//                "   ║ FULL MESSAGE:                                             ║\n" +
//                "   ║ {FullMessage,-58}║\n" +
//                "   ╚════════════════════════════════════════════════════════════╝",
//                to,
//                message.Length > 40 ? message.Substring(0, 40) + "..." : message,
//                $"{message.Length} chars",
//                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
//                message.Replace("\n", "\n   ║ "));

//            if (!_opts.Enabled)
//                {
//                _logger.LogInformation("   ⚠️  Notifications disabled - SMS NOT SENT (Dev Mode)");
//                return;
//                }

//            if (!_twilioConfigured)
//                {
//                _logger.LogWarning(
//                    "   ⚠️  Twilio not configured - SMS NOT SENT\n" +
//                    "   📌  Once Twilio is set up, SMS will be automatically sent to: {To}\n" +
//                    "   💡  Add Twilio credentials to appsettings.json under 'Notifications:Twilio'",
//                    to);
//                return;
//                }

//            //  when Twilio is configured
//            /*
//            TwilioClient.Init(_opts.Twilio.AccountSid, _opts.Twilio.AuthToken);
//            var twilioMessage = await MessageResource.CreateAsync(
//                to: new PhoneNumber(to),
//                from: new PhoneNumber(_opts.Twilio.FromPhone),
//                body: message
//            );

//            _logger.LogInformation(
//                "✅ [SMS SENT VIA TWILIO]\n" +
//                "   To: {To}\n" +
//                "   Twilio SID: {Sid}\n" +
//                "   Status: {Status}",
//                to, twilioMessage.Sid, twilioMessage.Status);
//            */

//            await Task.CompletedTask;
//            }
//        catch (Exception ex)
//            {
//            _logger.LogError(ex,
//                "❌ [SMS SEND FAILED]\n" +
//                "   To: {To}\n" +
//                "   Message: {Message}\n" +
//                "   Error: {Error}",
//                to, message, ex.Message);
//            throw;
//            }
//        }
//    }

//using Clinix.Application.Interfaces;
//using Clinix.Domain.Events;
//using Clinix.Domain.Interfaces;
//using Clinix.Infrastructure.Contacts;
//using Microsoft.Extensions.Logging;
//using static Clinix.Infrastructure.Notifications.NotificationTemplates;

//namespace Clinix.Infrastructure.Notifications;

//public sealed class NotificationHandlers
//    {
//    private readonly IAppointmentRepository _appointments;
//    private readonly IFollowUpRepository _followUps;
//    private readonly DbContactProvider _contacts;
//    private readonly INotificationSender _sender;
//    private readonly ILogger<NotificationHandlers> _logger;

//    public NotificationHandlers(
//        IAppointmentRepository appointments,
//        IFollowUpRepository followUps,
//        DbContactProvider contacts,
//        INotificationSender sender,
//        ILogger<NotificationHandlers> logger)
//        {
//        _appointments = appointments;
//        _followUps = followUps;
//        _contacts = contacts;
//        _sender = sender;
//        _logger = logger;
//        }

//    /// <summary>
//    /// Handles AppointmentScheduled event - sends confirmation to BOTH patient and doctor.
//    /// </summary>
//    public async Task HandleAppointmentScheduledAsync(AppointmentScheduled evt, CancellationToken ct)
//        {
//        try
//            {
//            var appt = await _appointments.GetByIdAsync(evt.AppointmentId, ct);
//            if (appt == null) return;

//            // Get contact details
//            var (patientEmail, patientPhone) = await _contacts.GetPatientContactAsync(appt.PatientId, ct);
//            var patientName = await _contacts.GetPatientNameAsync(appt.PatientId, ct);
//            var doctorName = await _contacts.GetProviderNameAsync(appt.ProviderId, ct);

//            // Send to PATIENT
//            if (!string.IsNullOrWhiteSpace(patientEmail))
//                {
//                var (subject, body) = AppointmentScheduled_Patient(patientName, doctorName, appt.When.Start, appt.When.End);
//                await _sender.SendEmailAsync(patientEmail, subject, body, ct);
//                }

//            if (!string.IsNullOrWhiteSpace(patientPhone))
//                {
//                var sms = AppointmentScheduled_SMS_Patient(patientName, doctorName, appt.When.Start);
//                await _sender.SendSmsAsync(patientPhone, sms, ct);
//                }

//            // Send to DOCTOR
//            // ✅ Use DbContactProvider method instead of local method
//            var doctor = await _contacts.GetDoctorByProviderIdAsync(appt.ProviderId, ct);
//            if (doctor != null)
//                {
//                var (docEmail, docPhone) = await _contacts.GetDoctorContactAsync(doctor.DoctorId, ct);
//                if (!string.IsNullOrWhiteSpace(docEmail))
//                    {
//                    var (subject, body) = AppointmentScheduled_Doctor(doctorName, patientName, appt.When.Start, appt.When.End, appt.Type);
//                    await _sender.SendEmailAsync(docEmail, subject, body, ct);
//                    }
//                }

//            _logger.LogInformation("Appointment scheduled notifications sent for appointment {AppointmentId}", evt.AppointmentId);
//            }
//        catch (Exception ex)
//            {
//            _logger.LogError(ex, "Failed to handle AppointmentScheduled event for {AppointmentId}", evt.AppointmentId);
//            }
//        }

//    /// <summary>
//    /// Handles AppointmentCancelled event - notifies BOTH patient and doctor.
//    /// </summary>
//    public async Task HandleAppointmentCancelledAsync(AppointmentCancelled evt, CancellationToken ct)
//        {
//        try
//            {
//            var appt = await _appointments.GetByIdAsync(evt.AppointmentId, ct);
//            if (appt == null) return;

//            var (patientEmail, patientPhone) = await _contacts.GetPatientContactAsync(appt.PatientId, ct);
//            var patientName = await _contacts.GetPatientNameAsync(appt.PatientId, ct);
//            var doctorName = await _contacts.GetProviderNameAsync(appt.ProviderId, ct);

//            // Notify PATIENT
//            if (!string.IsNullOrWhiteSpace(patientEmail))
//                {
//                var (subject, body) = AppointmentCancelled_Patient(patientName, doctorName, appt.When.Start, evt.Reason);
//                await _sender.SendEmailAsync(patientEmail, subject, body, ct);
//                }

//            if (!string.IsNullOrWhiteSpace(patientPhone))
//                {
//                var sms = AppointmentCancelled_SMS(patientName, appt.When.Start);
//                await _sender.SendSmsAsync(patientPhone, sms, ct);
//                }

//            // Notify DOCTOR
//            var doctor = await _contacts.GetDoctorByProviderIdAsync(appt.ProviderId, ct);
//            if (doctor != null)
//                {
//                var (docEmail, _) = await _contacts.GetDoctorContactAsync(doctor.DoctorId, ct);
//                if (!string.IsNullOrWhiteSpace(docEmail))
//                    {
//                    var (subject, body) = AppointmentCancelled_Doctor(doctorName, patientName, appt.When.Start);
//                    await _sender.SendEmailAsync(docEmail, subject, body, ct);
//                    }
//                }

//            _logger.LogInformation("Appointment cancelled notifications sent for appointment {AppointmentId}", evt.AppointmentId);
//            }
//        catch (Exception ex)
//            {
//            _logger.LogError(ex, "Failed to handle AppointmentCancelled event for {AppointmentId}", evt.AppointmentId);
//            }
//        }

//    /// <summary>
//    /// Handles AppointmentRescheduled event - notifies patient and doctor of time change.
//    /// </summary>
//    public async Task HandleAppointmentRescheduledAsync(AppointmentRescheduled evt, CancellationToken ct)
//        {
//        try
//            {
//            var appt = await _appointments.GetByIdAsync(evt.AppointmentId, ct);
//            if (appt == null) return;

//            var (patientEmail, patientPhone) = await _contacts.GetPatientContactAsync(appt.PatientId, ct);
//            var patientName = await _contacts.GetPatientNameAsync(appt.PatientId, ct);
//            var doctorName = await _contacts.GetProviderNameAsync(appt.ProviderId, ct);

//            // Notify PATIENT
//            if (!string.IsNullOrWhiteSpace(patientEmail))
//                {
//                var (subject, body) = AppointmentRescheduled_Patient(
//                    patientName, doctorName, evt.PreviousStart, evt.NewStart, appt.When.End);
//                await _sender.SendEmailAsync(patientEmail, subject, body, ct);
//                }

//            _logger.LogInformation("Appointment rescheduled notifications sent for appointment {AppointmentId}", evt.AppointmentId);
//            }
//        catch (Exception ex)
//            {
//            _logger.LogError(ex, "Failed to handle AppointmentRescheduled event for {AppointmentId}", evt.AppointmentId);
//            }
//        }

//    /// <summary>
//    /// Handles FollowUpCreated event - sends initial follow-up notification to patient.
//    /// </summary>
//    public async Task HandleFollowUpCreatedAsync(FollowUpCreated evt, CancellationToken ct)
//        {
//        try
//            {
//            var followUp = await _followUps.GetByIdAsync(evt.FollowUpId, ct);
//            if (followUp == null) return;

//            var appt = await _appointments.GetByIdAsync(followUp.AppointmentId, ct);
//            if (appt == null) return;

//            var (patientEmail, patientPhone) = await _contacts.GetPatientContactAsync(appt.PatientId, ct);
//            var patientName = await _contacts.GetPatientNameAsync(appt.PatientId, ct);
//            var doctorName = await _contacts.GetProviderNameAsync(appt.ProviderId, ct);

//            if (!string.IsNullOrWhiteSpace(patientEmail))
//                {
//                var (subject, body) = FollowUpCreated_Patient(patientName, doctorName, followUp.DueBy, followUp.Reason);
//                await _sender.SendEmailAsync(patientEmail, subject, body, ct);
//                }

//            _logger.LogInformation("Follow-up created notification sent for follow-up {FollowUpId}", evt.FollowUpId);
//            }
//        catch (Exception ex)
//            {
//            _logger.LogError(ex, "Failed to handle FollowUpCreated event for {FollowUpId}", evt.FollowUpId);
//            }
//        }

//    }

//using Clinix.Domain.Enums;

//namespace Clinix.Infrastructure.Notifications;

///// <summary>
///// Professional, human-friendly notification templates for all appointment and follow-up scenarios.
///// Provides both email (detailed) and SMS (concise) versions with personalization.
///// </summary>
//public static class NotificationTemplates
//    {
//    /// <summary>
//    /// Appointment scheduled confirmation for PATIENT
//    /// </summary>
//    public static (string Subject, string Body) AppointmentScheduled_Patient(
//        string patientName, string doctorName, DateTimeOffset start, DateTimeOffset end)
//        {
//        var subject = "✅ Appointment Confirmed - Clinix HMS";
//        var body = $@"Dear {patientName},

//Your appointment has been successfully scheduled!

//📅 Date: {start:dddd, MMMM dd, yyyy}
//🕐 Time: {start:hh:mm tt} - {end:hh:mm tt}
//👨‍⚕️ Doctor: Dr. {doctorName}

//Please arrive 10 minutes early for check-in.

//If you need to reschedule or cancel, please contact us at least 24 hours in advance.

//Best regards,
//Clinix Hospital Management System
//📞 Support: 1-800-CLINIX
//🌐 www.clinixhms.com";

//        return (subject, body);
//        }


//    /// <summary>
//    /// Appointment scheduled notification for DOCTOR
//    /// </summary>
//    public static (string Subject, string Body) AppointmentScheduled_Doctor(
//        string doctorName, string patientName, DateTimeOffset start, DateTimeOffset end, AppointmentType type)
//        {
//        var subject = "🔔 New Appointment Scheduled";
//        var body = $@"Dear Dr. {doctorName},

//A new appointment has been scheduled with you.

//📋 Patient: {patientName}
//📅 Date: {start:dddd, MMMM dd, yyyy}
//🕐 Time: {start:hh:mm tt} - {end:hh:mm tt}
//🏥 Type: {type}

//Please review patient history before the appointment.

//Clinix HMS
//🌐 Dashboard: www.clinixhms.com/doctor/schedule";

//        return (subject, body);
//        }

//    /// <summary>
//    /// SMS version for appointment scheduled (Patient)
//    /// </summary>
//    public static string AppointmentScheduled_SMS_Patient(string patientName, string doctorName, DateTimeOffset start)
//        => $"Hi {patientName}, your appointment with Dr. {doctorName} is confirmed for {start:MMM dd} at {start:hh:mm tt}. Arrive 10 min early. -Clinix HMS";

//    /// <summary>
//    /// Appointment cancelled notification for PATIENT
//    /// </summary>
//    public static (string Subject, string Body) AppointmentCancelled_Patient(
//        string patientName, string doctorName, DateTimeOffset start, string? reason)
//        {
//        var subject = "❌ Appointment Cancelled - Clinix HMS";
//        var reasonText = !string.IsNullOrWhiteSpace(reason) ? $"\n\nReason: {reason}" : "";
//        var body = $@"Dear {patientName},

//Your appointment has been cancelled.

//📅 Original Date: {start:dddd, MMMM dd, yyyy}
//🕐 Original Time: {start:hh:mm tt}
//👨‍⚕️ Doctor: Dr. {doctorName}{reasonText}

//To reschedule, please contact us or book online.

//Best regards,
//Clinix Hospital Management System
//📞 Support: 1-800-CLINIX";

//        return (subject, body);
//        }

//    /// <summary>
//    /// Appointment cancelled notification for DOCTOR
//    /// </summary>
//    public static (string Subject, string Body) AppointmentCancelled_Doctor(
//        string doctorName, string patientName, DateTimeOffset start)
//        {
//        var subject = "🔔 Appointment Cancelled";
//        var body = $@"Dear Dr. {doctorName},

//An appointment has been cancelled.

//📋 Patient: {patientName}
//📅 Date: {start:dddd, MMMM dd, yyyy}
//🕐 Time: {start:hh:mm tt}

//Your schedule has been updated accordingly.

//Clinix HMS";

//        return (subject, body);
//        }

//    /// <summary>
//    /// SMS for appointment cancelled
//    /// </summary>
//    public static string AppointmentCancelled_SMS(string name, DateTimeOffset start)
//        => $"Hi {name}, your appointment on {start:MMM dd} at {start:hh:mm tt} has been cancelled. Please reschedule if needed. -Clinix HMS";

//    /// <summary>
//    /// Appointment rescheduled for PATIENT
//    /// </summary>
//    public static (string Subject, string Body) AppointmentRescheduled_Patient(
//        string patientName, string doctorName, DateTimeOffset oldStart, DateTimeOffset newStart, DateTimeOffset newEnd)
//        {
//        var subject = "📅 Appointment Rescheduled - Clinix HMS";
//        var body = $@"Dear {patientName},

//Your appointment has been rescheduled.

//❌ Previous: {oldStart:MMM dd} at {oldStart:hh:mm tt}
//✅ New: {newStart:dddd, MMMM dd, yyyy} at {newStart:hh:mm tt} - {newEnd:hh:mm tt}
//👨‍⚕️ Doctor: Dr. {doctorName}

//Please arrive 10 minutes early.

//Best regards,
//Clinix HMS
//📞 Support: 1-800-CLINIX";

//        return (subject, body);
//        }

//    /// <summary>
//    /// 24-hour appointment reminder (SMS - concise)
//    /// </summary>
//    public static string AppointmentReminder_SMS(string patientName, string doctorName, DateTimeOffset start)
//        => $"⏰ Reminder: Your appointment with Dr. {doctorName} is tomorrow at {start:hh:mm tt}. Please confirm or reschedule. -Clinix HMS";

//    /// <summary>
//    /// Follow-up created notification for PATIENT
//    /// </summary>
//    public static (string Subject, string Body) FollowUpCreated_Patient(
//        string patientName, string doctorName, DateTimeOffset dueBy, string? reason)
//        {
//        var subject = "🔄 Follow-Up Required - Clinix HMS";
//        var reasonText = !string.IsNullOrWhiteSpace(reason) ? $"\n\n📝 Reason: {reason}" : "";
//        var body = $@"Dear {patientName},

//A follow-up appointment has been recommended for you.

//👨‍⚕️ Doctor: Dr. {doctorName}
//📅 Due By: {dueBy:dddd, MMMM dd, yyyy}{reasonText}

//Please schedule your follow-up appointment at your earliest convenience.

//To book online, visit: www.clinixhms.com/appointments/schedule
//Or call us at: 1-800-CLINIX

//Your health is our priority!

//Best regards,
//Clinix Hospital Management System";

//        return (subject, body);
//        }

//    /// <summary>
//    /// Follow-up reminder (sent when due date approaches)
//    /// </summary>
//    public static (string Subject, string Body) FollowUpReminder_Patient(
//        string patientName, string doctorName, DateTimeOffset dueBy, string? reason)
//        {
//        var subject = "⚠️ Follow-Up Reminder - Action Required";
//        var reasonText = !string.IsNullOrWhiteSpace(reason) ? $"\n\n📝 Purpose: {reason}" : "";
//        var body = $@"Dear {patientName},

//This is a reminder about your pending follow-up appointment.

//👨‍⚕️ Doctor: Dr. {doctorName}
//📅 Due By: {dueBy:dddd, MMMM dd, yyyy}{reasonText}

//⚠️ Please schedule this follow-up soon to ensure continuity of care.

//📲 Book Now: www.clinixhms.com/appointments/schedule
//📞 Call: 1-800-CLINIX

//We care about your health!

//Best regards,
//Clinix HMS";

//        return (subject, body);
//        }

//    /// <summary>
//    /// SMS for follow-up reminder
//    /// </summary>
//    public static string FollowUpReminder_SMS(string patientName, string doctorName, DateTimeOffset dueBy)
//        => $"⚠️ Hi {patientName}, reminder: Schedule your follow-up with Dr. {doctorName} by {dueBy:MMM dd}. Book at clinixhms.com or call 1-800-CLINIX";

//    /// <summary>
//    /// Marketing/Wellness message template (separate from medical notifications)
//    /// </summary>
//    public static (string Subject, string Body) WellnessMarketing_Patient(string patientName)
//        {
//        var subject = "💚 Your Health Matters - Clinix Wellness Tips";
//        var body = $@"Dear {patientName},

//Thank you for trusting Clinix HMS with your healthcare!

//Here are some wellness tips for you:
//✅ Stay hydrated - drink 8 glasses of water daily
//✅ Exercise regularly - at least 30 minutes a day
//✅ Get adequate sleep - 7-8 hours per night
//✅ Schedule regular check-ups

//We're here to support your health journey!

//📲 Book Your Next Check-Up: www.clinixhms.com
//📞 Questions? Call: 1-800-CLINIX

//Stay healthy!
//Clinix HMS Team";

//        return (subject, body);
//        }
//    }
