using System.Security.Cryptography.X509Certificates;

namespace Clinix.Application.Dtos;

public class RegisterPatientRequest
    {
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string Password { get; set; } = default!;
    public DateTime? DateOfBirth { get; set; } = default!;
    public string? Gender { get; set; } = default!;
    public string? BloodGroup { get; set; } = default!;
    public string? EmergencyContact { get; set; } = default!;
    };

