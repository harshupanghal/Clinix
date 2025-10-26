using Clinix.Application.Dtos.UserManagement;
using Clinix.Application.Interfaces;
using Clinix.Application.Interfaces.Functionalities;
using Clinix.Application.Interfaces.UserRepo;
using Clinix.Domain.Common;
using Clinix.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Clinix.Application.Services;

public class UserManagementService : IUserManagementService
    {
    private readonly IUserRepository _userRepo;
    private readonly IDoctorRepository _doctorRepo;
    private readonly IPatientRepository _patientRepo;
    private readonly IStaffRepository _staffRepo;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<UserManagementService> _logger;

    public UserManagementService(
        IUserRepository userRepo,
        IDoctorRepository doctorRepo,
        IPatientRepository patientRepo,
        IStaffRepository staffRepo,
        IUnitOfWork uow,
        ILogger<UserManagementService> logger)
        {
        _userRepo = userRepo;
        _doctorRepo = doctorRepo;
        _patientRepo = patientRepo;
        _staffRepo = staffRepo;
        _uow = uow;
        _logger = logger;
        }

    public async Task<UserStatsDto> GetUserStatsAsync(CancellationToken ct = default)
        {
        try
            {
            var allUsers = await _userRepo.GetAllAsync(ct);

            return new UserStatsDto(
                TotalUsers: allUsers.Count,
                TotalAdmins: allUsers.Count(u => u.Role == "Admin"),
                TotalDoctors: allUsers.Count(u => u.Role == "Doctor"),
                TotalPatients: allUsers.Count(u => u.Role == "Patient"),
                TotalStaff: allUsers.Count(u => u.Role == "Staff"),
                ActiveUsers: allUsers.Count(u => !u.IsDeleted),
                ProfileCompletedCount: allUsers.Count(u => u.IsProfileCompleted)
            );
            }
        catch (Exception ex)
            {
            _logger.LogError(ex, "Error getting user stats");
            return new UserStatsDto(0, 0, 0, 0, 0, 0, 0);
            }
        }

    public async Task<List<UserListDto>> GetAllUsersAsync(CancellationToken ct = default)
        {
        try
            {
            var users = await _userRepo.GetAllAsync(ct);
            var doctors = await _doctorRepo.GetAllAsync(ct);
            var patients = await _patientRepo.GetAllPatientsAsync(ct);

            var userDtos = new List<UserListDto>();

            foreach (var user in users)
                {
                var doctor = doctors.FirstOrDefault(d => d.UserId == user.Id);
                var patient = patients.FirstOrDefault(p => p.UserId == user.Id);

                userDtos.Add(new UserListDto(
                    user.Id,
                    user.FullName,
                    user.Email,
                    user.Phone,
                    user.Role,
                    user.IsProfileCompleted,
                    user.CreatedAt,
                    user.CreatedBy,
                    Specialty: doctor?.Specialty,
                    Department: null, // Add when you query Staff
                    Position: null,   // Add when you query Staff
                    BloodGroup: patient?.BloodGroup,
                    IsActive: doctor?.IsActive ?? patient?.IsActive
                ));
                }

            return userDtos.OrderByDescending(u => u.CreatedAt).ToList();
            }
        catch (Exception ex)
            {
            _logger.LogError(ex, "Error getting all users");
            return new List<UserListDto>();
            }
        }

    public async Task<List<UserListDto>> GetUsersByRoleAsync(string role, CancellationToken ct = default)
        {
        var allUsers = await GetAllUsersAsync(ct);
        return allUsers.Where(u => u.Role == role).ToList();
        }

    public async Task<UserDetailDto?> GetUserDetailAsync(long userId, CancellationToken ct = default)
        {
        try
            {
            var user = await _userRepo.GetByIdAsync(userId, ct);
            if (user == null) return null;

            DoctorDetailDto? doctorInfo = null;
            PatientDetailDto? patientInfo = null;
            StaffDetailDto? staffInfo = null;

            if (user.Role == "Doctor")
                {
                var doctor = await _doctorRepo.GetByUserIdAsync(userId, ct);
                if (doctor != null)
                    {
                    doctorInfo = new DoctorDetailDto(
                        doctor.Specialty,
                        doctor.Degree,
                        doctor.LicenseNumber,
                        doctor.ExperienceYears,
                        doctor.ConsultationFee,
                        doctor.IsOnDuty
                    );
                    }
                }
            else if (user.Role == "Patient")
                {
                var patient = await _patientRepo.GetByUserIdAsync(userId, ct);
                if (patient != null)
                    {
                    patientInfo = new PatientDetailDto(
                        patient.MedicalRecordNumber,
                        patient.BloodGroup,
                        patient.Gender,
                        patient.DateOfBirth
                    );
                    }
                }

            return new UserDetailDto(
                user.Id,
                user.FullName,
                user.Email,
                user.Phone,
                user.Role,
                user.IsProfileCompleted,
                user.IsDeleted,
                user.CreatedAt,
                user.UpdatedAt,
                user.CreatedBy,
                user.UpdatedBy,
                doctorInfo,
                patientInfo,
                staffInfo
            );
            }
        catch (Exception ex)
            {
            _logger.LogError(ex, "Error getting user detail for userId {UserId}", userId);
            return null;
            }
        }

    public async Task<Result> UpdateUserAsync(UpdateUserRequest request, string updatedBy, CancellationToken ct = default)
        {
        if (request == null)
            return Result.Failure("Invalid request.");

        try
            {
            var user = await _userRepo.GetByIdAsync(request.UserId, ct);
            if (user == null)
                return Result.Failure("User not found.");

            user.FullName = request.FullName.Trim();
            user.Email = request.Email.Trim();
            user.Phone = request.Phone.Trim();
            user.UpdatedBy = updatedBy;

            await _uow.BeginTransactionAsync(ct);
            try
                {
                await _userRepo.UpdateAsync(user, ct);
                await _uow.CommitAsync(ct);

                _logger.LogInformation("User {UserId} updated by {UpdatedBy}", user.Id, updatedBy);
                return Result.Success("User updated successfully.");
                }
            catch
                {
                await _uow.RollbackAsync(ct);
                throw;
                }
            }
        catch (Exception ex)
            {
            _logger.LogError(ex, "Error updating user {UserId}", request.UserId);
            return Result.Failure("An error occurred while updating user.");
            }
        }

    public async Task<Result> DeleteUserAsync(long userId, string deletedBy, CancellationToken ct = default)
        {
        try
            {
            var user = await _userRepo.GetByIdAsync(userId, ct);
            if (user == null)
                return Result.Failure("User not found.");

            if (user.Role == "Admin")
                return Result.Failure("Cannot delete admin users.");

            await _uow.BeginTransactionAsync(ct);
            try
                {
                await _userRepo.DeleteAsync(userId, ct);
                await _uow.CommitAsync(ct);

                _logger.LogInformation("User {UserId} soft-deleted by {DeletedBy}", userId, deletedBy);
                return Result.Success("User deleted successfully.");
                }
            catch
                {
                await _uow.RollbackAsync(ct);
                throw;
                }
            }
        catch (Exception ex)
            {
            _logger.LogError(ex, "Error deleting user {UserId}", userId);
            return Result.Failure("An error occurred while deleting user.");
            }
        }

    public async Task<Result> ReactivateUserAsync(long userId, string reactivatedBy, CancellationToken ct = default)
        {
        try
            {
            var user = await _userRepo.GetByIdAsync(userId, ct);
            if (user == null)
                return Result.Failure("User not found.");

            user.IsDeleted = false;
            user.UpdatedBy = reactivatedBy;

            await _uow.BeginTransactionAsync(ct);
            try
                {
                await _userRepo.UpdateAsync(user, ct);
                await _uow.CommitAsync(ct);

                _logger.LogInformation("User {UserId} reactivated by {ReactivatedBy}", userId, reactivatedBy);
                return Result.Success("User reactivated successfully.");
                }
            catch
                {
                await _uow.RollbackAsync(ct);
                throw;
                }
            }
        catch (Exception ex)
            {
            _logger.LogError(ex, "Error reactivating user {UserId}", userId);
            return Result.Failure("An error occurred while reactivating user.");
            }
        }
    }
