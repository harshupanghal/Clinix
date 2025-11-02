using Clinix.Domain.Events;
using Clinix.Infrastructure.Notifications;
using Clinix.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Clinix.Infrastructure.Background;

/// <summary>
/// Background worker that processes notification events from OutboxMessages table.
/// Runs every 10 seconds to ensure reliable, asynchronous notification delivery.
/// </summary>
public sealed class OutboxProcessorWorker : BackgroundService
    {
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessorWorker> _logger;
    private int _processedCount = 0;
    private int _failedCount = 0;

    public OutboxProcessorWorker(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessorWorker> logger)
        {
        _scopeFactory = scopeFactory;
        _logger = logger;
        }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
        _logger.LogInformation(
            "\n" +
            "╔═══════════════════════════════════════════════════════════════╗\n" +
            "║  🚀 OUTBOX PROCESSOR WORKER STARTED                          ║\n" +
            "║  📦 Polling Interval: 10 seconds                             ║\n" +
            "║  🔄 Max Retries: 3 attempts per message                      ║\n" +
            "╚═══════════════════════════════════════════════════════════════╝");

        var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));

        try
            {
            while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ClinixDbContext>();
                var handlers = scope.ServiceProvider.GetRequiredService<NotificationHandlers>();

                var messages = await db.OutboxMessages
                    .Where(m => !m.Processed && m.Channel == "Notification" && m.AttemptCount < 3)
                    .OrderBy(m => m.OccurredAtUtc)
                    .Take(20)
                    .ToListAsync(stoppingToken);

                if (messages.Any())
                    {
                    _logger.LogInformation(
                        "\n📨 [OUTBOX PROCESSING BATCH]\n" +
                        "   Found {Count} pending messages\n" +
                        "   Timestamp: {Timestamp}",
                        messages.Count,
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    }

                foreach (var msg in messages)
                    {
                    try
                        {
                        _logger.LogInformation(
                            "   ⚙️  Processing Message #{Id} | Type: {Type} | Attempt: {Attempt}",
                            msg.Id, msg.Type, msg.AttemptCount + 1);

                        await ProcessEventAsync(msg, handlers, stoppingToken);

                        msg.Processed = true;
                        msg.ProcessedAtUtc = DateTime.UtcNow;
                        _processedCount++;

                        _logger.LogInformation(
                            "   ✅ Message #{Id} processed successfully\n",
                            msg.Id);
                        }
                    catch (Exception ex)
                        {
                        msg.AttemptCount++;
                        _failedCount++;

                        _logger.LogError(
                            "   ❌ Message #{Id} failed (Attempt {Attempt}/3)\n" +
                            "      Error: {Error}\n",
                            msg.Id, msg.AttemptCount, ex.Message);

                        if (msg.AttemptCount >= 3)
                            {
                            msg.Processed = true;
                            _logger.LogWarning(
                                "   ⚠️  Message #{Id} marked as failed (max retries exceeded)\n",
                                msg.Id);
                            }
                        }
                    }

                if (messages.Any())
                    {
                    await db.SaveChangesAsync(stoppingToken);

                    _logger.LogInformation(
                        "📊 [BATCH COMPLETE] Processed: {Processed} | Failed: {Failed} | Total Stats: ✅ {TotalSuccess} ❌ {TotalFailed}\n",
                        messages.Count(m => m.Processed && m.AttemptCount < 3),
                        messages.Count(m => !m.Processed || m.AttemptCount >= 3),
                        _processedCount,
                        _failedCount);
                    }
                }
            }
        catch (OperationCanceledException)
            {
            _logger.LogInformation("\n🛑 OutboxProcessorWorker stopping gracefully...");
            }
        catch (Exception ex)
            {
            _logger.LogCritical(ex, "\n💥 FATAL ERROR in OutboxProcessorWorker: {Error}", ex.Message);
            }
        finally
            {
            _logger.LogInformation(
                "\n" +
                "╔═══════════════════════════════════════════════════════════════╗\n" +
                "║  🛑 OUTBOX PROCESSOR WORKER STOPPED                          ║\n" +
                "║  📊 Final Stats - Processed: {Processed,-31} ║\n" +
                "║                   Failed: {Failed,-34} ║\n" +
                "╚═══════════════════════════════════════════════════════════════╝",
                _processedCount, _failedCount);
            }
        }

    private async Task ProcessEventAsync(
    Clinix.Domain.Entities.OutboxMessage msg,
    NotificationHandlers handlers,
    CancellationToken ct)
        {
        switch (msg.Type)
            {
            case nameof(AppointmentScheduled):
                var scheduled = JsonSerializer.Deserialize<AppointmentScheduled>(msg.PayloadJson);
                if (scheduled != null)
                    await handlers.HandleAppointmentScheduledAsync(scheduled, ct);
                break;

            case nameof(AppointmentCancelled):
                var cancelled = JsonSerializer.Deserialize<AppointmentCancelled>(msg.PayloadJson);
                if (cancelled != null)
                    await handlers.HandleAppointmentCancelledAsync(cancelled, ct);
                break;

            case nameof(AppointmentRescheduled):
                var rescheduled = JsonSerializer.Deserialize<AppointmentRescheduled>(msg.PayloadJson);
                if (rescheduled != null)
                    await handlers.HandleAppointmentRescheduledAsync(rescheduled, ct);
                break;

            // ✅ NEW CASES
            case nameof(AppointmentCompleted):
                var completed = JsonSerializer.Deserialize<AppointmentCompleted>(msg.PayloadJson);
                if (completed != null)
                    await handlers.HandleAppointmentCompletedAsync(completed, ct);
                break;

            case nameof(AppointmentApproved):
                var approved = JsonSerializer.Deserialize<AppointmentApproved>(msg.PayloadJson);
                if (approved != null)
                    await handlers.HandleAppointmentApprovedAsync(approved, ct);
                break;

            case nameof(AppointmentRejected):
                var rejected = JsonSerializer.Deserialize<AppointmentRejected>(msg.PayloadJson);
                if (rejected != null)
                    await handlers.HandleAppointmentRejectedAsync(rejected, ct);
                break;

            case nameof(FollowUpCreated):
                var followUpCreated = JsonSerializer.Deserialize<FollowUpCreated>(msg.PayloadJson);
                if (followUpCreated != null)
                    await handlers.HandleFollowUpCreatedAsync(followUpCreated, ct);
                break;

            default:
                _logger.LogWarning("      ⚠️  Unknown event type: {Type}", msg.Type);
                break;
            }
        }

    }
