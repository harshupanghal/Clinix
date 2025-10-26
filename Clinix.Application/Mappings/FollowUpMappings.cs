namespace Clinix.Application.Mappers;
using Clinix.Application.DTOs;
using Clinix.Domain.Entities;

public static class FollowUpMappings
    {
    public static FollowUpDto ToDto(this FollowUp e) =>
        new FollowUpDto(e.Id, e.AppointmentId, e.DueBy, e.Status, e.Reason, e.Notes, e.CreatedAt, e.CompletedAt, e.UpdatedAt);
    }
