namespace Clinix.Domain.Entities.ApplicationUsers;

public class Staff
    {
    public long UserId { get; set; }
    public User User { get; set; } = null!;

    // Core role info
    public string Position { get; set; } = null!; // e.g. "Receptionist", "Pharmacist", "Nurse", "Lab Technician"
    public string? Department { get; set; } // e.g. "Front Desk", "Pharmacy", "Lab", "Emergency"
    public string? EmployeeCode { get; set; } // internal staff ID

    // Work-related info
    public string? ShiftJson { get; set; } // store structured shift schedule (like WorkHoursJson)
    public string? AssignedLocation { get; set; } // optional, e.g. "Main OPD", "Lab 2", "Pharmacy Desk 1"
    public bool IsActive { get; set; } = true; // quick toggle for employment status

    // Optional details (useful for HR/logistics)
    public DateTime? DateOfJoining { get; set; }
    public string? SupervisorName { get; set; } // e.g. Head Nurse, Chief Pharmacist
    public string? Notes { get; set; } // misc remarks, remarks for admin/internal use

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
