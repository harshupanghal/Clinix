﻿namespace Clinix.Application.Interfaces;

public interface IEmailSender
    {
    Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken ct);
    }
