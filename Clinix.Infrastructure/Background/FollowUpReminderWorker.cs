using Clinix.Domain.Interfaces;
using Clinix.Infrastructure.Contacts;
using Clinix.Infrastructure.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Clinix.Application.Options;
using Clinix.Application.Interfaces;
using static Clinix.Infrastructure.Notifications.NotificationTemplates;

namespace Clinix.Infrastructure.Background;

/// <summary>
/// Background worker that sends follow-up reminders to patients when their follow-up due date approaches.
/// Runs every 60 seconds and checks for follow-ups due within the next 24 hours.
/// Prevents duplicate reminders by tracking LastRemindedAt timestamp.
/// </summary>
public sealed class FollowUpReminderWorker : BackgroundService
    {
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<FollowUpReminderWorker> _logger;
    private readonly ReminderOptions _opts;
    private readonly NotificationsOptions _notify;

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
        _logger.LogInformation("FollowUpReminderWorker started (Interval: {Interval}s, LookAhead: {LookAhead}min)",
            _opts.IntervalSeconds, _opts.LookAheadMinutes);

        var timer = new PeriodicTimer(TimeSpan.FromSeconds(_opts.IntervalSeconds));

        try
            {
            while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                if (!_notify.Enabled)
                    {
                    _logger.LogDebug("Notifications disabled, skipping reminder check");
                    continue;
                    }

                using var scope = _scopeFactory.CreateScope();
                var followUps = scope.ServiceProvider.GetRequiredService<IFollowUpRepository>();
                var appointments = scope.ServiceProvider.GetRequiredService<IAppointmentRepository>();
                var contacts = scope.ServiceProvider.GetRequiredService<DbContactProvider>();
                var sender = scope.ServiceProvider.GetRequiredService<INotificationSender>();

                // Get follow-ups due within next 24 hours (or configured lookAheadMinutes)
                var dueUntil = DateTimeOffset.UtcNow.AddMinutes(_opts.LookAheadMinutes);
                var pending = await followUps.GetPendingDueAsync(dueUntil, stoppingToken);

                _logger.LogInformation("Found {Count} pending follow-ups due by {DueUntil}", pending.Count, dueUntil);

                foreach (var fu in pending)
                    {
                    // Skip if already reminded recently (within lookAheadMinutes)
                    if (fu.LastRemindedAt.HasValue &&
                        fu.LastRemindedAt >= DateTimeOffset.UtcNow.AddMinutes(-_opts.LookAheadMinutes))
                        {
                        _logger.LogDebug("Follow-up {Id} already reminded recently, skipping", fu.Id);
                        continue;
                        }

                    try
                        {
                        var appt = await appointments.GetByIdAsync(fu.AppointmentId, stoppingToken);
                        if (appt == null)
                            {
                            _logger.LogWarning("Appointment {AppointmentId} not found for follow-up {FollowUpId}",
                                fu.AppointmentId, fu.Id);
                            continue;
                            }

                        var (email, phone) = await contacts.GetPatientContactAsync(appt.PatientId, stoppingToken);
                        var patientName = await contacts.GetPatientNameAsync(appt.PatientId, stoppingToken);
                        var doctorName = await contacts.GetProviderNameAsync(appt.ProviderId, stoppingToken);

                        // Send reminder email
                        if (!string.IsNullOrWhiteSpace(email))
                            {
                            var (subject, body) = FollowUpReminder_Patient(patientName, doctorName, fu.DueBy, fu.Reason);
                            await sender.SendEmailAsync(email, subject, body, stoppingToken);
                            _logger.LogInformation("Follow-up reminder email sent to {Email} for follow-up {Id}", email, fu.Id);
                            }

                        // Send reminder SMS
                        if (!string.IsNullOrWhiteSpace(phone))
                            {
                            var sms = FollowUpReminder_SMS(patientName, doctorName, fu.DueBy);
                            await sender.SendSmsAsync(phone, sms, stoppingToken);
                            _logger.LogInformation("Follow-up reminder SMS sent to {Phone} for follow-up {Id}", phone, fu.Id);
                            }

                        // Mark as reminded
                        fu.MarkRemindedNow();
                        await followUps.UpdateAsync(fu, stoppingToken);
                        }
                    catch (Exception ex)
                        {
                        _logger.LogError(ex, "Failed to send reminder for follow-up {FollowUpId}", fu.Id);
                        }
                    }
                }
            }
        catch (OperationCanceledException)
            {
            _logger.LogInformation("FollowUpReminderWorker stopping gracefully");
            }
        catch (Exception ex)
            {
            _logger.LogError(ex, "Fatal error in FollowUpReminderWorker");
            }
        finally
            {
            _logger.LogInformation("FollowUpReminderWorker stopped");
            }
        }
    }
