using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Clinix.Application.Services;
using Clinix.Domain.Entities.FollowUps;

namespace Clinix.Infrastructure.Services;

public class InMemoryDispatcher : INotificationDispatcher
    {
    private readonly ILogger<InMemoryDispatcher> _logger;
    public InMemoryDispatcher(ILogger<InMemoryDispatcher> logger) => _logger = logger;

    public Task<bool> DispatchAsync(FollowUpTask task)
        {
        // For now, we just log the payload and return success.
        _logger.LogInformation("Dispatching task {TaskId} type={Type} payload={Payload}", task.Id, task.TaskType, task.Payload);
        return Task.FromResult(true);
        }
    }

