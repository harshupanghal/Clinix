namespace Clinix.Application.Dtos.UserManagement;

/// <summary>
/// DTO for displaying user in admin list with related entity info
/// </summary>
public sealed record UserListDto(
    long Id,
    string FullName,
    string Email,
    string Phone,
    string Role,
    bool IsProfileCompleted,
    DateTime CreatedAt,
    string? CreatedBy,
    // Role-specific data
    string? Specialty,        // For Doctor
    string? Department,       // For Staff
    string? Position,         // For Staff
    string? BloodGroup,       // For Patient
    bool? IsActive            // For Doctor/Patient/Staff
);

public sealed record UserStatsDto(
    int TotalUsers,
    int TotalAdmins,
    int TotalDoctors,
    int TotalPatients,
    int TotalStaff,
    int ActiveUsers,
    int ProfileCompletedCount
);

public sealed record UpdateUserRequest(
    long UserId,
    string FullName,
    string Email,
    string Phone,
    string Role
);

public sealed record UserDetailDto(
    long Id,
    string FullName,
    string Email,
    string Phone,
    string Role,
    bool IsProfileCompleted,
    bool IsDeleted,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string? CreatedBy,
    string? UpdatedBy,
    // Extended info based on role
    DoctorDetailDto? DoctorInfo,
    PatientDetailDto? PatientInfo,
    StaffDetailDto? StaffInfo
);

public sealed record DoctorDetailDto(
    string? Specialty,
    string? Degree,
    string? LicenseNumber,
    int? ExperienceYears,
    decimal? ConsultationFee,
    bool IsOnDuty
);

public sealed record PatientDetailDto(
    string? MedicalRecordNumber,
    string? BloodGroup,
    string? Gender,
    DateTime? DateOfBirth
);

public sealed record StaffDetailDto(
    string Position,
    string? Department,
    string? EmployeeCode,
    DateTime? DateOfJoining
);
