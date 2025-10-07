using System.Threading;
using System.Threading.Tasks;
using Clinix.Application.DTOs;

namespace Clinix.Application.Interfaces;

public interface IAuthenticationService
    {
    /// <summary>
    /// Validate username/email + password. Returns AuthenticationResult with user info if success.
    /// </summary>
    Task<AuthenticationResult> ValidateCredentialsAsync(string usernameOrEmail, string password, CancellationToken ct = default);
    }

