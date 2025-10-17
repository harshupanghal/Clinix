using Clinix.Domain.Enums;

namespace Clinix.Application.Dtos.Appointment;

public sealed record AppointmentDto(long Id, long DoctorId, long PatientId, DateTimeOffset StartAt, DateTimeOffset EndAt, AppointmentStatus Status, string? Reason, string? Notes);

