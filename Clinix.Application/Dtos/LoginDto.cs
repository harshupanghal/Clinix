namespace Clinix.Application.Dtos;
public class LoginDto
    {
    public string UserNameOrEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; } = false;
    }
