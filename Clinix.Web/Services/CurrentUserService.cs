
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Clinix.Application.Interfaces.UserRepo;

namespace Clinix.Web.Services;

public class CurrentUserService : ICurrentUserService
    {
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly IDoctorRepository _doctorRepo;
    private readonly IPatientRepository _patientRepo;

    public CurrentUserService(
        AuthenticationStateProvider authStateProvider,
        IDoctorRepository doctorRepo,
        IPatientRepository patientRepo)
        {
        _authStateProvider = authStateProvider;
        _doctorRepo = doctorRepo;
        _patientRepo = patientRepo;
        }

    public async Task<CurrentUserInfo> GetCurrentUserAsync()
        {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user?.Identity?.IsAuthenticated != true)
            return new CurrentUserInfo(false, "", "", "");

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? user.FindFirst("sub")?.Value
                  ?? user.FindFirst("userId")?.Value
                  ?? user.FindFirst("id")?.Value
                  ?? "";

        var userName = user.Identity?.Name ?? "User";

        var role = user.FindFirst(ClaimTypes.Role)?.Value
                ?? user.FindFirst("role")?.Value
                ?? "User";

        string? providerId = null;
        string? patientId = null;

        if (role == "Doctor" && long.TryParse(userId, out var doctorUserId))
            {
            var doctor = await _doctorRepo.GetByUserIdAsync(doctorUserId);
            providerId = doctor?.ProviderId.ToString();
            }
        else if (role == "Patient" && long.TryParse(userId, out var patientUserId))
            {
            var patient = await _patientRepo.GetByUserIdAsync(patientUserId);
            patientId = patient?.PatientId.ToString();
            }

        return new CurrentUserInfo(true, userId, userName, role, providerId, patientId);
        }
    }
