using Clinix.Application.DTOs;

namespace Clinix.Application.Interfaces.Functionalities;

public interface IAuthenticationService
    {
    /// <summary>
    /// Validate phone + password. Returns AuthenticationResult with user info if success.
    /// </summary>
    Task<LoginResult> ValidateCredentialsAsync(string Phone, string password, CancellationToken ct = default);

    }

