using System;

namespace Clinix.Domain.Entities.ApplicationUsers;

public class User
    {
    public long Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Role { get; set; } = null!; // "Admin","Patient","Doctor","Staff"
    public bool IsDeleted { get; set; } = false;
    public bool IsProfileCompleted { get; set; } = false;

    // Audit
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

