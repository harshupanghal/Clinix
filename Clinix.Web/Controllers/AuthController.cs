using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Clinix.Web.Controllers;

[Route("account")]
[AllowAnonymous]
public class ProcessLoginController : Controller
    {
    private readonly ILogger<ProcessLoginController> _logger;
    private readonly ProtectedSessionStorage _sessionStorage;
    private const string AuthScheme = "clx-auth"; // Must match Program.cs

    public ProcessLoginController(
        ILogger<ProcessLoginController> logger,
        ProtectedSessionStorage sessionStorage)
        {
        _logger = logger;
        _sessionStorage = sessionStorage;
        }

    [HttpGet("process-login")]
    public async Task<IActionResult> ProcessLogin()
        {
        try
            {
            // Retrieve pending auth data from session storage with strongly-typed model
            var authDataResult = await _sessionStorage.GetAsync<PendingAuthData>("PendingAuth");

            if (!authDataResult.Success || authDataResult.Value == null)
                {
                _logger.LogWarning("No pending authentication data found");
                return Redirect("/login?error=session-expired");
                }

            var authData = authDataResult.Value;

            // Clear the temporary storage
            await _sessionStorage.DeleteAsync("PendingAuth");

            // Create claims from the stored user data
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, authData.UserId.ToString()),
                new Claim(ClaimTypes.Name, authData.Username),
                new Claim(ClaimTypes.Email, authData.Email),
                new Claim(ClaimTypes.Role, authData.Role)
            };

            var identity = new ClaimsIdentity(claims, AuthScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
                {
                IsPersistent = authData.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(authData.RememberMe ? 72 : 8),
                AllowRefresh = true
                };

            // Perform the actual sign-in with cookie creation
            await HttpContext.SignInAsync(AuthScheme, principal, authProperties);

            _logger.LogInformation("User {Username} authenticated successfully via redirect", authData.Username);

            // Redirect to home or return URL
            var returnUrl = HttpContext.Request.Query["returnUrl"].FirstOrDefault() ?? "/";
            return Redirect(returnUrl);
            }
        catch (Exception ex)
            {
            _logger.LogError(ex, "Error processing login");
            return Redirect("/login?error=processing-failed");
            }
        }

    // Strongly-typed model for pending authentication data
    private sealed class PendingAuthData
        {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
        }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
        {
        await HttpContext.SignOutAsync(AuthScheme);
        _logger.LogInformation("User signed out");
        return Redirect("/login");
        }
    }
