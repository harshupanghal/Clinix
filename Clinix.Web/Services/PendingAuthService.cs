using System.Collections.Concurrent;

namespace Clinix.Web.Services
    {
    /// <summary>
    /// Server-side temporary storage for pending authentication data
    /// Uses in-memory cache with automatic expiration to prevent memory leaks
    /// </summary>
    public interface IPendingAuthService
        {
        string StoreAuthData(PendingAuthData data);
        PendingAuthData? RetrieveAuthData(string token);
        }

    public class PendingAuthService : IPendingAuthService
        {
        private readonly ConcurrentDictionary<string, (PendingAuthData Data, DateTime Expiry)> _storage = new();
        private readonly ILogger<PendingAuthService> _logger;
        private static readonly TimeSpan ExpirationTime = TimeSpan.FromMinutes(2);

        public PendingAuthService(ILogger<PendingAuthService> logger)
            {
            _logger = logger;

            // Start background cleanup task
            Task.Run(CleanupExpiredEntries);
            }

        public string StoreAuthData(PendingAuthData data)
            {
            // Generate secure random token
            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("+", "")
                .Replace("/", "")
                .Replace("=", "");

            var expiry = DateTime.UtcNow.Add(ExpirationTime);
            _storage[token] = (data, expiry);

            _logger.LogDebug("Stored auth data with token {Token}, expires at {Expiry}", token, expiry);
            return token;
            }

        public PendingAuthData? RetrieveAuthData(string token)
            {
            if (_storage.TryRemove(token, out var entry))
                {
                if (entry.Expiry > DateTime.UtcNow)
                    {
                    _logger.LogDebug("Retrieved auth data for token {Token}", token);
                    return entry.Data;
                    }

                _logger.LogWarning("Token {Token} has expired", token);
                }
            else
                {
                _logger.LogWarning("Token {Token} not found", token);
                }

            return null;
            }

        private async Task CleanupExpiredEntries()
            {
            while (true)
                {
                await Task.Delay(TimeSpan.FromMinutes(1));

                var now = DateTime.UtcNow;
                var expiredKeys = _storage
                    .Where(kvp => kvp.Value.Expiry <= now)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in expiredKeys)
                    {
                    _storage.TryRemove(key, out _);
                    }

                if (expiredKeys.Count > 0)
                    {
                    _logger.LogDebug("Cleaned up {Count} expired auth tokens", expiredKeys.Count);
                    }
                }
            }
        }

    public sealed class PendingAuthData
        {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
        }
    }