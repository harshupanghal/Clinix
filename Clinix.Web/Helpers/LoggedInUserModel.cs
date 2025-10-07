using System.Security.Claims;

namespace BlazorAuthNoIdentity;

public record LoggedInUserModel(long? Id, string Username, string Email, string Role)
    {
    public Claim[] ToClaims() =>
        [
            new Claim(ClaimTypes.NameIdentifier, Id.ToString() ?? string.Empty),
            new Claim(ClaimTypes.Name, Username ?? string.Empty),
            new Claim(ClaimTypes.Email, Email ?? string.Empty),
            new Claim(ClaimTypes.Role, Role ?? string.Empty)
        ];
    }