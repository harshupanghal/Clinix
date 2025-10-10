namespace Clinix.Application.Dtos;
public class LoginModel
    {
    public string Phone { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; } = false;
    }
