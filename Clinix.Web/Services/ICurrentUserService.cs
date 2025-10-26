
namespace Clinix.Web.Services;

public interface ICurrentUserService
    {
    Task<CurrentUserInfo> GetCurrentUserAsync();
    }

public record CurrentUserInfo(
    bool IsAuthenticated,
    string UserId,
    string UserName,
    string Role,
    string? ProviderId = null,
    string? PatientId = null
);
