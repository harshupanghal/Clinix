// Application/Dtos/CreateDoctorRequest.cs
namespace Clinix.Application.Dtos;

public sealed record CreateDoctorRequest(
    string FullName,
    string Email,
    string Phone,
    string Password,
    string? Degree,
    string? Specialty,
    string? LicenseNumber,
    int? ExperienceYears,
    string? RoomNumber,
    string? ExtensionNumber,
    decimal ConsultationFee,
    string? Notes,
    List<DoctorScheduleDto>? Schedules // NEW: List of weekly schedules
);

// NEW: DTO for individual schedule entries
public sealed record DoctorScheduleDto(
    DayOfWeek DayOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime,
    bool IsAvailable
);
