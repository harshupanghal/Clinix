namespace Clinix.Application.DTOs;

using Clinix.Domain.Enums;

public record ProviderDto(long Id, string Name, string Specialty, string? Tags, DateTime WorkStart, DateTime WorkEnd);
public record ProviderRecommendationRequest(string Query, AppointmentType Type, DateTimeOffset? DesiredStart);
public record AvailableSlotsRequest(long ProviderId, DateOnly Day);

public record UpdateProviderWorkingHoursRequest(long ProviderId, DateTime WorkStart, DateTime WorkEnd);
