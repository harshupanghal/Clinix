using Clinix.Application.Interfaces;
using Clinix.Application.Options;
using Clinix.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Clinix.Infrastructure.Background;

public sealed class FollowUpReminderWorker : BackgroundService
    {
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<FollowUpReminderWorker> _logger;
    private readonly ReminderOptions _opts;
    private readonly NotificationsOptions _notify;

    public FollowUpReminderWorker(IServiceScopeFactory scopeFactory, IOptions<ReminderOptions> opts, IOptions<NotificationsOptions> notify, ILogger<FollowUpReminderWorker> logger)
        { _scopeFactory = scopeFactory; _logger = logger; _opts = opts.Value; _notify = notify.Value; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
        _logger.LogInformation("FollowUpReminderWorker started.");
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(_opts.IntervalSeconds));
        try
            {
            while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                if (!_notify.Enabled) continue;
                using var scope = _scopeFactory.CreateScope();
                var followUps = scope.ServiceProvider.GetRequiredService<IFollowUpRepository>();
                var appointments = scope.ServiceProvider.GetRequiredService<IAppointmentRepository>();
                var contacts = scope.ServiceProvider.GetRequiredService<IContactProvider>();
                var sender = scope.ServiceProvider.GetRequiredService<INotificationSender>();

                var dueUntil = DateTimeOffset.UtcNow.AddMinutes(_opts.LookAheadMinutes);
                var pending = await followUps.GetPendingDueAsync(dueUntil, stoppingToken);
                foreach (var fu in pending)
                    {
                    if (fu.LastRemindedAt.HasValue && fu.LastRemindedAt >= DateTimeOffset.UtcNow.AddMinutes(-_opts.LookAheadMinutes))
                        continue;

                    var appt = await appointments.GetByIdAsync(fu.AppointmentId, stoppingToken);
                    if (appt is null) continue;

                    var (email, phone) = await contacts.GetPatientContactAsync(appt.PatientId, stoppingToken);
                    var subject = "Follow-up reminder";
                    var body = $"Your follow-up for appointment {appt.Id} is due by {fu.DueBy:u}.";

                    if (!string.IsNullOrWhiteSpace(email)) await sender.SendEmailAsync(email!, subject, body, stoppingToken);
                    if (!string.IsNullOrWhiteSpace(phone)) await sender.SendSmsAsync(phone!, body, stoppingToken);

                    fu.MarkRemindedNow();
                    await followUps.UpdateAsync(fu, stoppingToken);
                    }
                }
            }
        catch (OperationCanceledException) { }
        catch (Exception ex) { _logger.LogError(ex, "Error in FollowUpReminderWorker"); }
        finally { _logger.LogInformation("FollowUpReminderWorker stopped."); }
        }
    }
