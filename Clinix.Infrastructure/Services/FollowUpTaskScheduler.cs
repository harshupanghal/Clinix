using Clinix.Application.Interfaces.Functionalities;
using Clinix.Application.Services;
using Clinix.Domain.Entities.FollowUps;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Clinix.Infrastructure.Services;

public class FollowUpTaskScheduler : BackgroundService
    {
    private readonly ILogger<FollowUpTaskScheduler> _logger;
    private readonly IFollowUpTaskRepository _taskRepo;
    private readonly INotificationDispatcher _dispatcher;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(15); // configurable

    public FollowUpTaskScheduler(ILogger<FollowUpTaskScheduler> logger,
                                 IFollowUpTaskRepository taskRepo,
                                 INotificationDispatcher dispatcher)
        {
        _logger = logger;
        _taskRepo = taskRepo;
        _dispatcher = dispatcher;
        }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
        _logger.LogInformation("FollowUpTaskScheduler started.");
        while (!stoppingToken.IsCancellationRequested)
            {
            try
                {
                await ProcessDueTasks(stoppingToken);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Scheduler loop error");
                }

            await Task.Delay(_pollingInterval, stoppingToken);
            }
        _logger.LogInformation("FollowUpTaskScheduler stopped.");
        }

    private async Task ProcessDueTasks(CancellationToken cancellationToken)
        {
        var claimed = await _taskRepo.ClaimDueTasksAsync(DateTimeOffset.UtcNow, batchSize: 50);
        if (claimed == null || claimed.Count == 0)
            {
            return;
            }

        _logger.LogInformation("Scheduler claimed {Count} tasks", claimed.Count);

        var tasks = new List<Task>();
        foreach (var task in claimed)
            {
            tasks.Add(ProcessSingleTaskAsync(task));
            }

        await Task.WhenAll(tasks);
        }

    private async Task ProcessSingleTaskAsync(FollowUpTask task)
        {
        try
            {
            var success = await _dispatcher.DispatchAsync(task);
            // Reload from DB to get fresh rowversion if needed — repo UpdateAsync expects the task instance
            if (success)
                {
                task.MarkCompleted("dispatcher", $"delivered at {DateTimeOffset.UtcNow:o}");
                }
            else
                {
                task.MarkFailed("dispatcher", "dispatch-failed");
                }
            }
        catch (Exception ex)
            {
            _logger.LogError(ex, "Error dispatching task {TaskId}", task.Id);
            task.MarkFailed("dispatcher", $"exception:{ex.Message}");
            }

        // persist changes
        try
            {
            await _taskRepo.UpdateAsync(task);
            }
        catch (Exception ex)
            {
            _logger.LogError(ex, "Failed to persist status for task {TaskId}", task.Id);
            }
        }
    }

