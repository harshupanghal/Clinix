using Clinix.Application.Dtos.Patient;
using Clinix.Domain.Common;

namespace Clinix.Application.Interfaces.Functionalities;

/// <summary>
/// Services used by the UI to build & update the patient's dashboard.
/// </summary>
public interface IPatientDashboardService
    {
    /// <summary>
    /// Gets dashboard data for the given user id. Returns null when user not found.
    /// </summary>
    Task<PatientDashboardDto?> GetDashboardAsync(long userId, CancellationToken ct = default);

    /// <summary>
    /// Updates patient profile. 'updatedBy' should be the actor (username or "self").
    /// </summary>
    Task<Result> UpdateProfileAsync(PatientUpdateProfileRequest request, string updatedBy, CancellationToken ct = default);
    }

