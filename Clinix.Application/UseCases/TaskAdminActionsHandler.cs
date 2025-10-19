using Clinix.Application.Dtos.FollowUps;
using Clinix.Application.Interfaces.Functionalities;
using Clinix.Domain.Entities.FollowUps;
using Clinix.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Clinix.Application.UseCases;

public sealed class TaskAdminActionsHandler
    {
    private readonly IFollowUpTaskRepository _taskRepo;
    private readonly ILogger<TaskAdminActionsHandler> _logger;

    public TaskAdminActionsHandler(IFollowUpTaskRepository taskRepo, ILogger<TaskAdminActionsHandler> logger)
        {
        _taskRepo = taskRepo;
        _logger = logger;
        }

    public async Task PauseTaskAsync(AdminTaskActionRequest req)
        {
        if (req.ActorRole != "Admin") throw new UnauthorizedAccessException("Only admins may perform this action.");

        var task = await _taskRepo.GetByIdAsync(req.TaskId);
        if (task == null) throw new InvalidOperationException("Task not found.");

        // Pause: For simplicity, mark as Cancelled with reason "paused" and create a new Task to resume later if needed.
        task.Cancel($"admin:{req.ActorUserId}", req.Reason ?? "paused by admin");
        await _taskRepo.UpdateAsync(task);

        _logger.LogInformation("Admin {User} paused task {TaskId}", req.ActorUserId, req.TaskId);
        }

    public async Task ResumeTaskAsync(AdminTaskActionRequest req, DateTimeOffset scheduleAt)
        {
        if (req.ActorRole != "Admin") throw new UnauthorizedAccessException("Only admins may perform this action.");

        var task = await _taskRepo.GetByIdAsync(req.TaskId);
        if (task == null) throw new InvalidOperationException("Task not found.");

        if (task.Status == FollowUpTaskStatus.Completed) throw new InvalidOperationException("Cannot resume a completed task.");

        // Create a new task to resume (safer than re-using cancelled row)
        var resumed = new FollowUpTask(task.FollowUpRecordId, task.TaskType, task.Payload, scheduleAt, task.MaxAttempts);
        await _taskRepo.AddManyAsync(new[] { resumed });

        _logger.LogInformation("Admin {User} resumed task {OldTaskId} as {NewTaskId}", req.ActorUserId, req.TaskId, resumed.Id);
        }

    public async Task CancelTaskAsync(AdminTaskActionRequest req)
        {
        if (req.ActorRole != "Admin") throw new UnauthorizedAccessException("Only admins may perform this action.");

        var task = await _taskRepo.GetByIdAsync(req.TaskId);
        if (task == null) throw new InvalidOperationException("Task not found.");

        task.Cancel($"admin:{req.ActorUserId}", req.Reason);
        await _taskRepo.UpdateAsync(task);

        _logger.LogInformation("Admin {User} cancelled task {TaskId}", req.ActorUserId, req.TaskId);
        }
    }

