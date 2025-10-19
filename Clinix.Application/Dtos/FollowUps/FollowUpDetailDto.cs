using System.Collections.Generic;

namespace Clinix.Application.Dtos.FollowUps;

public sealed class FollowUpDetailDto
    {
    public long Id { get; init; }
    public string PatientName { get; init; } = string.Empty;
    public string DoctorName { get; init; } = string.Empty;
    public DateTimeOffset AppointmentDate { get; init; }
    public string? Diagnosis { get; init; }
    public string? Notes { get; init; }
    public List<FollowUpTaskDto> Tasks { get; init; } = new();
    }
