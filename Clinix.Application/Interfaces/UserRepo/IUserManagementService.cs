using Clinix.Application.Dtos.UserManagement;
using Clinix.Domain.Common;

namespace Clinix.Application.Interfaces.UserRepo;

public interface IUserManagementService
    {
    Task<UserStatsDto> GetUserStatsAsync(CancellationToken ct = default);
    Task<List<UserListDto>> GetAllUsersAsync(CancellationToken ct = default);
    Task<List<UserListDto>> GetUsersByRoleAsync(string role, CancellationToken ct = default);
    Task<UserDetailDto?> GetUserDetailAsync(long userId, CancellationToken ct = default);
    Task<Result> UpdateUserAsync(UpdateUserRequest request, string updatedBy, CancellationToken ct = default);
    Task<Result> DeleteUserAsync(long userId, string deletedBy, CancellationToken ct = default);
    Task<Result> ReactivateUserAsync(long userId, string reactivatedBy, CancellationToken ct = default);
    }
