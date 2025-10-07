using Clinix.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Clinix.Infrastructure.Email;

public class SendGridEmailSender : IEmailSender
    {
    private readonly IConfiguration _config;
    private readonly ILogger<SendGridEmailSender> _log;
    public SendGridEmailSender(IConfiguration config, ILogger<SendGridEmailSender> log)
        {
        _config = config; _log = log;
        }

    public Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken ct)
        {
        // TODO: implement actual SendGrid or SMTP client
        // Example: use SendGrid nuget, pick API key from config.
        _log.LogInformation("Stub SendEmailAsync called for {To} subject {Subject}", toEmail, subject);
        return Task.CompletedTask;
        }
    }
