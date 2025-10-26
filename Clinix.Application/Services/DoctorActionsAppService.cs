namespace Clinix.Application.Services;

using Clinix.Application.Interfaces;
using Clinix.Domain.Interfaces;
using Clinix.Domain.ValueObjects;

public sealed class DoctorActionsAppService : IDoctorActionsAppService
    {
    private readonly IAppointmentRepository _appointments;
    private readonly IProviderRepository _providers;

    public DoctorActionsAppService(IAppointmentRepository appointments, IProviderRepository providers)
        {
        _appointments = appointments;
        _providers = providers;
        }

    public async Task<bool> ApproveAsync(long appointmentId, CancellationToken ct = default)
        {
        var a = await _appointments.GetByIdAsync(appointmentId, ct);
        if (a is null) return false;
        a.Approve();
        await _appointments.UpdateAsync(a, ct);
        return true;
        }

    public async Task<bool> RejectAsync(long appointmentId, string? reason = null, CancellationToken ct = default)
        {
        var a = await _appointments.GetByIdAsync(appointmentId, ct);
        if (a is null) return false;
        a.Reject(reason);
        await _appointments.UpdateAsync(a, ct);
        return true;
        }

    public async Task<bool> DelayCascadeAsync(long appointmentId, TimeSpan delay, CancellationToken ct = default)
        {
        var target = await _appointments.GetByIdAsync(appointmentId, ct);
        if (target is null) return false;

        var provider = await _providers.GetByIdAsync(target.ProviderId, ct)
            ?? throw new KeyNotFoundException("Provider not found.");

        var day = DateOnly.FromDateTime(target.When.Start.LocalDateTime);

        var startTime = provider.WorkStartTime.TimeOfDay;
        var endTime = provider.WorkEndTime.TimeOfDay;

        var dayStart = new DateTimeOffset(
            new DateTime(day.Year, day.Month, day.Day).Add(startTime),
            target.When.Start.Offset);
        var dayEnd = new DateTimeOffset(
            new DateTime(day.Year, day.Month, day.Day).Add(endTime),
            target.When.Start.Offset);

        var appts = (await _appointments.GetByProviderAsync(provider.Id, dayStart, dayEnd, ct))
            .OrderBy(x => x.When.Start)
            .ToList();

        var currentShift = delay;
        DateTimeOffset lastEnd = DateTimeOffset.MinValue;

        foreach (var appt in appts)
            {
            if (appt.When.Start < target.When.Start) continue;

            var proposedStart = appt.When.Start + currentShift;
            var proposedEnd = appt.When.End + currentShift;

            if (lastEnd > DateTimeOffset.MinValue && proposedStart < lastEnd)
                {
                var extra = lastEnd - proposedStart;
                proposedStart += extra;
                proposedEnd += extra;
                currentShift += extra;
                }

            if (proposedEnd > dayEnd)
                {
                day = day.AddDays(1);
                var nextStart = new DateTimeOffset(
                    new DateTime(day.Year, day.Month, day.Day).Add(startTime),
                    proposedEnd.Offset);
                var span = appt.When.End - appt.When.Start;
                proposedStart = nextStart;
                proposedEnd = nextStart + span;
                dayStart = nextStart;
                dayEnd = new DateTimeOffset(
                    new DateTime(day.Year, day.Month, day.Day).Add(endTime),
                    proposedEnd.Offset);
                }

            appt.Reschedule(new DateRange(proposedStart, proposedEnd));
            lastEnd = proposedEnd;
            await _appointments.UpdateAsync(appt, ct);
            }

        return true;
        }
    }
