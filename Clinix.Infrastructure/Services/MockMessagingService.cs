//using Clinix.Application.Interfaces.Functionalities;
//using Microsoft.Extensions.Logging;

//namespace Clinix.Infrastructure.Services;

///// <summary>
///// Development mock messaging adapter. Useful for Day-1 integration and tests.
///// Replace with concrete SendGrid/Twilio adapters for production.
///// </summary>
//public class MockMessagingService : IMessagingService
//    {
//    private readonly ILogger<MockMessagingService> _logger;
//    private readonly Random _rng = new Random();

//    public MockMessagingService(ILogger<MockMessagingService> logger)
//        {
//        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//        }

//    public Task<MessagingResult> SendAsync(string channel, string to, string subject, string body, CancellationToken ct = default)
//        {
//        // Basic input checks
//        if (string.IsNullOrWhiteSpace(channel))
//            throw new ArgumentException("channel is required", nameof(channel));

//        // Simulate provider behavior
//        // 95% success, 4% transient failure, 1% permanent failure — deterministic random for tests.
//        var roll = _rng.Next(0, 100);
//        if (roll < 95)
//            {
//            _logger.LogInformation("MockMessagingService: Sent via {Channel} to {To} (length={Len})", channel, MaskContact(to), body?.Length ?? 0);
//            return Task.FromResult(new MessagingResult(true, ProviderMessageId: Guid.NewGuid().ToString()));
//            }
//        else if (roll < 99)
//            {
//            _logger.LogWarning("MockMessagingService: Transient failure sending to {To} channel={Channel}", MaskContact(to), channel);
//            return Task.FromResult(new MessagingResult(false, FailureReason: "TransientNetworkError", IsTransientFailure: true));
//            }
//        else
//            {
//            _logger.LogWarning("MockMessagingService: Permanent failure sending to {To} channel={Channel}", MaskContact(to), channel);
//            return Task.FromResult(new MessagingResult(false, FailureReason: "InvalidRecipient", IsTransientFailure: false));
//            }
//        }

//    private static string MaskContact(string? c)
//        {
//        if (string.IsNullOrEmpty(c)) return "(empty)";
//        if (c.Length <= 4) return c;
//        return new string('*', c.Length - 4) + c.Substring(c.Length - 4);
//        }
//    }

