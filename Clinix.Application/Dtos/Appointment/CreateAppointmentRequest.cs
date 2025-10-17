using System;

namespace Clinix.Application.Dtos.Appointment;

public sealed record CreateAppointmentRequest(long DoctorId, long PatientId, DateTimeOffset StartAt, DateTimeOffset EndAt, string? Reason);

