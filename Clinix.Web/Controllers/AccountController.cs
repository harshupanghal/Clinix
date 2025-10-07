//using System.Security.Claims;
//using BlazorAuthNoIdentity;
//using Clinix.Domain.Common;
//using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Authentication.Cookies;
//using Microsoft.AspNetCore.Mvc;

//namespace Clinix.Web.Controllers;

//[ApiController]
//[Route("api/[controller]")]
//public class AccountController : ControllerBase
//    {
//    private readonly Application.Interfaces.IAuthenticationService _authService;

//    public AccountController(Application.Interfaces.IAuthenticationService authService)
//        {
//        _authService = authService;
//        }

//    public class LoginRequest
//        {
//        public string UsernameOrEmail { get; set; } = null!;
//        public string Password { get; set; } = null!;
//        public bool RememberMe { get; set; } = false;
//        }

//    [HttpPost("login")]
//    public async Task<IActionResult> Login([FromBody] LoginRequest request)
//        {
//        const string AuthScheme = "clx-auth";
//        var authResult = await _authService.ValidateCredentialsAsync(request.UsernameOrEmail, request.Password);
//        if (!authResult.IsSuccess)
//            return Unauthorized(new { error = authResult.Error ?? "Invalid credentials" });

//        LoggedInUserModel user = new LoggedInUserModel(authResult.UserId, authResult.Username, authResult.Email, authResult.Role);

//        var claims = user.ToClaims();
//        var identity = new ClaimsIdentity(claims, AuthScheme);
//        var principal = new ClaimsPrincipal(identity);

//        var authProperties = new AuthenticationProperties
//            {
//            IsPersistent = request.RememberMe
//            };

//        await HttpContext.SignInAsync(AuthScheme, principal, authProperties);

//       

//        //await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

//        return Ok();
//        }


