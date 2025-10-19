using System.Threading.Tasks;
using Clinix.Domain.Entities.FollowUps;

namespace Clinix.Application.Services;

public interface INotificationDispatcher
    {
    /// <summary>
    /// Dispatch a followup task to the underlying channel.
    /// Return true when delivery succeeded, false otherwise.
    /// </summary>
    Task<bool> DispatchAsync(FollowUpTask task);
    }

