using Clinix.Application.DTOs;

namespace Clinix.Application.Interfaces.ServiceInterfaces;

public interface IAuthenticationService
    {
    /// <summary>
    /// Validate username/email + password. Returns AuthenticationResult with user info if success.
    /// </summary>
    Task<AuthenticationResult> ValidateCredentialsAsync(string Phone, string password, CancellationToken ct = default);
    //Task<AuthenticationResult> ValidateCredentialsAsync(string EmailOrPhone, string password, CancellationToken ct = default);
    }

