using Clinix.Application.Dtos;
using Clinix.Application.DTOs;
using Clinix.Domain.Common;

namespace Clinix.Application.Interfaces.ServiceInterfaces;

public interface IRegistrationService
    {
    Task<Result> RegisterPatientAsync(RegisterPatientRequest request, string createdBy, CancellationToken ct = default);
    Task<Result> CreateDoctorAsync(CreateDoctorRequest request, string createdBy, CancellationToken ct = default);
    Task<Result> CreateStaffAsync(CreateStaffRequest request, string createdBy, CancellationToken ct = default);
 
    }

