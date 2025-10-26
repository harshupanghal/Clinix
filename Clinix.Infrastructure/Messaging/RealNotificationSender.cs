using System.Net;
using System.Net.Mail;

using Clinix.Application.Interfaces;
using Clinix.Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Clinix.Infrastructure.Messaging;

public sealed class RealNotificationSender : INotificationSender
    {
    private readonly NotificationsOptions _opts;
    private readonly ILogger<RealNotificationSender> _logger;
    private readonly bool _twilioReady;

    public RealNotificationSender(IOptions<NotificationsOptions> opts, ILogger<RealNotificationSender> logger)
        {
        _opts = opts.Value; _logger = logger;
        if (!string.IsNullOrWhiteSpace(_opts.Twilio.AccountSid) && !string.IsNullOrWhiteSpace(_opts.Twilio.AuthToken))
            { TwilioClient.Init(_opts.Twilio.AccountSid, _opts.Twilio.AuthToken); _twilioReady = true; }
        }

    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken ct = default)
        {
        if (!_opts.Enabled) { _logger.LogInformation("DEV Email -> {To} | {Subject}", to, subject); return; }
        using var client = new SmtpClient(_opts.Smtp.Host, _opts.Smtp.Port) { EnableSsl = _opts.Smtp.EnableSsl };
        if (!string.IsNullOrWhiteSpace(_opts.Smtp.User))
            client.Credentials = new NetworkCredential(_opts.Smtp.User, _opts.Smtp.Password);
        using var msg = new MailMessage(new MailAddress(_opts.Smtp.FromEmail, _opts.Smtp.FromName), new MailAddress(to))
            { Subject = subject, Body = body, IsBodyHtml = false };
        await client.SendMailAsync(msg, ct);
        }

    public async Task SendSmsAsync(string to, string message, CancellationToken ct = default)
        {
        if (!_opts.Enabled) { _logger.LogInformation("DEV SMS -> {To}", to); return; }
        if (!_twilioReady || string.IsNullOrWhiteSpace(_opts.Twilio.FromPhone))
            { _logger.LogWarning("Twilio not configured"); return; }
        await MessageResource.CreateAsync(to: new PhoneNumber(to), from: new PhoneNumber(_opts.Twilio.FromPhone), body: message);
        }
    }
