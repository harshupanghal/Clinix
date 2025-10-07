namespace Clinix.Application.DTOs;

public record CreateDoctorRequest(
    string Username,
    string Email,
    string Password,
    string? Degree,
    string? Specialty,
    string? LicenseNumber,
    int? ExperienceYears,
    string? ClinicAddress
);

