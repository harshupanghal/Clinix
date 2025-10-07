namespace Clinix.Application.Interfaces.ServiceInterfaces;

public interface IEmailSender
    {
    Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken ct);
    }
