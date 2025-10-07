using System.Threading.Tasks;
using Clinix.Application.Dtos;
using Clinix.Application.DTOs;
using Clinix.Application.Interfaces;
using Clinix.Domain.Common;

namespace Clinix.Web.Services;

public class RegistrationUiService : IRegistrationUiService
    {
    private readonly IRegistrationService _registrationService;

    public RegistrationUiService(IRegistrationService registrationService)
        {
        _registrationService = registrationService;
        }

    public Task<Result> RegisterPatientAsync(RegisterPatientRequest request)
        => _registrationService.RegisterPatientAsync(request, createdBy: "self");

    public Task<Result> CreateDoctorAsync(CreateDoctorRequest request, string createdBy)
        => _registrationService.CreateDoctorAsync(request, createdBy);

    public Task<Result> CreateStaffAsync(CreateStaffRequest request, string createdBy)
        => _registrationService.CreateStaffAsync(request, createdBy);
    }

