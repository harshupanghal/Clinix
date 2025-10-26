namespace Clinix.Application.Options;

public sealed class NotificationsOptions
    {
    public bool Enabled { get; set; } = false;
    public SmtpOptions Smtp { get; set; } = new();
    public TwilioOptions Twilio { get; set; } = new();

    public sealed class SmtpOptions
        {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 25;
        public bool EnableSsl { get; set; } = false;
        public string? User { get; set; }
        public string? Password { get; set; }
        public string FromEmail { get; set; } = "noreply@example.test";
        public string FromName { get; set; } = "Clinic";
        }

    public sealed class TwilioOptions
        {
        public string? AccountSid { get; set; }
        public string? AuthToken { get; set; }
        public string? FromPhone { get; set; }
        }
    }
