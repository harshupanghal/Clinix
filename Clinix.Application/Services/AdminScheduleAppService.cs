// Application/Services/AdminScheduleAppService.cs
namespace Clinix.Application.Services;

using Clinix.Application.DTOs;
using Clinix.Application.Interfaces;
using Clinix.Application.Interfaces.UserRepo;
using Clinix.Domain.Entities;
using Clinix.Domain.Enums;
using Clinix.Domain.Interfaces;

public sealed class AdminScheduleAppService : IAdminScheduleAppService
    {
    private readonly IProviderRepository _providers;
    private readonly IAppointmentRepository _appointments;
    private readonly IDoctorScheduleRepository _doctorSchedules;
    private readonly IDoctorRepository _doctors;

    public AdminScheduleAppService(
        IProviderRepository providers,
        IAppointmentRepository appointments,
        IDoctorScheduleRepository doctorSchedules,
        IDoctorRepository doctors)
        {
        _providers = providers;
        _appointments = appointments;
        _doctorSchedules = doctorSchedules;
        _doctors = doctors;
        }

    public async Task<List<DoctorDayViewDto>> GetMasterScheduleAsync(
        AdminScheduleRequest request,
        CancellationToken ct = default)
        {
        // Get providers based on filters
        var providers = await GetFilteredProvidersAsync(request, ct);
        var result = new List<DoctorDayViewDto>();

        foreach (var provider in providers)
            {
            for (var date = request.StartDate; date <= request.EndDate; date = date.AddDays(1))
                {
                var dayView = await BuildDoctorDayViewAsync(provider, date, request, ct);

                // Apply utilization filters
                if (request.MinUtilizationPercent.HasValue &&
                    dayView.UtilizationPercent < request.MinUtilizationPercent.Value)
                    continue;

                if (request.MaxUtilizationPercent.HasValue &&
                    dayView.UtilizationPercent > request.MaxUtilizationPercent.Value)
                    continue;

                result.Add(dayView);
                }
            }

        return result.OrderBy(r => r.Date).ThenBy(r => r.DoctorName).ToList();
        }

    private async Task<List<Provider>> GetFilteredProvidersAsync(
        AdminScheduleRequest request,
        CancellationToken ct)
        {
        var providers = await _providers.SearchAsync(
            request.Specialty != null ? new[] { request.Specialty } : Array.Empty<string>(),
            ct);

        if (request.ProviderId.HasValue)
            providers = providers.Where(p => p.Id == request.ProviderId.Value).ToList();

        if (request.DoctorId.HasValue)
            {
            var doctor = await _doctors.GetByUserIdAsync(request.DoctorId.Value, ct);
            if (doctor != null)
                providers = providers.Where(p => p.Id == doctor.ProviderId).ToList();
            }

        return providers;
        }

    private async Task<DoctorDayViewDto> BuildDoctorDayViewAsync(
     Provider provider,
     DateOnly date,
     AdminScheduleRequest request,
     CancellationToken ct)
        {
        var dayOfWeek = date.DayOfWeek;

        // ✅ FIX: Get all doctors linked to this provider first
        var doctorsForProvider = await _doctors.GetByProviderIdAsync(provider.Id, ct);

        if (!doctorsForProvider.Any())
            {
            // No doctors assigned to this provider
            return new DoctorDayViewDto(
                provider.Id,
                provider.Name,
                provider.Specialty,
                date,
                new List<DoctorScheduleSlotDto>(),
                0, 0, 0,
                IsWorkingDay: false
            );
            }

        // ✅ FIX: Use the first active doctor (or implement better logic)
        var doctor = doctorsForProvider.FirstOrDefault(d => d.IsActive && d.IsOnDuty)
                     ?? doctorsForProvider.First();

        // ✅ FIX: Get schedule by DoctorId, not ProviderId
        var schedule = await _doctorSchedules.GetByDoctorAndDayAsync(doctor.DoctorId, dayOfWeek, ct);

        if (schedule == null || !schedule.IsAvailable)
            {
            return new DoctorDayViewDto(
                provider.Id,
                provider.Name,
                provider.Specialty,
                date,
                new List<DoctorScheduleSlotDto>(),
                0, 0, 0,
                IsWorkingDay: false
            );
            }

        var baseDate = new DateTime(date.Year, date.Month, date.Day);
        var start = new DateTimeOffset(baseDate.Add(schedule.StartTime), DateTimeOffset.Now.Offset);
        var end = new DateTimeOffset(baseDate.Add(schedule.EndTime), DateTimeOffset.Now.Offset);

        // Get appointments for this day
        var appointments = await _appointments.GetByProviderAsync(provider.Id, start, end, ct);

        // Filter by status if requested
        if (request.Statuses?.Any() == true)
            appointments = appointments.Where(a => request.Statuses.Contains(a.Status)).ToList();

        var slots = BuildTimeSlots(provider, start, end, appointments, request.ShowOnlyAvailable);

        var totalSlots = slots.Count;
        var bookedSlots = slots.Count(s => s.Status == SlotStatus.Booked);
        var utilization = totalSlots > 0 ? (decimal)bookedSlots / totalSlots * 100 : 0;

        return new DoctorDayViewDto(
            provider.Id,
            provider.Name,
            provider.Specialty,
            date,
            slots,
            totalSlots,
            bookedSlots,
            utilization,
            IsWorkingDay: true
        );
        }

    private List<DoctorScheduleSlotDto> BuildTimeSlots(
        Provider provider,
        DateTimeOffset start,
        DateTimeOffset end,
        List<Appointment> appointments,
        bool showOnlyAvailable)
        {
        var slots = new List<DoctorScheduleSlotDto>();
        var slotDuration = TimeSpan.FromMinutes(30);

        for (var cursor = start; cursor < end; cursor += slotDuration)
            {
            var slotEnd = cursor + slotDuration;
            var appt = appointments.FirstOrDefault(a =>
                a.When.Start < slotEnd && cursor < a.When.End);

            if (appt != null)
                {
                slots.Add(new DoctorScheduleSlotDto(
                    provider.Id,
                    provider.Name,
                    provider.Specialty,
                    cursor,
                    slotEnd,
                    SlotStatus.Booked,
                    appt.Id,
                    $"Patient #{appt.PatientId}",
                    appt.Type,
                    appt.Status
                ));
                }
            else if (!showOnlyAvailable)
                {
                slots.Add(new DoctorScheduleSlotDto(
                    provider.Id,
                    provider.Name,
                    provider.Specialty,
                    cursor,
                    slotEnd,
                    SlotStatus.Available,
                    null,
                    null,
                    null,
                    null
                ));
                }
            }

        return slots;
        }

    public async Task<AdminScheduleStatsDto> GetDashboardStatsAsync(
        DateOnly date,
        CancellationToken ct = default)
        {
        var start = new DateTimeOffset(
            new DateTime(date.Year, date.Month, date.Day),
            DateTimeOffset.Now.Offset);
        var end = start.AddDays(1);

        var allProviders = await _providers.SearchAsync(Array.Empty<string>(), ct);
        var allAppointments = new List<Appointment>();

        // Gather all appointments for today
        foreach (var provider in allProviders)
            {
            var appts = await _appointments.GetByProviderAsync(provider.Id, start, end, ct);
            allAppointments.AddRange(appts);
            }

        var totalAppts = allAppointments.Count;
        var pending = allAppointments.Count(a => a.Status == AppointmentStatus.Pending);
        var noShows = allAppointments.Count(a => a.Status == AppointmentStatus.NoShow);
        var completed = allAppointments.Count(a => a.Status == AppointmentStatus.Completed);

        // Calculate available slots
        var totalPossibleSlots = 0;
        var bookedSlots = 0;

        var dayOfWeek = date.DayOfWeek;

        foreach (var provider in allProviders)
            {
            var schedules = await _doctorSchedules.GetByProviderAndDayAsync(provider.Id, dayOfWeek, ct);

            foreach (var schedule in schedules.Where(s => s.IsAvailable))
                {
                var duration = schedule.EndTime - schedule.StartTime;
                var slots = (int)(duration.TotalMinutes / 30);
                totalPossibleSlots += slots;
                }

            var providerAppts = allAppointments.Where(a => a.ProviderId == provider.Id).ToList();
            bookedSlots += providerAppts.Count;
            }

        var availableSlots = totalPossibleSlots - bookedSlots;
        var utilization = totalPossibleSlots > 0
            ? (decimal)bookedSlots / totalPossibleSlots * 100
            : 0;

        return new AdminScheduleStatsDto(
            totalAppts,
            pending,
            availableSlots,
            utilization,
            noShows,
            completed
        );
        }

    public async Task<List<string>> GetAllSpecialtiesAsync(CancellationToken ct = default)
        {
        var providers = await _providers.SearchAsync(Array.Empty<string>(), ct);
        return providers
            .Select(p => p.Specialty)
            .Distinct()
            .OrderBy(s => s)
            .ToList();
        }
    }
