using Clinix.Application.Dtos.Appointment;
using Clinix.Domain.Entities.Appointments;

namespace Clinix.Application.Mappings;

public static class AppointmentMapper
    {
    public static AppointmentDto ToDto(Appointment a) => new AppointmentDto(a.Id, a.DoctorId, a.PatientId, a.StartAt, a.EndAt, a.Status, a.Reason, a.Notes);
    }
