namespace Clinix.Application.Dto;

public record DoctorDto(int Id, string Name, string Specialty);
public record SlotDto(int Id, DateTime StartUtc, DateTime EndUtc);
public record AppointmentSummaryDto(int Id, string Status, string Reason, DateTime CreatedAt, SlotDto Slot, DoctorDto Doctor);
public record CreateAppointmentDto(int? DoctorId, int? AppointmentSlotId, DateTime? PreferredUtc, string Reason);
