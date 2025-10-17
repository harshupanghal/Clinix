using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;


namespace Clinix.Infrastructure.Services;

public interface IEmailService
    {
    Task SendEmailAsync(string toEmail, string subject, string body);
    }


public class SmtpEmailService : IEmailService
    {
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpPass;


    public SmtpEmailService(string smtpHost, int smtpPort, string smtpUser, string smtpPass)
        {
        _smtpHost = smtpHost;
        _smtpPort = smtpPort;
        _smtpUser = smtpUser;
        _smtpPass = smtpPass;
        }


    public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
        using var client = new SmtpClient(_smtpHost, _smtpPort)
            {
            Credentials = new NetworkCredential(_smtpUser, _smtpPass),
            EnableSsl = true
            };


        var mail = new MailMessage(_smtpUser, toEmail, subject, body);
        mail.IsBodyHtml = true;
        await client.SendMailAsync(mail);
        }
    }
