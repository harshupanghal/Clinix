using Clinix.Domain.Entities.ApplicationUsers;

namespace Clinix.Application.DTOs;

public record CreateDoctorRequest(
    string FullName,
    string Email,
    string Phone,
    string Password,
    string? Degree,
    string? Specialty,
    string? LicenseNumber,
    int? ExperienceYears,
    string? RoomNumber,
    string? WorkHoursJson,
    string? ExtensionNumber, 
    decimal ConsultationFee,
    string? Notes
);


       