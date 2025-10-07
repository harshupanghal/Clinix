using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Clinix.Application.DTOs;
using Clinix.Application.Interfaces;
using Clinix.Domain.Entities;

namespace Clinix.Infrastructure.Services;

public class AuthenticationService : IAuthenticationService
    {
    private readonly IUserRepository _userRepo;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public AuthenticationService(IUserRepository userRepo)
        {
        _userRepo = userRepo;
        }

    public async Task<AuthenticationResult> ValidateCredentialsAsync(string usernameOrEmail, string password, CancellationToken ct = default)
        {
        if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(password))
            return new AuthenticationResult(false, "Username/email and password are required.", null, null,null, null);

        // Try email then username
        var user = await _userRepo.GetByEmailAsync(usernameOrEmail.Trim(), ct);
        if (user == null) user = await _userRepo.GetByUsernameAsync(usernameOrEmail.Trim(), ct);
        if (user == null) return new AuthenticationResult(false, "Invalid credentials.", null, null,null, null);

        var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (verify == PasswordVerificationResult.Success || verify == PasswordVerificationResult.SuccessRehashNeeded)
            {
            return new AuthenticationResult(true, null, user.Id, user.Username, user.Email, user.Role);
            }

        return new AuthenticationResult(false, "Invalid credentials.", null, null,null, null);
        }
    }

