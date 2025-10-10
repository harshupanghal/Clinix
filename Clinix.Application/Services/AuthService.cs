using Clinix.Application.Dtos;
using Clinix.Application.DTOs;
using Clinix.Application.Interfaces.RepoInterfaces;
using Clinix.Application.Interfaces.ServiceInterfaces;
using Clinix.Application.Mappers;
using Clinix.Application.Mappings;
using Clinix.Domain.Common;
using Clinix.Domain.Entities.ApplicationUsers;
using Microsoft.AspNetCore.Identity;

namespace Clinix.Application.Services;

public class RegistrationService : IRegistrationService
    {
    private readonly IUserRepository _userRepo;
    private readonly IPatientRepository _patientRepo;
    private readonly IDoctorRepository _doctorRepo;
    private readonly IStaffRepository _staffRepo;
    private readonly IUnitOfWork _uow;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public RegistrationService(
        IUserRepository userRepo,
        IPatientRepository patientRepo,
        IDoctorRepository doctorRepo,
        IStaffRepository staffRepo,
        IUnitOfWork uow)
        {
        _userRepo = userRepo;
        _patientRepo = patientRepo;
        _doctorRepo = doctorRepo;
        _staffRepo = staffRepo;
        _uow = uow;
        }

    public async Task<Result> RegisterPatientAsync(RegisterPatientRequest request, string createdBy, CancellationToken ct = default)
        {
        // Basic input checks
        if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Phone) || string.IsNullOrWhiteSpace(request.Password))
            return Result.Failure("FullName, phone and password are required.");

        if (await _userRepo.GetByPhoneAsync(request.Phone, ct) != null)
            return Result.Failure("Phone Number already in use. Try using a different one");

        //if (await _userRepo.GetByUsernameAsync(request.Username, ct) != null)
        //    return Result.Failure("FullName already in use.");

        var user = UserMappers.CreateForRole(request.FullName, request.Email, request.Phone, role: "Patient", createdBy);
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        await _uow.BeginTransactionAsync(ct);
        try
            {
            await _userRepo.AddAsync(user, ct);

            var patient = PatientMappers.CreateFrom(user, request);
            await _patientRepo.AddAsync(patient, ct);

            await _uow.CommitAsync(ct);
            return Result.Success();
            }
        catch (Exception ex)
            {
            await _uow.RollbackAsync(ct);
            return Result.Failure("Error while registering patient: " + ex.Message);
            }
        }

    public async Task<Result> CreateDoctorAsync(CreateDoctorRequest request, string createdBy, CancellationToken ct = default)
        {
        if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Phone) || string.IsNullOrWhiteSpace(request.Password))
            return Result.Failure("FullName, phone and password are required.");

        //if (await _userRepo.GetByEmailAsync(request.Email, ct) != null)
        //    return Result.Failure("Email already in use.");

        if (await _userRepo.GetByPhoneAsync(request.Phone, ct) != null)
            return Result.Failure("Phone Number already in use. Try using a different one");

        var user = UserMappers.CreateForRole(request.FullName, request.Email, request.Phone, role: "Doctor", createdBy);
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        await _uow.BeginTransactionAsync(ct);
        try
            {
            await _userRepo.AddAsync(user, ct);

            var doctor = DoctorMappers.CreateFrom(user, request);
            await _doctorRepo.AddAsync(doctor, ct);

            await _uow.CommitAsync(ct);
            return Result.Success();
            }
        catch (Exception ex)
            {
            await _uow.RollbackAsync(ct);
            return Result.Failure("Error while creating doctor: " + ex.Message);
            }
        }

    public async Task<Result> CreateStaffAsync(CreateStaffRequest request, string createdBy, CancellationToken ct = default)
        {
        if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Phone) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.Position))
            return Result.Failure("FullName, phone, password and position are required.");

        //if (await _userRepo.GetByEmailAsync(request.Email, ct) != null)
        //    return Result.Failure("Email already in use.");

        if (await _userRepo.GetByPhoneAsync(request.Phone, ct) != null)
            return Result.Failure("Phone Number already in use. Try using a different one");

        var user = UserMappers.CreateForRole(request.FullName, request.Email, request.Phone, role: "Staff", createdBy);
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        await _uow.BeginTransactionAsync(ct);
        try
            {
            await _userRepo.AddAsync(user, ct);

            var staff = StaffMappers.CreateFrom(user, request);
            await _staffRepo.AddAsync(staff, ct);

            await _uow.CommitAsync(ct);
            return Result.Success();
            }
        catch (Exception ex)
            {
            await _uow.RollbackAsync(ct);
            return Result.Failure("Error while creating staff: " + ex.Message);
            }
        }
    }




