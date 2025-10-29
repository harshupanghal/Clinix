using Clinix.Application.Dtos.Patient;
using Clinix.Application.Interfaces.Functionalities;
using Clinix.Domain.Common;
using Microsoft.Extensions.Logging;

namespace Clinix.Web.Services;

/// <summary>
/// Thin UI service used by Blazor pages to fetch/update dashboard data.
/// In Blazor Server this calls application services directly.
/// </summary>
public class PatientDashboardUiService : IPatientDashboardUiService
    {
    private readonly IPatientDashboardService _dashboardService;
    private readonly ILogger<PatientDashboardUiService> _logger;

    public PatientDashboardUiService(IPatientDashboardService dashboardService, ILogger<PatientDashboardUiService> logger)
        {
        _dashboardService = dashboardService;
        _logger = logger;
        }

    public Task<PatientDashboardDto?> GetDashboardAsync(long userId, CancellationToken ct = default)
        => _dashboardService.GetDashboardAsync(userId, ct);

    public async Task<Result> UpdateProfileAsync(PatientUpdateProfileRequest request, CancellationToken ct = default)
        {
        try
            {
            var res = await _dashboardService.UpdateProfileAsync(request, updatedBy: "self", ct);
            return res;
            }
        catch (Exception ex)
            {
            _logger.LogError(ex, "Unexpected error in UI service while updating profile for {UserId}", request?.UserId);
            return Result.Failure("An error occurred while updating profile. Please try again later.");
            }
        }
    }

