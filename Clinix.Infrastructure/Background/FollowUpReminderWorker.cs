using Clinix.Domain.Interfaces;
using Clinix.Infrastructure.Contacts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Clinix.Application.Options;
using Clinix.Application.Interfaces;
using static Clinix.Infrastructure.Notifications.NotificationTemplates;

namespace Clinix.Infrastructure.Background;

/// <summary>
/// Background worker that sends follow-up reminders to patients.
/// Runs every 60 seconds and checks for follow-ups due within configured window.
/// </summary>
public sealed class FollowUpReminderWorker : BackgroundService
    {
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<FollowUpReminderWorker> _logger;
    private readonly ReminderOptions _opts;
    private readonly NotificationsOptions _notify;
    private int _remindersSent = 0;
    private int _remindersFailed = 0;

    public FollowUpReminderWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<ReminderOptions> opts,
        IOptions<NotificationsOptions> notify,
        ILogger<FollowUpReminderWorker> logger)
        {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _opts = opts.Value;
        _notify = notify.Value;
        }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
        _logger.LogInformation(
            "\n" +
            "╔═══════════════════════════════════════════════════════════════╗\n" +
            "║  🔔 FOLLOW-UP REMINDER WORKER STARTED                        ║\n" +
            "║  ⏱️  Check Interval: {Interval,-41} ║\n" +
            "║  🔍 Look Ahead Window: {LookAhead,-37} ║\n" +
            "║  📧 Notifications Enabled: {Enabled,-34} ║\n" +
            "╚═══════════════════════════════════════════════════════════════╝",
            $"{_opts.IntervalSeconds} seconds",
            $"{_opts.LookAheadMinutes} minutes",
            _notify.Enabled ? "Yes" : "No (Dev Mode)");

        var timer = new PeriodicTimer(TimeSpan.FromSeconds(_opts.IntervalSeconds));

        try
            {
            while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                if (!_notify.Enabled)
                    {
                    _logger.LogDebug("⏸️  Reminders paused (Notifications disabled in config)");
                    continue;
                    }

                using var scope = _scopeFactory.CreateScope();
                var followUps = scope.ServiceProvider.GetRequiredService<IFollowUpRepository>();
                var appointments = scope.ServiceProvider.GetRequiredService<IAppointmentRepository>();
                var contacts = scope.ServiceProvider.GetRequiredService<DbContactProvider>();
                var sender = scope.ServiceProvider.GetRequiredService<INotificationSender>();

                var dueUntil = DateTimeOffset.UtcNow.AddMinutes(_opts.LookAheadMinutes);
                var pending = await followUps.GetPendingDueAsync(dueUntil, stoppingToken);

                if (pending.Any())
                    {
                    _logger.LogInformation(
                        "\n🔍 [FOLLOW-UP SCAN]\n" +
                        "   Found {Count} pending follow-ups due by {DueUntil}\n" +
                        "   Scan Time: {ScanTime}",
                        pending.Count,
                        dueUntil.ToString("yyyy-MM-dd HH:mm"),
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    }

                foreach (var fu in pending)
                    {
                    if (fu.LastRemindedAt.HasValue &&
                        fu.LastRemindedAt >= DateTimeOffset.UtcNow.AddMinutes(-_opts.LookAheadMinutes))
                        {
                        _logger.LogDebug("   ⏭️  Follow-up #{Id} already reminded recently, skipping", fu.Id);
                        continue;
                        }

                    try
                        {
                        _logger.LogInformation("   📤 Sending reminder for Follow-up #{Id}...", fu.Id);

                        var appt = await appointments.GetByIdAsync(fu.AppointmentId, stoppingToken);
                        if (appt == null)
                            {
                            _logger.LogWarning("   ⚠️  Appointment #{AppointmentId} not found, skipping", fu.AppointmentId);
                            continue;
                            }

                        var (email, phone) = await contacts.GetPatientContactAsync(appt.PatientId, stoppingToken);
                        var patientName = await contacts.GetPatientNameAsync(appt.PatientId, stoppingToken);
                        var doctorName = await contacts.GetProviderNameAsync(appt.ProviderId, stoppingToken);

                        bool emailSent = false, smsSent = false;

                        if (!string.IsNullOrWhiteSpace(email))
                            {
                            var (subject, body) = FollowUpReminder_Patient(patientName, doctorName, fu.DueBy, fu.Reason);
                            await sender.SendEmailAsync(email, subject, body, stoppingToken);
                            emailSent = true;
                            }

                        if (!string.IsNullOrWhiteSpace(phone))
                            {
                            var sms = FollowUpReminder_SMS(patientName, doctorName, fu.DueBy);
                            await sender.SendSmsAsync(phone, sms, stoppingToken);
                            smsSent = true;
                            }

                        fu.MarkRemindedNow();
                        await followUps.UpdateAsync(fu, stoppingToken);
                        _remindersSent++;

                        _logger.LogInformation(
                            "   ✅ Reminder sent | Email: {Email} | SMS: {Sms}\n",
                            emailSent ? "✓" : "✗",
                            smsSent ? "✓" : "✗");
                        }
                    catch (Exception ex)
                        {
                        _remindersFailed++;
                        _logger.LogError(
                            "   ❌ Failed to send reminder for Follow-up #{FollowUpId}\n" +
                            "      Error: {Error}\n",
                            fu.Id, ex.Message);
                        }
                    }

                if (pending.Any())
                    {
                    _logger.LogInformation(
                        "📊 [REMINDER STATS] Sent: {Sent} | Failed: {Failed} | Total: ✅ {TotalSent} ❌ {TotalFailed}\n",
                        pending.Count - _remindersFailed,
                        _remindersFailed,
                        _remindersSent,
                        _remindersFailed);
                    }
                }
            }
        catch (OperationCanceledException)
            {
            _logger.LogInformation("\n🛑 FollowUpReminderWorker stopping gracefully...");
            }
        catch (Exception ex)
            {
            _logger.LogCritical(ex, "\n💥 FATAL ERROR in FollowUpReminderWorker: {Error}", ex.Message);
            }
        finally
            {
            _logger.LogInformation(
                "\n" +
                "╔═══════════════════════════════════════════════════════════════╗\n" +
                "║  🛑 FOLLOW-UP REMINDER WORKER STOPPED                        ║\n" +
                "║  📊 Final Stats - Sent: {Sent,-40} ║\n" +
                "║                   Failed: {Failed,-37} ║\n" +
                "╚═══════════════════════════════════════════════════════════════╝",
                _remindersSent, _remindersFailed);
            }
        }
    }
