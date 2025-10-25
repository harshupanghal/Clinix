// Application/Services/ProviderAppService.cs
namespace Clinix.Application.Services;

using Clinix.Application.DTOs;
using Clinix.Application.Interfaces;
using Clinix.Domain.Interfaces;

public sealed class ProviderAppService : IProviderAppService
    {
    private readonly IProviderRepository _providers;
    private readonly IAppointmentRepository _appointments;

    public ProviderAppService(IProviderRepository providers, IAppointmentRepository appointments)
        {
        _providers = providers;
        _appointments = appointments;
        }

    public async Task<ProviderDto?> GetByIdAsync(long id, CancellationToken ct = default)
        {
        var p = await _providers.GetByIdAsync(id, ct);
        return p is null ? null : new ProviderDto(p.Id, p.Name, p.Specialty, p.Tags, p.WorkStartTime, p.WorkEndTime);
        }

    // Application/Services/ProviderAppService.cs
    // Application/Services/ProviderAppService.cs
    // Application/Services/ProviderAppService.cs
    public async Task<List<ProviderDto>> RecommendAsync(ProviderRecommendationRequest request, CancellationToken ct = default)
        {
        Console.WriteLine($"[ProviderAppService] RecommendAsync called");
        Console.WriteLine($"[ProviderAppService] Query: '{request.Query}'");

        if (string.IsNullOrWhiteSpace(request.Query))
            {
            Console.WriteLine($"[ProviderAppService] Query is null/empty - returning empty list");
            return new List<ProviderDto>();
            }

        // Split search text into keywords
        var keywords = request.Query
            .ToLowerInvariant()
            .Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(k => k.Trim())
            .Where(k => k.Length >= 2)
            .ToArray();

        Console.WriteLine($"[ProviderAppService] Keywords extracted: {string.Join(", ", keywords)}");

        if (!keywords.Any())
            {
            Console.WriteLine($"[ProviderAppService] No valid keywords - returning empty list");
            return new List<ProviderDto>();
            }

        // Search providers
        Console.WriteLine($"[ProviderAppService] Calling repository.SearchAsync...");
        var providers = await _providers.SearchAsync(keywords, ct);
        Console.WriteLine($"[ProviderAppService] Repository returned {providers.Count} providers");

        var dtos = providers.Select(p => new ProviderDto(
            p.Id,
            p.Name,
            p.Specialty,
            p.Tags ?? "",
            p.WorkStartTime,
            p.WorkEndTime
        )).ToList();

        Console.WriteLine($"[ProviderAppService] Returning {dtos.Count} DTOs");
        return dtos;
        }



    public async Task<List<(DateTimeOffset Start, DateTimeOffset End)>> GetAvailableSlotsAsync(AvailableSlotsRequest req, CancellationToken ct = default)
        {
        var provider = await _providers.GetByIdAsync(req.ProviderId, ct)
            ?? throw new KeyNotFoundException("Provider not found.");

        var baseDate = new DateTime(req.Day.Year, req.Day.Month, req.Day.Day, 0, 0, 0, DateTimeKind.Unspecified);

        // Extract time-of-day from provider's DateTime working hours
        var startTime = provider.WorkStartTime.TimeOfDay;
        var endTime = provider.WorkEndTime.TimeOfDay;

        var start = new DateTimeOffset(baseDate.Add(startTime), DateTimeOffset.Now.Offset);
        var end = new DateTimeOffset(baseDate.Add(endTime), DateTimeOffset.Now.Offset);

        var appts = await _appointments.GetByProviderAsync(req.ProviderId, start, end, ct);
        var busy = appts.Select(a => (a.When.Start, a.When.End)).OrderBy(x => x.Start).ToList();

        var slots = new List<(DateTimeOffset Start, DateTimeOffset End)>();
        var step = TimeSpan.FromMinutes(30);

        for (var cursor = start; cursor + step <= end; cursor += step)
            {
            var candidate = (Start: cursor, End: cursor + step);
            var conflict = busy.Any(b => candidate.Start < b.End && b.Start < candidate.End);
            if (!conflict) slots.Add(candidate);
            }

        return slots;
        }

    public async Task<bool> UpdateWorkingHoursAsync(UpdateProviderWorkingHoursRequest request, CancellationToken ct = default)
        {
        var p = await _providers.GetByIdAsync(request.ProviderId, ct);
        if (p is null) return false;

        p.UpdateWorkingHours(request.WorkStart, request.WorkEnd);
        await _providers.UpdateAsync(p, ct);

        return true;
        }
    }
