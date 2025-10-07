namespace Clinix.Application.DTOs;

public record AuthenticationResult(
    bool IsSuccess, 
    string? Error,
    long? UserId,
    string? Username,
    string? Email,
    string? Role
    );

