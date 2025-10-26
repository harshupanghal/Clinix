namespace Clinix.Application.Mappers;
using Clinix.Application.DTOs;
using Clinix.Domain.Entities;

public static class AppointmentMappings
    {
    public static AppointmentDto ToDto(this Appointment e) =>
        new AppointmentDto(e.Id, e.PatientId, e.ProviderId, e.Type, e.Status, e.When.Start, e.When.End, e.Notes, e.CreatedAt, e.UpdatedAt);

    public static AppointmentSummaryDto ToSummaryDto(this Appointment e) =>
        new AppointmentSummaryDto(e.Id, e.PatientId, e.ProviderId, e.Type, e.Status, e.When.Start, e.When.End);
    }
