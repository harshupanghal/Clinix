using Clinix.Application.Dtos;
using Clinix.Domain.Common;

namespace Clinix.Web.Services;

public interface IPatientDashboardUiService
    {
    Task<PatientDashboardDto?> GetDashboardAsync(long userId, CancellationToken ct = default);
    Task<Result> UpdateProfileAsync(PatientUpdateProfileRequest request, CancellationToken ct = default);
    }

