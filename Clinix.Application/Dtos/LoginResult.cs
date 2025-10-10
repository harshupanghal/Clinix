namespace Clinix.Application.DTOs;

public record LoginResult(
    bool IsSuccess,
    string? Error,
    long? UserId,
    string? Fullname,
    string? Email,
    string? Phone,
    string? Role
    );

