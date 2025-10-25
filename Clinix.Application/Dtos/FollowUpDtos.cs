// Application/DTOs/FollowUpDtos.cs
namespace Clinix.Application.DTOs;

using System.ComponentModel.DataAnnotations;
using Clinix.Domain.Enums;

public record FollowUpDto(long Id, long AppointmentId, DateTimeOffset DueBy, FollowUpStatus Status, string? Reason,
    string? Notes, DateTimeOffset CreatedAt, DateTimeOffset? CompletedAt, DateTimeOffset? UpdatedAt);

public record CreateFollowUpRequest([Required] long AppointmentId, [Required] DateTimeOffset DueBy, [MaxLength(500)] string? Reason);
public record CompleteFollowUpRequest([Required] long FollowUpId, [MaxLength(1000)] string? Notes);
public record CancelFollowUpRequest([Required] long FollowUpId, [MaxLength(1000)] string? Notes);
