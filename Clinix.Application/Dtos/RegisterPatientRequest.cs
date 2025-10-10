namespace Clinix.Application.Dtos;

/// <summary>
/// DTO used to register a new patient (initial registration).
/// </summary>
public class RegisterPatientRequest
    {
    /// <summary>Full name of the user (required).</summary>
    public string FullName { get; set; } = default!;

    /// <summary>Email address (recommended, required by validator).</summary>
    public string Email { get; set; } = default!;

    /// <summary>Phone number — stored/compared in normalized form where possible.</summary>
    public string Phone { get; set; } = default!;

    /// <summary>Plain-text password (only transitory; hashed server-side).</summary>
    public string Password { get; set; } = default!;
    }

