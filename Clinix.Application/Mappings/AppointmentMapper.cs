using Clinix.Application.Dtos;

namespace Clinix.Application.Mappings;
public static class AppointmentMapper
    {
    public static Appointment ToEntity(this AppointmentCreateDto dto)
        {
        return new Appointment
            {
            PatientId = dto.PatientId,
            DoctorId = dto.DoctorId,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Reason = dto.Reason,
            Type = dto.Type,
            Status = "Scheduled",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
            };
        }

    public static AppointmentDetailsDto ToDetailsDto(this Appointment entity, string patientName, string doctorName)
        {
        return new AppointmentDetailsDto
            {
            AppointmentId = entity.Id,
            PatientId = entity.PatientId,
            PatientName = patientName,
            DoctorId = entity.DoctorId,
            DoctorName = doctorName,
            StartTime = entity.StartTime,
            EndTime = entity.EndTime,
            Status = entity.Status,
            Reason = entity.Reason,
            Type = entity.Type
            };
        }

    public static AppointmentSlotDto ToSlotDto(this AppointmentSlot slot)
        {
        return new AppointmentSlotDto
            {
            SlotId = slot.Id,
            StartTime = slot.StartUtc,
            EndTime = slot.EndUtc,
            IsBooked = slot.Status != SlotStatus.Available
            };
        }

    public static AppointmentListDto ToListDto(this Appointment entity, string? patientName = null, string? doctorName = null)
        {
        return new AppointmentListDto
            {
            AppointmentId = entity.Id,
            StartTime = entity.StartTime,
            EndTime = entity.EndTime,
            Status = entity.Status,
            PatientName = patientName,
            DoctorName = doctorName
            };
        }
    }
