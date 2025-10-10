using System.Threading.Tasks;
using Azure.Core;
using Clinix.Application.Dtos;
using Clinix.Application.DTOs;
using Clinix.Application.Interfaces.RepoInterfaces;
using Clinix.Application.Interfaces.ServiceInterfaces;
using Clinix.Domain.Common;

namespace Clinix.Web.Services;

public class RegistrationUiService : IRegistrationUiService
    {
    private readonly IRegistrationService _registrationService;
    private readonly IUserRepository _userRepository;

    public RegistrationUiService(IRegistrationService registrationService, IUserRepository userRepo)
        {
        _registrationService = registrationService;
        _userRepository = userRepo;
        }

    public Task<Result> RegisterPatientAsync(RegisterPatientRequest request)
        => _registrationService.RegisterPatientAsync(request, createdBy: "self");

    public Task<Result> CreateDoctorAsync(CreateDoctorRequest request, string createdBy)
        => _registrationService.CreateDoctorAsync(request, createdBy);

    public Task<Result> CreateStaffAsync(CreateStaffRequest request, string createdBy)
        => _registrationService.CreateStaffAsync(request, createdBy);

    //public Task<bool> IsEmailTakenAsync(string Email)
    //    {
    //    CancellationToken ct = default;
    //    return _userRepository.GetByEmailAsync(Email, ct)
    //        .ContinueWith(task => task.Result != null, TaskContinuationOptions.ExecuteSynchronously);
    //    }

    //public Task<bool> IsUsernameTakenAsync(string userName)
    //    {
    //    CancellationToken ct = default;
    //    return _userRepository.GetByUsernameAsync(userName, ct)
    //        .ContinueWith(task => task.Result != null, TaskContinuationOptions.ExecuteSynchronously);
    //    }

    public Task<bool> IsPhoneTakenAsync(string Phone)
        {
        CancellationToken ct = default;
        return _userRepository.GetByPhoneAsync(Phone, ct)
            .ContinueWith(task => task.Result != null, TaskContinuationOptions.ExecuteSynchronously);
        }
    }

