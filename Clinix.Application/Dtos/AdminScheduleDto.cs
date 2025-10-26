namespace Clinix.Application.DTOs;

using System.ComponentModel.DataAnnotations;
using Clinix.Domain.Enums;

public record AdminScheduleRequest(
    [Required] DateOnly StartDate,
    [Required] DateOnly EndDate,
    long? DoctorId = null,
    long? ProviderId = null,
    string? Specialty = null,
    List<AppointmentStatus>? Statuses = null,
    bool ShowOnlyAvailable = false,
    int? MinUtilizationPercent = null,
    int? MaxUtilizationPercent = null
);

public record DoctorScheduleSlotDto(
    long ProviderId,
    string DoctorName,
    string Specialty,
    DateTimeOffset Start,
    DateTimeOffset End,
    SlotStatus Status,
    long? AppointmentId,
    string? PatientName,
    AppointmentType? Type,
    AppointmentStatus? AppointmentStatus
);

public record DoctorDayViewDto(
    long ProviderId,
    string DoctorName,
    string Specialty,
    DateOnly Date,
    List<DoctorScheduleSlotDto> Slots,
    int TotalSlots,
    int BookedSlots,
    decimal UtilizationPercent,
    bool IsWorkingDay
);

public record AdminScheduleStatsDto(
    int TotalAppointments,
    int PendingApprovals,
    int AvailableSlots,
    decimal AvgUtilizationPercent,
    int NoShowsToday,
    int CompletedToday
);

public enum SlotStatus
    {
    Available = 0,
    Booked = 1,
    Blocked = 2
    }
