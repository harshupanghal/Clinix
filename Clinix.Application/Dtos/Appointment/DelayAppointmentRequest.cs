using System;

namespace Clinix.Application.Dtos.Appointment;

public sealed record DelayAppointmentRequest(long DoctorId, long AppointmentId, TimeSpan DelayBy, long RequestedBy);
