using Clinix.Application.Dtos;
using Clinix.Application.DTOs;
using Clinix.Application.Interfaces.UserRepo;
using Clinix.Domain.Common;
using Clinix.Domain.Entities.ApplicationUsers;
using Microsoft.Extensions.Logging;

namespace Clinix.Web.Services;

/// <summary>
/// UI-facing service used by Blazor components. In server-side hosting this can call application services directly.
/// Keep this thin and map to application services (don't return raw exceptions).
/// </summary>
public class RegistrationUiService : IRegistrationUiService
    {
    private readonly IRegistrationService _registrationService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<RegistrationUiService> _logger;

    public RegistrationUiService(IRegistrationService registrationService, IUserRepository userRepo, ILogger<RegistrationUiService> logger)
        {
        _registrationService = registrationService;
        _userRepository = userRepo;
        _logger = logger;
        }

    public Task<Result> RegisterPatientAsync(RegisterPatientRequest request)
        => _registrationService.RegisterPatientAsync(request, createdBy: "self");

    public Task<Result> CreateDoctorAsync(CreateDoctorRequest request, string createdBy)
        => _registrationService.CreateDoctorAsync(request, createdBy);

    public Task<Result> CreateStaffAsync(CreateStaffRequest request, string createdBy)
        => _registrationService.CreateStaffAsync(request, createdBy);

    public async Task<bool> IsPhoneTakenAsync(string phone)
        {
        try
            {
            var user = await _userRepository.GetByPhoneAsync(phone, default);
            return user != null;
            }
        catch (Exception ex)
            {
            _logger.LogError(ex, "Error checking phone uniqueness for {Phone}", phone);
            // Fail-safe: return true to avoid accepting a possibly-duplicate phone in a failure scenario
            return true;
            }
        }

    public Task<Result> CompletePatientProfileAsync(CompletePatientProfileRequest request)
        => _registrationService.CompletePatientProfileAsync(request, "self");
    }

