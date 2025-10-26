using Microsoft.AspNetCore.Identity;
using Clinix.Application.DTOs;
using Clinix.Domain.Entities.ApplicationUsers;
using Clinix.Application.Interfaces.UserRepo;
using Clinix.Application.Interfaces.Functionalities;

namespace Clinix.Infrastructure.Services;

public class AuthenticationService : IAuthenticationService
    {
    private readonly IUserRepository _userRepo;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public AuthenticationService(IUserRepository userRepo)
        {
        _userRepo = userRepo;
        }

    public async Task<LoginResult> ValidateCredentialsAsync(string Phone, string password, CancellationToken ct = default)
        {
        if (string.IsNullOrWhiteSpace(Phone) || string.IsNullOrWhiteSpace(password))
            return new LoginResult(false, "Phone number and password are required.", null, null,null,null, null);

        var user = await _userRepo.GetByPhoneAsync(Phone.Trim(), ct);
    
        if (user == null) return new LoginResult(false, "Invalid credentials.", null, null,null,null, null);

        var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (verify == PasswordVerificationResult.Success || verify == PasswordVerificationResult.SuccessRehashNeeded)
            {
            return new LoginResult(true, null, user.Id, user.FullName, user.Email, user.Phone, user.Role);
            }

        return new LoginResult(false, "Invalid credentials.", null, null,null,null, null);
        }
    }

