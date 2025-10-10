using Clinix.Application.Dtos;
using Clinix.Application.DTOs;
using Clinix.Application.Interfaces.Functionalities;
using Clinix.Application.Interfaces.UserRepo;
using Clinix.Application.Mappings;
using Clinix.Domain.Common;
using Clinix.Domain.Entities.ApplicationUsers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Clinix.Application.Services;

/// <summary>
/// Handles registration and profile completion flows.
/// </summary>
public class RegistrationService : IRegistrationService
    {
    private readonly IUserRepository _userRepo;
    private readonly IPatientRepository _patientRepo;
    private readonly IDoctorRepository _doctorRepo;
    private readonly IStaffRepository _staffRepo;
    private readonly IUnitOfWork _uow;
    private readonly PasswordHasher<User> _passwordHasher = new();
    private readonly ILogger<RegistrationService> _logger;

    public RegistrationService(
        IUserRepository userRepo,
        IPatientRepository patientRepo,
        IDoctorRepository doctorRepo,
        IStaffRepository staffRepo,
        IUnitOfWork uow,
        ILogger<RegistrationService> logger)
        {
        _userRepo = userRepo;
        _patientRepo = patientRepo;
        _doctorRepo = doctorRepo;
        _staffRepo = staffRepo;
        _uow = uow;
        _logger = logger;
        }

    /// <summary>
    /// Registers a patient (creates User row). Password is hashed server-side.
    /// </summary>
    public async Task<Result> RegisterPatientAsync(RegisterPatientRequest request, string createdBy, CancellationToken ct = default)
        {
        if (request is null)
            return Result.Failure("Invalid request.");

        if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Phone) || string.IsNullOrWhiteSpace(request.Password))
            return Result.Failure("FullName, phone and password are required.");

        var normalizedPhone = NormalizePhone(request.Phone);
        if (string.IsNullOrEmpty(normalizedPhone))
            return Result.Failure("Invalid phone number.");

        try
            {
            var existing = await _userRepo.GetByPhoneAsync(normalizedPhone, ct);
            if (existing != null)
                return Result.Failure("Phone number already in use.");

            var emailNormalized = request.Email?.Trim();

            var user = UserMappers.CreateForRole(request.FullName.Trim(), emailNormalized ?? string.Empty, normalizedPhone, role: "Patient", createdBy);
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            user.CreatedBy = createdBy;
            user.UpdatedBy = createdBy;
            user.IsProfileCompleted = false;

            await _uow.BeginTransactionAsync(ct);
            try
                {
                await _userRepo.AddAsync(user, ct);
                await _uow.CommitAsync(ct);
                _logger.LogInformation("New patient registered with phone {Phone} and id {UserId}", normalizedPhone, user.Id);
                return Result.Success("Registration successful. Please login to continue.");
                }
            catch (DbUpdateException dbEx)
                {
                await _uow.RollbackAsync(ct);
                _logger.LogError(dbEx, "DB error while registering user with phone {Phone}", normalizedPhone);
                // If unique constraint violation, return friendly message
                return Result.Failure("Phone number already in use.");
                }
            }
        catch (Exception ex)
            {
            _logger.LogError(ex, "Unexpected error while registering patient with phone {Phone}", request.Phone);
            return Result.Failure("An error occurred while registering. Please try again later.");
            }
        }

    public async Task<Result> CreateDoctorAsync(CreateDoctorRequest request, string createdBy, CancellationToken ct = default)
        {
        if (request is null)
            return Result.Failure("Invalid request.");

        if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Phone) || string.IsNullOrWhiteSpace(request.Password))
            return Result.Failure("FullName, phone and password are required.");

        var normalizedPhone = NormalizePhone(request.Phone);
        if (normalizedPhone == null)
            return Result.Failure("Invalid phone number.");

        try
            {
            if (await _userRepo.GetByPhoneAsync(normalizedPhone, ct) != null)
                return Result.Failure("Phone Number already in use. Try a different one.");

            var user = UserMappers.CreateForRole(request.FullName.Trim(), request.Email?.Trim(), normalizedPhone, role: "Doctor", createdBy);
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            user.CreatedBy = createdBy;

            await _uow.BeginTransactionAsync(ct);
            try
                {
                await _userRepo.AddAsync(user, ct);
                var doctor = DoctorMappers.CreateFrom(user, request);
                await _doctorRepo.AddAsync(doctor, ct);
                await _uow.CommitAsync(ct);

                _logger.LogInformation("New doctor created. UserId: {UserId}", user.Id);
                return Result.Success("Doctor added successfully.");
                }
            catch (DbUpdateException dbEx)
                {
                await _uow.RollbackAsync(ct);
                _logger.LogError(dbEx, "DB error while creating doctor with phone {Phone}", normalizedPhone);
                return Result.Failure("A database error occurred while creating the doctor.");
                }
            }
        catch (Exception ex)
            {
            _logger.LogError(ex, "Unexpected error while creating doctor with phone {Phone}", request.Phone);
            return Result.Failure("An error occurred. Please try again later.");
            }
        }

    public async Task<Result> CreateStaffAsync(CreateStaffRequest request, string createdBy, CancellationToken ct = default)
        {
        if (request is null)
            return Result.Failure("Invalid request.");

        if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Phone) ||
            string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.Position))
            return Result.Failure("FullName, phone, password and position are required.");

        var normalizedPhone = NormalizePhone(request.Phone);
        if (normalizedPhone == null)
            return Result.Failure("Invalid phone number.");

        try
            {
            if (await _userRepo.GetByPhoneAsync(normalizedPhone, ct) != null)
                return Result.Failure("Phone Number already in use. Try a different one.");

            var user = UserMappers.CreateForRole(request.FullName.Trim(), request.Email?.Trim(), normalizedPhone, role: "Staff", createdBy);
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            user.CreatedBy = createdBy;

            await _uow.BeginTransactionAsync(ct);
            try
                {
                await _userRepo.AddAsync(user, ct);
                var staff = StaffMappers.CreateFrom(user, request);
                await _staffRepo.AddAsync(staff, ct);
                await _uow.CommitAsync(ct);

                _logger.LogInformation("New staff created. UserId: {UserId}", user.Id);
                return Result.Success("Staff created successfully.");
                }
            catch (DbUpdateException dbEx)
                {
                await _uow.RollbackAsync(ct);
                _logger.LogError(dbEx, "DB error while creating staff with phone {Phone}", normalizedPhone);
                return Result.Failure("A database error occurred while creating the staff.");
                }
            }
        catch (Exception ex)
            {
            _logger.LogError(ex, "Unexpected error while creating staff with phone {Phone}", request.Phone);
            return Result.Failure("An error occurred. Please try again later.");
            }
        }

    public async Task<Result> CompletePatientProfileAsync(CompletePatientProfileRequest request, string updatedBy, CancellationToken ct = default)
        {
        if (request is null)
            return Result.Failure("Invalid request.");

        var user = await _userRepo.GetByIdAsync(request.UserId, ct);
        if (user == null)
            return Result.Failure("User not found.");

        try
            {
            var existingPatient = await _patientRepo.GetByUserIdAsync(user.Id, ct);
            if (existingPatient != null)
                return Result.Failure("Profile already completed.");

            var patient = new Patient
                {
                UserId = user.Id,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                BloodGroup = request.BloodGroup,
                EmergencyContactName = request.EmergencyContactName,
                EmergencyContactNumber = request.EmergencyContactNumber,
                KnownAllergies = request.KnownAllergies,
                ExistingConditions = request.ExistingConditions,
                CreatedBy = updatedBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                RegisteredAt = DateTime.UtcNow,
                };

            await _uow.BeginTransactionAsync(ct);
            try
                {
                await _patientRepo.AddAsync(patient, ct);

                // mark user's profile as completed
                user.IsProfileCompleted = true;
                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedBy = updatedBy;
                await _userRepo.UpdateAsync(user, ct);

                await _uow.CommitAsync(ct);
                _logger.LogInformation("Patient profile completed for user {UserId}", user.Id);
                return Result.Success("Profile updated successfully.");
                }
            catch (DbUpdateException dbEx)
                {
                await _uow.RollbackAsync(ct);
                _logger.LogError(dbEx, "DB error while completing profile for user {UserId}", user.Id);
                return Result.Failure("Could not complete profile due to a database error.");
                }
            }
        catch (Exception ex)
            {
            _logger.LogError(ex, "Unexpected error while completing profile for user {UserId}", request.UserId);
            return Result.Failure("An error occurred while saving profile. Please try again later.");
            }
        }

    /// <summary>
    /// Simple phone normalizer: removes spaces, parentheses and dashes. Keeps leading + if present.
    /// </summary>
    private static string NormalizePhone(string phone)
        {
        if (string.IsNullOrWhiteSpace(phone))
            return string.Empty;

        var trimmed = phone.Trim();
        var keepPlus = trimmed.StartsWith("+");
        var digitsOnly = new string(trimmed.Where(c => char.IsDigit(c)).ToArray());
        if (string.IsNullOrEmpty(digitsOnly))
            return string.Empty;

        return keepPlus ? "+" + digitsOnly : digitsOnly;
        }
    }

