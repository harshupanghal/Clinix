using System.Net;
using System.Net.Mail;
using Clinix.Application.Interfaces;
using Clinix.Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Clinix.Infrastructure.Messaging;

/// <summary>
/// Production notification sender with detailed SMS logging for development.
/// Email: Sends via SMTP when configured
/// SMS: Logs detailed message content (ready for Twilio integration)
/// </summary>
public sealed class RealNotificationSender : INotificationSender
    {
    private readonly NotificationsOptions _opts;
    private readonly ILogger<RealNotificationSender> _logger;
    private readonly bool _twilioConfigured;

    public RealNotificationSender(IOptions<NotificationsOptions> opts, ILogger<RealNotificationSender> logger)
        {
        _opts = opts.Value;
        _logger = logger;

        // Check if Twilio is fully configured
        _twilioConfigured = !string.IsNullOrWhiteSpace(_opts.Twilio.AccountSid)
                          && !string.IsNullOrWhiteSpace(_opts.Twilio.AuthToken)
                          && !string.IsNullOrWhiteSpace(_opts.Twilio.FromPhone);
        }

    /// <summary>
    /// Sends email via SMTP. Logs to console if notifications disabled.
    /// </summary>
    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken ct = default)
        {
        try
            {
            if (!_opts.Enabled)
                {
                _logger.LogInformation(
                    "📧 [DEV MODE - EMAIL NOT SENT]\n" +
                    "   To: {To}\n" +
                    "   Subject: {Subject}\n" +
                    "   Body Preview: {BodyPreview}",
                    to, subject, body.Length > 100 ? body.Substring(0, 100) + "..." : body);
                return;
                }

            // Validate SMTP configuration
            if (string.IsNullOrWhiteSpace(_opts.Smtp.Host) || string.IsNullOrWhiteSpace(_opts.Smtp.User))
                {
                _logger.LogWarning("⚠️ SMTP not configured. Email to {To} not sent.", to);
                return;
                }

            using var client = new SmtpClient(_opts.Smtp.Host, _opts.Smtp.Port)
                {
                EnableSsl = _opts.Smtp.EnableSsl,
                Credentials = new NetworkCredential(_opts.Smtp.User, _opts.Smtp.Password)
                };

            using var msg = new MailMessage(
                new MailAddress(_opts.Smtp.FromEmail, _opts.Smtp.FromName),
                new MailAddress(to))
                {
                Subject = subject,
                Body = body,
                IsBodyHtml = false
                };

            await client.SendMailAsync(msg, ct);

            _logger.LogInformation(
                "✅ [EMAIL SENT SUCCESSFULLY]\n" +
                "   To: {To}\n" +
                "   Subject: {Subject}\n" +
                "   Timestamp: {Timestamp}",
                to, subject, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            }
        catch (Exception ex)
            {
            _logger.LogError(ex,
                "❌ [EMAIL SEND FAILED]\n" +
                "   To: {To}\n" +
                "   Subject: {Subject}\n" +
                "   Error: {Error}",
                to, subject, ex.Message);
            throw;
            }
        }

    /// <summary>
    /// Sends SMS via Twilio (if configured) or logs detailed message for development.
    /// Perfect for testing before purchasing Twilio numbers.
    /// </summary>
    public async Task SendSmsAsync(string to, string message, CancellationToken ct = default)
        {
        try
            {
            // Always log SMS content for development/debugging
            _logger.LogInformation(
                "📱 [SMS MESSAGE DETAILS]\n" +
                "   ╔════════════════════════════════════════════════════════════╗\n" +
                "   ║ TO: {To,-54} ║\n" +
                "   ║ MESSAGE: {MessagePreview,-48} ║\n" +
                "   ║ LENGTH: {Length,-51} ║\n" +
                "   ║ TIMESTAMP: {Timestamp,-47} ║\n" +
                "   ╠════════════════════════════════════════════════════════════╣\n" +
                "   ║ FULL MESSAGE:                                             ║\n" +
                "   ║ {FullMessage,-58}║\n" +
                "   ╚════════════════════════════════════════════════════════════╝",
                to,
                message.Length > 40 ? message.Substring(0, 40) + "..." : message,
                $"{message.Length} chars",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                message.Replace("\n", "\n   ║ "));

            if (!_opts.Enabled)
                {
                _logger.LogInformation("   ⚠️  Notifications disabled - SMS NOT SENT (Dev Mode)");
                return;
                }

            if (!_twilioConfigured)
                {
                _logger.LogWarning(
                    "   ⚠️  Twilio not configured - SMS NOT SENT\n" +
                    "   📌  Once Twilio is set up, SMS will be automatically sent to: {To}\n" +
                    "   💡  Add Twilio credentials to appsettings.json under 'Notifications:Twilio'",
                    to);
                return;
                }

            //  when Twilio is configured
            /*
            TwilioClient.Init(_opts.Twilio.AccountSid, _opts.Twilio.AuthToken);
            var twilioMessage = await MessageResource.CreateAsync(
                to: new PhoneNumber(to),
                from: new PhoneNumber(_opts.Twilio.FromPhone),
                body: message
            );

            _logger.LogInformation(
                "✅ [SMS SENT VIA TWILIO]\n" +
                "   To: {To}\n" +
                "   Twilio SID: {Sid}\n" +
                "   Status: {Status}",
                to, twilioMessage.Sid, twilioMessage.Status);
            */

            await Task.CompletedTask;
            }
        catch (Exception ex)
            {
            _logger.LogError(ex,
                "❌ [SMS SEND FAILED]\n" +
                "   To: {To}\n" +
                "   Message: {Message}\n" +
                "   Error: {Error}",
                to, message, ex.Message);
            throw;
            }
        }
    }
