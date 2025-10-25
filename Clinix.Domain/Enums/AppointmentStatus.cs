// Domain/Enums/AppointmentStatus.cs
namespace Clinix.Domain.Enums;

public enum AppointmentStatus
    {
    Pending = 0,        // Waiting for doctor approval
    Confirmed = 1,      // Doctor approved
    Scheduled = 2,      // Alternative to Confirmed (use one or the other)
    Completed = 3,      // Appointment finished
    Cancelled = 4,      // Cancelled by patient or doctor
    Rescheduled = 5,    // Time changed
    NoShow = 6,         // Patient didn't show up
    Rejected = 7        // Doctor rejected
    }
