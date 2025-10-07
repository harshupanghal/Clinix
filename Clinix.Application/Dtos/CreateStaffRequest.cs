namespace Clinix.Application.DTOs;

public record CreateStaffRequest(
    string Username,
    string Email,
    string Password,
    string Position,
    string? Department,
    string? ShiftInfo
);

