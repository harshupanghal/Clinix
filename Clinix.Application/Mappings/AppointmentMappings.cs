namespace Clinix.Application.Mappers;
using Clinix.Application.DTOs;
using Clinix.Domain.Entities;

public static class AppointmentMappings
    {
    /// <summary>
    /// Maps Appointment entity to DTO with patient and doctor names.
    /// Requires Patient and Provider navigation properties to be loaded via .Include()
    /// </summary>
    public static AppointmentDto ToDto(this Appointment e)
        {
        // ✅ Extract patient name from Patient -> User -> FullName
        var patientName = e.Patient?.User?.FullName ?? "Unknown Patient";

        // ✅ Extract doctor name from Provider -> Name (Provider entity has Name property)
        var doctorName = e.Provider?.Name ?? "Unknown Doctor";

        return new AppointmentDto(
            e.Id,
            e.PatientId,
            patientName,      // ✅ NEW: Patient name
            e.ProviderId,
            doctorName,       // ✅ NEW: Doctor name
            e.Type,
            e.Status,
            e.When.Start,
            e.When.End,
            e.Notes,
            e.CreatedAt,
            e.UpdatedAt
        );
        }

    /// <summary>
    /// Maps Appointment entity to summary DTO with names.
    /// </summary>
    public static AppointmentSummaryDto ToSummaryDto(this Appointment e)
        {
        var patientName = e.Patient?.User?.FullName ?? "Unknown Patient";
        var doctorName = e.Provider?.Name ?? "Unknown Doctor";

        return new AppointmentSummaryDto(
            e.Id,
            e.PatientId,
            patientName,      // ✅ NEW: Patient name
            e.ProviderId,
            doctorName,       // ✅ NEW: Doctor name
            e.Type,
            e.Status,
            e.When.Start,
            e.When.End
        );
        }
    }
