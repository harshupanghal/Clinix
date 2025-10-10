namespace Clinix.Application.DTOs;

public record AuthenticationResult(
    bool IsSuccess,
    string? Error,
    long? UserId,
    string? Fullname,
    string? Email,
    string? Phone,
    string? Role
    );

