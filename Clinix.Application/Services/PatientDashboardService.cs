using Clinix.Application.Dtos;
using Clinix.Application.Interfaces.Functionalities;
using Clinix.Application.Interfaces.UserRepo;
using Clinix.Domain.Common;
using Clinix.Domain.Entities.ApplicationUsers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Clinix.Application.Services
    {
    /// <summary>
    /// Provides read & update operations for patient dashboard.
    /// </summary>
    public class PatientDashboardService : IPatientDashboardService
        {
        private readonly IUserRepository _userRepo;
        private readonly IPatientRepository _patientRepo;
        private readonly IUnitOfWork _uow;
        private readonly ILogger<PatientDashboardService> _logger;

        public PatientDashboardService(
            IUserRepository userRepo,
            IPatientRepository patientRepo,
            IUnitOfWork uow,
            ILogger<PatientDashboardService> logger)
            {
            _userRepo = userRepo;
            _patientRepo = patientRepo;
            _uow = uow;
            _logger = logger;
            }

        /// <summary>
        /// Retrieves dashboard DTO for the patient. Returns null if user not found.
        /// </summary>
        public async Task<PatientDashboardDto?> GetDashboardAsync(long userId, CancellationToken ct = default)
            {
            var user = await _userRepo.GetByIdAsync(userId, ct);
            if (user == null) return null;

            var patient = await _patientRepo.GetByUserIdAsync(userId, ct);

            var dto = new PatientDashboardDto
                {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                IsProfileCompleted = user.IsProfileCompleted,
                DateOfBirth = patient?.DateOfBirth,
                Gender = patient?.Gender,
                BloodGroup = patient?.BloodGroup,
                EmergencyContactName = patient?.EmergencyContactName,
                EmergencyContactNumber = patient?.EmergencyContactNumber,
                KnownAllergies = patient?.KnownAllergies,
                ExistingConditions = patient?.ExistingConditions,
                RegisteredAt = patient?.RegisteredAt ?? user.CreatedAt,
                CreatedAt = patient?.CreatedAt ?? user.CreatedAt,
                UpdatedAt = patient?.UpdatedAt ?? user.UpdatedAt
                };

            // Appointments and FollowUps intentionally left out for now (commented)
            // dto.UpcomingAppointments = ...;
            // dto.FollowUps = ...;

            return dto;
            }

        /// <summary>
        /// Updates or creates the patient's profile (medical + optional name/email).
        /// </summary>
        public async Task<Result> UpdateProfileAsync(PatientUpdateProfileRequest request, string updatedBy, CancellationToken ct = default)
            {
            if (request == null)
                return Result.Failure("Invalid request.");

            // basic retrieval
            var user = await _userRepo.GetByIdAsync(request.UserId, ct);
            if (user == null)
                return Result.Failure("User not found.");

            try
                {
                // If email change requested, ensure it is not used by another account
                if (!string.IsNullOrWhiteSpace(request.Email) && !string.Equals(request.Email.Trim(), user.Email?.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                    var existing = await _userRepo.GetByEmailAsync(request.Email.Trim(), ct);
                    if (existing != null && existing.Id != user.Id)
                        return Result.Failure("Email is already in use by another account.");
                    }

                var existingPatient = await _patientRepo.GetByUserIdAsync(user.Id, ct);

                // map user updates (FullName, Email)
                if (!string.IsNullOrWhiteSpace(request.FullName))
                    user.FullName = request.FullName.Trim();

                if (!string.IsNullOrWhiteSpace(request.Email))
                    user.Email = request.Email.Trim();

                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedBy = updatedBy;

                await _uow.BeginTransactionAsync(ct);
                try
                    {
                    // update or create patient record
                    if (existingPatient == null)
                        {
                        var newPatient = new Patient
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
                            RegisteredAt = DateTime.UtcNow
                            };

                        await _patientRepo.AddAsync(newPatient, ct);
                        // update profile completed flag based on new patient
                        user.IsProfileCompleted = newPatient.IsProfileComplete();
                        }
                    else
                        {
                        // update tracked patient fields
                        existingPatient.DateOfBirth = request.DateOfBirth ?? existingPatient.DateOfBirth;
                        existingPatient.Gender = request.Gender ?? existingPatient.Gender;
                        existingPatient.BloodGroup = request.BloodGroup ?? existingPatient.BloodGroup;
                        existingPatient.EmergencyContactName = request.EmergencyContactName ?? existingPatient.EmergencyContactName;
                        existingPatient.EmergencyContactNumber = request.EmergencyContactNumber ?? existingPatient.EmergencyContactNumber;
                        existingPatient.KnownAllergies = request.KnownAllergies ?? existingPatient.KnownAllergies;
                        existingPatient.ExistingConditions = request.ExistingConditions ?? existingPatient.ExistingConditions;
                        existingPatient.UpdatedAt = DateTime.UtcNow;
                        existingPatient.UpdatedBy = updatedBy;

                        await _patientRepo.UpdateAsync(existingPatient, ct);

                        // set profile completion based on updated data
                        user.IsProfileCompleted = existingPatient.IsProfileComplete();
                        }

                    // persist user update
                    await _userRepo.UpdateAsync(user, ct);

                    await _uow.CommitAsync(ct);
                    _logger.LogInformation("Patient profile updated for user {UserId} by {Actor}", user.Id, updatedBy);
                    return Result.Success("Profile updated successfully.");
                    }
                catch (DbUpdateException dbEx)
                    {
                    await _uow.RollbackAsync(ct);
                    _logger.LogError(dbEx, "Database error while updating profile for user {UserId}", user.Id);
                    return Result.Failure("Could not update profile due to a database error.");
                    }
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Unexpected error while updating profile for user {UserId}", request.UserId);
                return Result.Failure("An unexpected error occurred. Please try again later.");
                }
            }
        }
    }
