//using Clinix.Domain.Entities.FollowUp;
//namespace Clinix.Application.Interfaces.Functionalities;

///// <summary>
///// Messaging service abstraction. Implementations must be resilient and idempotent-friendly.
///// </summary>
//public interface IMessagingService
//    {
//    /// <summary>
//    /// Sends a message through the requested channel. Implementations must not throw for expected provider transient errors; prefer returning result objects.
//    /// </summary>
//    Task<MessageTemplate> SendAsync(string channel, string to, string subject, string body, CancellationToken ct = default);
//    }
