// Application/DTOs/AppointmentDtos.cs
namespace Clinix.Application.DTOs;

using System.ComponentModel.DataAnnotations;
using Clinix.Domain.Enums;

public record AppointmentDto(long Id, long PatientId, long ProviderId, AppointmentType Type, AppointmentStatus Status,
    DateTimeOffset Start, DateTimeOffset End, string? Notes, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);

public record AppointmentSummaryDto(long Id, long PatientId, long ProviderId, AppointmentType Type, AppointmentStatus Status,
    DateTimeOffset Start, DateTimeOffset End);

public record ScheduleAppointmentRequest(
    [Required] long PatientId, [Required] long ProviderId, [Required] AppointmentType Type,
    [Required] DateTimeOffset Start, [Required] DateTimeOffset End, [MaxLength(1000)] string? Notes);

public record RescheduleAppointmentRequest([Required] long AppointmentId, [Required] DateTimeOffset NewStart, [Required] DateTimeOffset NewEnd);
public record CancelAppointmentRequest([Required] long AppointmentId, [MaxLength(500)] string? Reason);
public record CompleteAppointmentRequest([Required] long AppointmentId);
