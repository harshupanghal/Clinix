using System;

namespace Clinix.Application.Dtos;

public record RegisterPatientRequest(
    string Username,
    string Email,
    string Password,
    DateTime? DateOfBirth,
    string? Gender,
    string? BloodGroup,
    string? EmergencyContact
);

