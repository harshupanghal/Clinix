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
/// Background worker that polls OutboxMessages table and processes domain events.
/// Ensures reliable notification delivery with automatic retry on failure.
/// Runs every 10 seconds to process pending notifications.
/// </summary>
public sealed class OutboxProcessorWorker : BackgroundService
    {
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessorWorker> _logger;

    public OutboxProcessorWorker(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessorWorker> logger)
        {
        _scopeFactory = scopeFactory;
        _logger = logger;
        }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
        _logger.LogInformation("OutboxProcessorWorker started");
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(100));

        try
            {
            while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ClinixDbContext>();
                var handlers = scope.ServiceProvider.GetRequiredService<NotificationHandlers>();

                // Get unprocessed notification events (max 20 per batch)
                var messages = await db.OutboxMessages
                    .Where(m => !m.Processed && m.Channel == "Notification" && m.AttemptCount < 3)
                    .OrderBy(m => m.OccurredAtUtc)
                    .Take(20)
                    .ToListAsync(stoppingToken);

                foreach (var msg in messages)
                    {
                    try
                        {
                        // Dispatch to appropriate handler based on event type
                        await ProcessEventAsync(msg, handlers, stoppingToken);

                        // Mark as processed
                        msg.Processed = true;
                        msg.ProcessedAtUtc = DateTime.UtcNow;
                        }
                    catch (Exception ex)
                        {
                        _logger.LogError(ex, "Failed to process outbox message {Id} (Type: {Type})", msg.Id, msg.Type);
                        msg.AttemptCount++;

                        // If max retries reached, mark as processed to avoid infinite loop
                        if (msg.AttemptCount >= 3)
                            {
                            msg.Processed = true;
                            _logger.LogWarning("Outbox message {Id} exceeded max retries, marked as processed", msg.Id);
                            }
                        }
                    }

                if (messages.Any())
                    await db.SaveChangesAsync(stoppingToken);
                }
            }
        catch (OperationCanceledException)
            {
            _logger.LogInformation("OutboxProcessorWorker stopping gracefully");
            }
        catch (Exception ex)
            {
            _logger.LogError(ex, "Fatal error in OutboxProcessorWorker");
            }
        finally
            {
            _logger.LogInformation("OutboxProcessorWorker stopped");
            }
        }

    /// <summary>
    /// Routes domain events to appropriate notification handlers based on event type.
    /// </summary>
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

            case nameof(FollowUpCreated):
                var followUpCreated = JsonSerializer.Deserialize<FollowUpCreated>(msg.PayloadJson);
                if (followUpCreated != null)
                    await handlers.HandleFollowUpCreatedAsync(followUpCreated, ct);
                break;

            default:
                _logger.LogWarning("Unknown event type: {Type}", msg.Type);
                break;
            }
        }
    }
