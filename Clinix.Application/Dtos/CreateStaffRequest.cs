namespace Clinix.Application.DTOs;

public record CreateStaffRequest(
    string FullName,
    string? Email,
    string Phone,
    string Password,
    string Position,
    string? Department,
    string? ShiftJson,
    string? AssignedLocation,
    string? SupervisorName,
    string? Notes

    
);

