namespace Clinix.Application.Dtos;


// For booking a new appointment
public class AppointmentCreateDto
    {
    public long PatientId { get; set; }
    public long DoctorId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Reason { get; set; }
    public string? Type { get; set; } // Consultation, Follow-up
    }


// For updating / rescheduling an appointment
public class AppointmentUpdateDto
    {
    public long AppointmentId { get; set; }
    public DateTime NewStartTime { get; set; }
    public DateTime NewEndTime { get; set; }
    public string? Reason { get; set; }
    public string? Status { get; set; } // Scheduled, Rescheduled, Cancelled
    }


// For showing a single appointment
public class AppointmentDetailsDto
    {
    public long AppointmentId { get; set; }
    public long PatientId { get; set; }
    public string PatientName { get; set; } = null!;
    public long DoctorId { get; set; }
    public string DoctorName { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = null!;
    public string? Reason { get; set; }
    public string? Type { get; set; }
    }


// For listing appointments for doctor or patient 
public class AppointmentListDto
    {
    public long AppointmentId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = null!;
    public string? PatientName { get; set; } // optional if doctor view
    public string? DoctorName { get; set; } // optional if patient view
    }


// For showing available slots for a doctor
public class AppointmentSlotDto
    {
    public long SlotId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsBooked { get; set; }
    }


// Doctor schedule for a date range
public class DoctorScheduleDto
    {
    public long DoctorId { get; set; }
    public string DoctorName { get; set; } = null!;
    public List<AppointmentSlotDto> Slots { get; set; } = new();
    }


// For doctor delaying multiple appointments
public class DelayAppointmentsDto
    {
    public long DoctorId { get; set; }
    public TimeSpan DelayDuration { get; set; }
    public List<long>? AffectedAppointmentIds { get; set; } // optional, filled after processing
    }


// Generic service response wrapper
public class ServiceResult<T>
    {
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    }
