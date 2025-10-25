// Application/Services/AppointmentAppService.cs
namespace Clinix.Application.Services;
using Clinix.Application.DTOs;
using Clinix.Application.Interfaces;
using Clinix.Application.Mappers;
using Clinix.Domain.Entities;
using Clinix.Domain.Interfaces;
using Clinix.Domain.ValueObjects;

public sealed class AppointmentAppService : IAppointmentAppService
    {
    private readonly IAppointmentRepository _appointments;

    public AppointmentAppService(IAppointmentRepository appointments) => _appointments = appointments;

    public async Task<AppointmentDto> ScheduleAsync(ScheduleAppointmentRequest request, CancellationToken ct = default)
        {
        if (request.End < request.Start) throw new ArgumentException("End must be >= Start");
        var existing = await _appointments.GetByProviderAsync(request.ProviderId, request.Start, request.End, ct);
        var newRange = new DateRange(request.Start, request.End);
        if (existing.Any(a => new DateRange(a.When.Start, a.When.End).Overlaps(newRange)))
            throw new InvalidOperationException("Provider has a conflicting appointment.");

        var appt = Appointment.Schedule(request.PatientId, request.ProviderId, request.Type, newRange, request.Notes);
        await _appointments.AddAsync(appt, ct);
        return appt.ToDto();
        }

    public async Task<AppointmentDto> RescheduleAsync(RescheduleAppointmentRequest request, CancellationToken ct = default)
        {
        var appt = await _appointments.GetByIdAsync(request.AppointmentId, ct) ?? throw new KeyNotFoundException("Appointment not found.");
        var newRange = new DateRange(request.NewStart, request.NewEnd);
        var existing = await _appointments.GetByProviderAsync(appt.ProviderId, request.NewStart, request.NewEnd, ct);
        if (existing.Any(a => a.Id != appt.Id && new DateRange(a.When.Start, a.When.End).Overlaps(newRange)))
            throw new InvalidOperationException("Provider has a conflicting appointment.");
        appt.Reschedule(newRange);
        await _appointments.UpdateAsync(appt, ct);
        return appt.ToDto();
        }

    public async Task<bool> CancelAsync(CancelAppointmentRequest request, CancellationToken ct = default)
        {
        var appt = await _appointments.GetByIdAsync(request.AppointmentId, ct);
        if (appt is null) return false;
        appt.Cancel(request.Reason);
        await _appointments.UpdateAsync(appt, ct);
        return true;
        }

    public async Task<bool> CompleteAsync(CompleteAppointmentRequest request, CancellationToken ct = default)
        {
        var appt = await _appointments.GetByIdAsync(request.AppointmentId, ct);
        if (appt is null) return false;
        appt.Complete();
        appt.CreateFollowUp(DateTimeOffset.UtcNow.AddHours(48), "Automatic follow-up after visit");
        await _appointments.UpdateAsync(appt, ct);
        return true;
        }

    public async Task<List<AppointmentSummaryDto>> GetByProviderAsync(long providerId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default)
        => (await _appointments.GetByProviderAsync(providerId, from, to, ct)).Select(a => a.ToSummaryDto()).ToList();

    public async Task<List<AppointmentSummaryDto>> GetByPatientAsync(long patientId, CancellationToken ct = default)
        => (await _appointments.GetByPatientAsync(patientId, ct)).Select(a => a.ToSummaryDto()).ToList();

    public async Task<AppointmentDto?> GetByIdAsync(long id, CancellationToken ct = default)
        => (await _appointments.GetByIdAsync(id, ct))?.ToDto();
    }
