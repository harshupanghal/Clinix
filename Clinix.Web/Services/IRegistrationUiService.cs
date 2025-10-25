using Clinix.Application.Dtos;
using Clinix.Application.Dtos.Patient;
using Clinix.Application.DTOs;
using Clinix.Domain.Common;

namespace Clinix.Web.Services;

public interface IRegistrationUiService
    {
    Task<Result> RegisterPatientAsync(RegisterPatientRequest request);
    Task<Result> CreateDoctorAsync(CreateDoctorRequest request, string createdBy);
    Task<Result> CreateStaffAsync(CreateStaffRequest request, string createdBy);
    Task<bool> IsPhoneTakenAsync(string phone);
    Task<Result> CompletePatientProfileAsync(CompletePatientProfileRequest request);
    }

