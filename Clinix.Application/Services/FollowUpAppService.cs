// Application/Services/FollowUpAppService.cs
namespace Clinix.Application.Services;
using Clinix.Application.DTOs;
using Clinix.Application.Interfaces;
using Clinix.Application.Mappers;
using Clinix.Domain.Interfaces;

public sealed class FollowUpAppService : IFollowUpAppService
    {
    private readonly IAppointmentRepository _appointments;
    private readonly IFollowUpRepository _followUps;

    public FollowUpAppService(IAppointmentRepository appointments, IFollowUpRepository followUps)
        { _appointments = appointments; _followUps = followUps; }

    public async Task<FollowUpDto> CreateAsync(CreateFollowUpRequest request, CancellationToken ct = default)
        {
        var appt = await _appointments.GetByIdAsync(request.AppointmentId, ct) ?? throw new KeyNotFoundException("Appointment not found.");
        var fu = appt.CreateFollowUp(request.DueBy, request.Reason);
        await _followUps.AddAsync(fu, ct);
        await _appointments.UpdateAsync(appt, ct);
        return fu.ToDto();
        }

    public async Task<bool> CompleteAsync(CompleteFollowUpRequest request, CancellationToken ct = default)
        {
        var fu = await _followUps.GetByIdAsync(request.FollowUpId, ct);
        if (fu is null) return false;
        fu.Complete(request.Notes);
        await _followUps.UpdateAsync(fu, ct);
        return true;
        }

    public async Task<bool> CancelAsync(CancelFollowUpRequest request, CancellationToken ct = default)
        {
        var fu = await _followUps.GetByIdAsync(request.FollowUpId, ct);
        if (fu is null) return false;
        fu.Cancel(request.Notes);
        await _followUps.UpdateAsync(fu, ct);
        return true;
        }

    public async Task<List<FollowUpDto>> GetByAppointmentAsync(long appointmentId, CancellationToken ct = default)
        => (await _followUps.GetByAppointmentAsync(appointmentId, ct)).Select(f => f.ToDto()).ToList();

    public async Task<FollowUpDto?> GetByIdAsync(long id, CancellationToken ct = default)
        => (await _followUps.GetByIdAsync(id, ct))?.ToDto();
    }
