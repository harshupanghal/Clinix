using Clinix.Domain.Enums;

namespace Clinix.Infrastructure.Notifications;

/// <summary>
/// Production-grade HTML email templates with professional design and responsive layout.
/// All templates follow consistent branding with color-coded status indicators.
/// </summary>
public static class NotificationTemplates
    {
    // Professional color scheme
    private const string PrimaryColor = "#2563eb";      // Professional blue
    private const string SuccessColor = "#059669";      // Green for confirmations
    private const string WarningColor = "#dc2626";      // Red for cancellations  
    private const string InfoColor = "#0891b2";         // Teal for updates
    private const string AlertColor = "#f59e0b";        // Amber for reminders
    private const string TextColor = "#1f2937";
    private const string LightTextColor = "#6b7280";
    private const string BackgroundColor = "#f9fafb";
    private const string BorderColor = "#e5e7eb";

    // ========================================
    // BASE EMAIL TEMPLATE
    // ========================================

    private static string WrapEmailTemplate(string headerColor, string badge, string title, string content)
        {
        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
    <title>{title}</title>
    <style type=""text/css"">
        body {{ margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background-color: {BackgroundColor}; }}
        table {{ border-collapse: collapse; }}
        @media only screen and (max-width: 600px) {{
            .container {{ width: 100% !important; }}
            .content {{ padding: 20px !important; }}
        }}
    </style>
</head>
<body style=""margin: 0; padding: 0; background-color: {BackgroundColor};"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: {BackgroundColor};"">
        <tr>
            <td align=""center"" style=""padding: 40px 20px;"">
                <table class=""container"" width=""600"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.05);"">
                    
                    <!-- Header with Status Badge -->
                    <tr>
                        <td align=""center"" style=""padding: 40px 40px 30px 40px; background: linear-gradient(135deg, {headerColor} 0%, {headerColor}dd 100%); border-radius: 8px 8px 0 0;"">
                            <table cellpadding=""0"" cellspacing=""0"" border=""0"">
                                <tr>
                                    <td style=""background-color: rgba(255,255,255,0.2); padding: 8px 16px; border-radius: 20px;"">
                                        <span style=""color: #ffffff; font-size: 13px; font-weight: 600; letter-spacing: 0.5px;"">{badge}</span>
                                    </td>
                                </tr>
                            </table>
                            <h1 style=""margin: 20px 0 0 0; color: #ffffff; font-size: 28px; font-weight: 600; line-height: 1.3;"">{title}</h1>
                        </td>
                    </tr>

                    <!-- Main Content -->
                    <tr>
                        <td class=""content"" style=""padding: 40px;"">
                            {content}
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style=""padding: 30px 40px; background-color: {BackgroundColor}; border-radius: 0 0 8px 8px; border-top: 1px solid {BorderColor};"">
                            <p style=""margin: 0 0 8px 0; color: {TextColor}; font-size: 15px; font-weight: 600;"">Clinix Hospital Management System</p>
                            <p style=""margin: 0; color: {LightTextColor}; font-size: 13px; line-height: 1.6;"">
                                This is an automated notification. For assistance, contact us at 
                                <a href=""mailto:support@clinixhms.com"" style=""color: {PrimaryColor}; text-decoration: none;"">support@clinixhms.com</a>
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        }

    // ========================================
    // APPOINTMENT SCHEDULED
    // ========================================

    public static (string Subject, string HtmlBody) AppointmentScheduled_Patient(
        string patientName, string doctorName, DateTimeOffset start, DateTimeOffset end)
        {
        var subject = "Appointment Confirmation – Clinix Hospital";

        var content = $@"
            <p style=""margin: 0 0 30px 0; color: {TextColor}; font-size: 16px; line-height: 1.6;"">Dear <strong>{patientName}</strong>,</p>
            
            <p style=""margin: 0 0 30px 0; color: {TextColor}; font-size: 16px; line-height: 1.6;"">Your appointment has been successfully scheduled.</p>

            <!-- Appointment Details Card -->
            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: {BackgroundColor}; border-radius: 6px; border: 1px solid {BorderColor};"">
                <tr>
                    <td style=""padding: 24px;"">
                        <h2 style=""margin: 0 0 20px 0; color: {TextColor}; font-size: 14px; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;"">Appointment Details</h2>
                        
                        <!-- Date & Time -->
                        <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""margin-bottom: 16px;"">
                            <tr>
                                <td width=""80"" valign=""top"">
                                    <div style=""width: 48px; height: 48px; background-color: {PrimaryColor}; border-radius: 6px; text-align: center; line-height: 48px;"">
                                        <span style=""color: #ffffff; font-size: 20px; font-weight: 700;"">{start:dd}</span>
                                    </div>
                                </td>
                                <td valign=""top"">
                                    <p style=""margin: 0; color: {LightTextColor}; font-size: 13px; font-weight: 500;"">Date & Time</p>
                                    <p style=""margin: 4px 0 0 0; color: {TextColor}; font-size: 16px; font-weight: 600;"">{start:dddd, MMMM dd, yyyy}</p>
                                    <p style=""margin: 2px 0 0 0; color: {TextColor}; font-size: 15px;"">{start:hh:mm tt} – {end:hh:mm tt}</p>
                                </td>
                            </tr>
                        </table>

                        <!-- Doctor -->
                        <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                            <tr>
                                <td width=""80"" valign=""top"">
                                    <div style=""width: 48px; height: 48px; background-color: #e0f2fe; border-radius: 24px; border: 2px solid {PrimaryColor}; display: flex; align-items: center; justify-content: center; text-align: center; line-height: 44px;"">
                                        <span style=""color: {PrimaryColor}; font-size: 18px; font-weight: 700;"">Dr</span>
                                    </div>
                                </td>
                                <td valign=""top"">
                                    <p style=""margin: 0; color: {LightTextColor}; font-size: 13px; font-weight: 500;"">Healthcare Provider</p>
                                    <p style=""margin: 4px 0 0 0; color: {TextColor}; font-size: 16px; font-weight: 600;"">Dr. {doctorName}</p>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>

            <!-- Important Information -->
            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""margin: 30px 0;"">
                <tr>
                    <td style=""padding: 20px; background-color: #fef3c7; border-left: 4px solid {AlertColor}; border-radius: 4px;"">
                        <p style=""margin: 0 0 12px 0; color: {TextColor}; font-size: 14px; font-weight: 600;"">⚠️ Before Your Visit</p>
                        <ul style=""margin: 0; padding-left: 20px; color: {TextColor}; font-size: 14px; line-height: 1.8;"">
                            <li style=""margin-bottom: 6px;"">Please arrive <strong>15 minutes early</strong> for check-in</li>
                            <li style=""margin-bottom: 6px;"">Bring a valid photo ID and insurance card</li>
                            <li>Bring any relevant medical records or test results</li>
                        </ul>
                    </td>
                </tr>
            </table>

            <!-- CTA Button -->
            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""margin: 30px 0;"">
                <tr>
                    <td align=""center"">
                        <table cellpadding=""0"" cellspacing=""0"" border=""0"">
                            <tr>
                                <td align=""center"" style=""border-radius: 6px; background-color: {PrimaryColor};"">
                                    <a href=""https://www.clinixhms.com/appointments"" target=""_blank"" style=""display: inline-block; padding: 14px 32px; color: #ffffff; font-size: 15px; font-weight: 600; text-decoration: none; border-radius: 6px;"">Manage Appointment</a>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>

            <p style=""margin: 20px 0 0 0; color: {LightTextColor}; font-size: 14px; line-height: 1.6; text-align: center;"">
                Need help? Call us at <strong style=""color: {TextColor};"">1-800-CLINIX-CARE</strong>
            </p>";

        return (subject, WrapEmailTemplate(SuccessColor, "✓ CONFIRMED", "Appointment Confirmed", content));
        }

    public static (string Subject, string HtmlBody) AppointmentScheduled_Doctor(
        string doctorName, string patientName, DateTimeOffset start, DateTimeOffset end, AppointmentType type)
        {
        var subject = $"New Appointment: {patientName} – {start:MMM dd, hh:mm tt}";

        var content = $@"
            <p style=""margin: 0 0 30px 0; color: {TextColor}; font-size: 16px; line-height: 1.6;"">Dear <strong>Dr. {doctorName}</strong>,</p>
            
            <p style=""margin: 0 0 30px 0; color: {TextColor}; font-size: 16px; line-height: 1.6;"">A new appointment has been scheduled with you.</p>

            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: {BackgroundColor}; border-radius: 6px; border: 1px solid {BorderColor};"">
                <tr>
                    <td style=""padding: 24px;"">
                        <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                            <tr>
                                <td style=""padding-bottom: 16px; border-bottom: 1px solid {BorderColor};"">
                                    <p style=""margin: 0; color: {LightTextColor}; font-size: 13px;"">Patient Name</p>
                                    <p style=""margin: 4px 0 0 0; color: {TextColor}; font-size: 17px; font-weight: 600;"">{patientName}</p>
                                </td>
                            </tr>
                            <tr>
                                <td style=""padding: 16px 0; border-bottom: 1px solid {BorderColor};"">
                                    <p style=""margin: 0; color: {LightTextColor}; font-size: 13px;"">Date & Time</p>
                                    <p style=""margin: 4px 0 0 0; color: {TextColor}; font-size: 16px; font-weight: 600;"">{start:dddd, MMMM dd, yyyy}</p>
                                    <p style=""margin: 2px 0 0 0; color: {TextColor}; font-size: 15px;"">{start:hh:mm tt} – {end:hh:mm tt}</p>
                                </td>
                            </tr>
                            <tr>
                                <td style=""padding-top: 16px;"">
                                    <p style=""margin: 0; color: {LightTextColor}; font-size: 13px;"">Appointment Type</p>
                                    <p style=""margin: 4px 0 0 0; color: {TextColor}; font-size: 16px; font-weight: 600;"">{type}</p>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>

            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""margin: 30px 0;"">
                <tr>
                    <td align=""center"">
                        <table cellpadding=""0"" cellspacing=""0"" border=""0"">
                            <tr>
                                <td align=""center"" style=""border-radius: 6px; background-color: {PrimaryColor};"">
                                    <a href=""https://www.clinixhms.com/doctor/schedule"" target=""_blank"" style=""display: inline-block; padding: 14px 32px; color: #ffffff; font-size: 15px; font-weight: 600; text-decoration: none; border-radius: 6px;"">View Dashboard</a>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>";

        return (subject, WrapEmailTemplate(InfoColor, "📅 NEW", "New Appointment Scheduled", content));
        }

    // ========================================
    // APPOINTMENT RESCHEDULED
    // ========================================

    public static (string Subject, string HtmlBody) AppointmentRescheduled_Patient(
        string patientName, string doctorName, DateTimeOffset oldStart, DateTimeOffset newStart, DateTimeOffset newEnd)
        {
        var subject = "Appointment Rescheduled – Clinix Hospital";

        var content = $@"
            <p style=""margin: 0 0 30px 0; color: {TextColor}; font-size: 16px; line-height: 1.6;"">Dear <strong>{patientName}</strong>,</p>
            
            <p style=""margin: 0 0 30px 0; color: {TextColor}; font-size: 16px; line-height: 1.6;"">Your appointment has been rescheduled.</p>

            <!-- Comparison Table -->
            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""margin-bottom: 20px;"">
                <tr>
                    <td width=""48%"" valign=""top"">
                        <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #fee; border-radius: 6px; border: 1px solid #fcc;"">
                            <tr>
                                <td style=""padding: 16px;"">
                                    <p style=""margin: 0 0 8px 0; color: {WarningColor}; font-size: 12px; font-weight: 600; text-transform: uppercase;"">Previous</p>
                                    <p style=""margin: 0; color: {TextColor}; font-size: 15px; text-decoration: line-through;"">{oldStart:MMM dd, yyyy}</p>
                                    <p style=""margin: 2px 0 0 0; color: {TextColor}; font-size: 14px; text-decoration: line-through;"">{oldStart:hh:mm tt}</p>
                                </td>
                            </tr>
                        </table>
                    </td>
                    <td width=""4%"" align=""center"" valign=""middle"">
                        <span style=""color: {TextColor}; font-size: 20px;"">→</span>
                    </td>
                    <td width=""48%"" valign=""top"">
                        <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #d1fae5; border-radius: 6px; border: 1px solid #6ee7b7;"">
                            <tr>
                                <td style=""padding: 16px;"">
                                    <p style=""margin: 0 0 8px 0; color: {SuccessColor}; font-size: 12px; font-weight: 600; text-transform: uppercase;"">New</p>
                                    <p style=""margin: 0; color: {TextColor}; font-size: 15px; font-weight: 600;"">{newStart:MMM dd, yyyy}</p>
                                    <p style=""margin: 2px 0 0 0; color: {TextColor}; font-size: 14px; font-weight: 600;"">{newStart:hh:mm tt} – {newEnd:hh:mm tt}</p>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>

            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: {BackgroundColor}; border-radius: 6px; border: 1px solid {BorderColor}; margin-bottom: 30px;"">
                <tr>
                    <td style=""padding: 20px;"">
                        <p style=""margin: 0; color: {LightTextColor}; font-size: 13px;"">Healthcare Provider</p>
                        <p style=""margin: 4px 0 0 0; color: {TextColor}; font-size: 16px; font-weight: 600;"">Dr. {doctorName}</p>
                    </td>
                </tr>
            </table>

            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""margin: 30px 0;"">
                <tr>
                    <td align=""center"">
                        <table cellpadding=""0"" cellspacing=""0"" border=""0"">
                            <tr>
                                <td align=""center"" style=""border-radius: 6px; background-color: {PrimaryColor};"">
                                    <a href=""https://www.clinixhms.com/appointments"" target=""_blank"" style=""display: inline-block; padding: 14px 32px; color: #ffffff; font-size: 15px; font-weight: 600; text-decoration: none; border-radius: 6px;"">View Appointment</a>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>";

        return (subject, WrapEmailTemplate(InfoColor, "📅 RESCHEDULED", "Appointment Rescheduled", content));
        }

    public static (string Subject, string HtmlBody) AppointmentRescheduled_Doctor(
        string doctorName, string patientName, DateTimeOffset oldStart, DateTimeOffset newStart, DateTimeOffset newEnd)
        {
        var subject = $"Appointment Rescheduled: {patientName}";

        var content = $@"
            <p style=""margin: 0 0 30px 0; color: {TextColor}; font-size: 16px; line-height: 1.6;"">Dear <strong>Dr. {doctorName}</strong>,</p>
            
            <p style=""margin: 0 0 30px 0; color: {TextColor}; font-size: 16px; line-height: 1.6;"">An appointment with <strong>{patientName}</strong> has been rescheduled.</p>

            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: {BackgroundColor}; border-radius: 6px; border: 1px solid {BorderColor};"">
                <tr>
                    <td style=""padding: 20px;"">
                        <p style=""margin: 0 0 12px 0; color: {LightTextColor}; font-size: 13px;"">Previous: <span style=""text-decoration: line-through;"">{oldStart:MMM dd, yyyy} at {oldStart:hh:mm tt}</span></p>
                        <p style=""margin: 0; color: {TextColor}; font-size: 16px; font-weight: 600;"">New: {newStart:dddd, MMMM dd, yyyy}</p>
                        <p style=""margin: 4px 0 0 0; color: {TextColor}; font-size: 15px; font-weight: 600;"">{newStart:hh:mm tt} – {newEnd:hh:mm tt}</p>
                    </td>
                </tr>
            </table>

            <p style=""margin: 30px 0 0 0; color: {LightTextColor}; font-size: 14px; text-align: center;"">Your schedule has been updated accordingly.</p>";

        return (subject, WrapEmailTemplate(InfoColor, "📅 UPDATED", "Schedule Updated", content));
        }

    // ========================================
    // APPOINTMENT CANCELLED
    // ========================================

    public static (string Subject, string HtmlBody) AppointmentCancelled_Patient(
        string patientName, string doctorName, DateTimeOffset start, string? reason)
        {
        var subject = "Appointment Cancelled – Clinix Hospital";

        var reasonSection = !string.IsNullOrWhiteSpace(reason)
            ? $@"<table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""margin: 20px 0;"">
                    <tr>
                        <td style=""padding: 16px; background-color: #fef3c7; border-left: 4px solid {AlertColor}; border-radius: 4px;"">
                            <p style=""margin: 0 0 8px 0; color: {TextColor}; font-size: 13px; font-weight: 600;"">Cancellation Reason:</p>
                            <p style=""margin: 0; color: {TextColor}; font-size: 14px;"">{reason}</p>
                        </td>
                    </tr>
                </table>"
            : "";

        var content = $@"
            <p style=""margin: 0 0 30px 0; color: {TextColor}; font-size: 16px; line-height: 1.6;"">Dear <strong>{patientName}</strong>,</p>
            
            <p style=""margin: 0 0 30px 0; color: {TextColor}; font-size: 16px; line-height: 1.6;"">Your appointment has been cancelled.</p>

            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: {BackgroundColor}; border-radius: 6px; border: 1px solid {BorderColor};"">
                <tr>
                    <td style=""padding: 20px;"">
                        <p style=""margin: 0 0 4px 0; color: {LightTextColor}; font-size: 13px;"">Original Appointment</p>
                        <p style=""margin: 0; color: {TextColor}; font-size: 16px; font-weight: 600;"">{start:dddd, MMMM dd, yyyy}</p>
                        <p style=""margin: 4px 0 0 0; color: {TextColor}; font-size: 15px;"">{start:hh:mm tt}</p>
                        <p style=""margin: 12px 0 0 0; color: {LightTextColor}; font-size: 13px;"">Doctor: <strong style=""color: {TextColor};"">Dr. {doctorName}</strong></p>
                    </td>
                </tr>
            </table>

            {reasonSection}

            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""margin: 30px 0;"">
                <tr>
                    <td align=""center"">
                        <table cellpadding=""0"" cellspacing=""0"" border=""0"">
                            <tr>
                                <td align=""center"" style=""border-radius: 6px; background-color: {PrimaryColor};"">
                                    <a href=""https://www.clinixhms.com/appointments"" target=""_blank"" style=""display: inline-block; padding: 14px 32px; color: #ffffff; font-size: 15px; font-weight: 600; text-decoration: none; border-radius: 6px;"">Book New Appointment</a>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>

            <p style=""margin: 20px 0 0 0; color: {LightTextColor}; font-size: 14px; text-align: center;"">We understand plans change. Feel free to schedule a new appointment anytime.</p>";

        return (subject, WrapEmailTemplate(WarningColor, "✕ CANCELLED", "Appointment Cancelled", content));
        }

    public static (string Subject, string HtmlBody) AppointmentCancelled_Doctor(
        string doctorName, string patientName, DateTimeOffset start)
        {
        var subject = $"Appointment Cancelled: {patientName}";

        var content = $@"
            <p style=""margin: 0 0 30px 0; color: {TextColor}; font-size: 16px; line-height: 1.6;"">Dear <strong>Dr. {doctorName}</strong>,</p>
            
            <p style=""margin: 0 0 30px 0; color: {TextColor}; font-size: 16px; line-height: 1.6;"">An appointment with <strong>{patientName}</strong> has been cancelled.</p>

            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: {BackgroundColor}; border-radius: 6px; border: 1px solid {BorderColor};"">
                <tr>
                    <td style=""padding: 20px;"">
                        <p style=""margin: 0; color: {TextColor}; font-size: 16px; font-weight: 600;"">{start:dddd, MMMM dd, yyyy}</p>
                        <p style=""margin: 4px 0 0 0; color: {TextColor}; font-size: 15px;"">{start:hh:mm tt}</p>
                    </td>
                </tr>
            </table>

            <p style=""margin: 30px 0 0 0; color: {LightTextColor}; font-size: 14px; text-align: center;"">This time slot is now available in your schedule.</p>";

        return (subject, WrapEmailTemplate(WarningColor, "✕ CANCELLED", "Appointment Cancelled", content));
        }

    // ========================================
    // APPOINTMENT COMPLETED
    // ========================================

    public static (string Subject, string HtmlBody) AppointmentCompleted_Patient(
        string patientName, string doctorName, DateTimeOffset completedAt)
        {
        var subject = "Visit Completed – Clinix Hospital";

        var content = $@"
            <p style=""margin: 0 0 30px 0; color: {TextColor}; font-size: 16px; line-height: 1.6;"">Dear <strong>{patientName}</strong>,</p>
            
            <p style=""margin: 0 0 30px 0; color: {TextColor}; font-size: 16px; line-height: 1.6;"">Thank you for your visit to Clinix Hospital.</p>

            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: {BackgroundColor}; border-radius: 6px; border: 1px solid {BorderColor};"">
                <tr>
                    <td style=""padding: 20px;"">
                        <h3 style=""margin: 0 0 16px 0; color: {TextColor}; font-size: 15px; font-weight: 600;"">Visit Summary</h3>
                        <p style=""margin: 0; color: {LightTextColor}; font-size: 13px;"">Doctor</p>
                        <p style=""margin: 4px 0 12px 0; color: {TextColor}; font-size: 15px; font-weight: 600;"">Dr. {doctorName}</p>
                        <p style=""margin: 0; color: {LightTextColor}; font-size: 13px;"">Date</p>
                        <p style=""margin: 4px 0 0 0; color: {TextColor}; font-size: 15px;"">{completedAt:dddd, MMMM dd, yyyy} at {completedAt:hh:mm tt}</p>
                    </td>
                </tr>
            </table>

            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""margin: 30px 0;"">
                <tr>
                    <td style=""padding: 20px; background-color: #e0f2fe; border-left: 4px solid {InfoColor}; border-radius: 4px;"">
                        <p style=""margin: 0 0 12px 0; color: {TextColor}; font-size: 14px; font-weight: 600;"">📋 Next Steps</p>
                        <ul style=""margin: 0; padding-left: 20px; color: {TextColor}; font-size: 14px; line-height: 1.8;"">
                            <li style=""margin-bottom: 6px;"">Check your patient portal for test results and prescriptions</li>
                            <li style=""margin-bottom: 6px;"">You may receive a follow-up appointment notification</li>
                            <li>Contact us if you have any questions about your visit</li>
                        </ul>
                    </td>
                </tr>
            </table>

            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""margin: 30px 0;"">
                <tr>
                    <td align=""center"">
                        <table cellpadding=""0"" cellspacing=""0"" border=""0"">
                            <tr>
                                <td align=""center"" style=""border-radius: 6px; background-color: {PrimaryColor};"">
                                    <a href=""https://www.clinixhms.com/patient/dashboard"" target=""_blank"" style=""display: inline-block; padding: 14px 32px; color: #ffffff; font-size: 15px; font-weight: 600; text-decoration: none; border-radius: 6px;"">View Patient Portal</a>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>

            <p style=""margin: 20px 0 0 0; color: {LightTextColor}; font-size: 14px; text-align: center;"">We hope you feel better and appreciate your trust in our care.</p>";

        return (subject, WrapEmailTemplate(SuccessColor, "✓ COMPLETED", "Visit Completed", content));
        }

    public static (string Subject, string HtmlBody) AppointmentCompleted_Doctor(
        string doctorName, string patientName, DateTimeOffset completedAt)
        {
        var subject = $"Appointment Completed: {patientName}";

        var content = $@"
            <p style=""margin: 0 0 30px 0; color: {TextColor}; font-size: 16px; line-height: 1.6;"">Dear <strong>Dr. {doctorName}</strong>,</p>
            
            <p style=""margin: 0 0 30px 0; color: {TextColor}; font-size: 16px; line-height: 1.6;"">The appointment with <strong>{patientName}</strong> has been marked as completed.</p>

            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: {BackgroundColor}; border-radius: 6px; border: 1px solid {BorderColor};"">
                <tr>
                    <td style=""padding: 20px;"">
                        <p style=""margin: 0; color: {TextColor}; font-size: 16px; font-weight: 600;"">{completedAt:dddd, MMMM dd, yyyy}</p>
                        <p style=""margin: 4px 0 0 0; color: {TextColor}; font-size: 15px;"">{completedAt:hh:mm tt}</p>
                    </td>
                </tr>
            </table>

            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""margin: 30px 0;"">
                <tr>
                    <td style=""padding: 16px; background-color: #fef3c7; border-left: 4px solid {AlertColor}; border-radius: 4px;"">
                        <p style=""margin: 0; color: {TextColor}; font-size: 14px;"">⚠️ <strong>Reminder:</strong> Please complete visit notes and upload test results to the patient portal if not already done.</p>
                    </td>
                </tr>
            </table>";

        return (subject, WrapEmailTemplate(SuccessColor, "✓ COMPLETED", "Appointment Completed", content));
        }

    // ========================================
    // APPOINTMENT APPROVED
    // ========================================

    public static (string Subject, string HtmlBody) AppointmentApproved_Patient(
        string patientName, string doctorName, DateTimeOffset start, DateTimeOffset end)
        {
        var subject = "Appointment Approved – Clinix Hospital";

        var content = $@"
            <p style=""margin: 0 0 30px 0; color: {TextColor}; font-size: 16px; line-height: 1.6;"">Dear <strong>{patientName}</strong>,</p>
            
            <p style=""margin: 0 0 30px 0; color: {TextColor}; font-size: 16px; line-height: 1.6;"">Great news! Your appointment request has been approved by <strong>Dr. {doctorName}</strong>.</p>

            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #d1fae5; border-radius: 6px; border: 1px solid {SuccessColor};"">
                <tr>
                    <td style=""padding: 24px;"">
                        <p style=""margin: 0 0 16px 0; color: {SuccessColor}; font-size: 14px; font-weight: 600;"">✓ CONFIRMED APPOINTMENT</p>
                        <p style=""margin: 0; color: {TextColor}; font-size: 17px; font-weight: 600;"">{start:dddd, MMMM dd, yyyy}</p>
                        <p style=""margin: 4px 0 0 0; color: {TextColor}; font-size: 16px;"">{start:hh:mm tt} – {end:hh:mm tt}</p>
                        <p style=""margin: 12px 0 0 0; color: {TextColor}; font-size: 15px;"">Dr. {doctorName}</p>
                    </td>
                </tr>
            </table>

            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""margin: 30px 0;"">
                <tr>
                    <td align=""center"">
                        <table cellpadding=""0"" cellspacing=""0"" border=""0"">
                            <tr>
                                <td align=""center"" style=""border-radius: 6px; background-color: {PrimaryColor};"">
                                    <a href=""https://www.clinixhms.com/appointments"" target=""_blank"" style=""display: inline-block; padding: 14px 32px; color: #ffffff; font-size: 15px; font-weight: 600; text-decoration: none; border-radius: 6px;"">View Appointment</a>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>

            <p style=""margin: 20px 0 0 0; color: {LightTextColor}; font-size: 14px; text-align: center;"">We look forward to seeing you!</p>";

        return (subject, WrapEmailTemplate(SuccessColor, "✓ APPROVED", "Appointment Approved", content));
        }

    public static (string Subject, string HtmlBody) AppointmentApproved_Doctor(
        string doctorName, string patientName, DateTimeOffset start, DateTimeOffset end)
        {
        var subject = $"Appointment Approved: {patientName}";

        var content = $@"
            <p style=""margin: 0 0 30px 0; color: {TextColor}; font-size: 16px; line-height: 1.6;"">Dear <strong>Dr. {doctorName}</strong>,</p>
            
            <p style=""margin: 0 0 30px 0; color: {TextColor}; font-size: 16px; line-height: 1.6;"">You have approved an appointment request from <strong>{patientName}</strong>.</p>

            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: {BackgroundColor}; border-radius: 6px; border: 1px solid {BorderColor};"">
                <tr>
                    <td style=""padding: 20px;"">
                        <p style=""margin: 0; color: {TextColor}; font-size: 16px; font-weight: 600;"">{start:dddd, MMMM dd, yyyy}</p>
                        <p style=""margin: 4px 0 0 0; color: {TextColor}; font-size: 15px;"">{start:hh:mm tt} – {end:hh:mm tt}</p>
                    </td>
                </tr>
            </table>

            <p style=""margin: 30px 0 0 0; color: {LightTextColor}; font-size: 14px; text-align: center;"">The patient has been notified of the confirmation.</p>";

        return (subject, WrapEmailTemplate(SuccessColor, "✓ APPROVED", "Appointment Approved", content));
        }

    // ========================================
    // APPOINTMENT REJECTED
    // ========================================

    public static (string Subject, string HtmlBody) AppointmentRejected_Patient(
        string patientName, string doctorName, DateTimeOffset requestedStart, string? reason)
        {
        var subject = "Appointment Request – Alternative Time Needed";

        var reasonSection = !string.IsNullOrWhiteSpace(reason)
            ? $@"<table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""margin: 20px 0;"">
                    <tr>
                        <td style=""padding: 16px; background-color: #fef3c7; border-left: 4px solid {AlertColor}; border-radius: 4px;"">
                            <p style=""margin: 0 0 8px 0; color: {TextColor}; font-size: 13px; font-weight: 600;"">Reason:</p>
                            <p style=""margin: 0; color: {TextColor}; font-size: 14px;"">{reason}</p>
                        </td>
                    </tr>
                </table>"
            : "";

        var content = $@"
            <p style=""margin: 0 0 30px 0; color: {TextColor}; font-size: 16px; line-height: 1.6;"">Dear <strong>{patientName}</strong>,</p>
            
            <p style=""margin: 0 0 30px 0; color: {TextColor}; font-size: 16px; line-height: 1.6;"">Unfortunately, your requested appointment time with <strong>Dr. {doctorName}</strong> is not available.</p>

            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: {BackgroundColor}; border-radius: 6px; border: 1px solid {BorderColor};"">
                <tr>
                    <td style=""padding: 20px;"">
                        <p style=""margin: 0 0 4px 0; color: {LightTextColor}; font-size: 13px;"">Requested Time</p>
                        <p style=""margin: 0; color: {TextColor}; font-size: 16px;"">{requestedStart:dddd, MMMM dd, yyyy}</p>
                        <p style=""margin: 4px 0 0 0; color: {TextColor}; font-size: 15px;"">{requestedStart:hh:mm tt}</p>
                    </td>
                </tr>
            </table>

            {reasonSection}

            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""margin: 30px 0;"">
                <tr>
                    <td align=""center"">
                        <table cellpadding=""0"" cellspacing=""0"" border=""0"">
                            <tr>
                                <td align=""center"" style=""border-radius: 6px; background-color: {PrimaryColor};"">
                                    <a href=""https://www.clinixhms.com/appointments"" target=""_blank"" style=""display: inline-block; padding: 14px 32px; color: #ffffff; font-size: 15px; font-weight: 600; text-decoration: none; border-radius: 6px;"">Choose Alternative Time</a>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>

            <p style=""margin: 20px 0 0 0; color: {LightTextColor}; font-size: 14px; text-align: center;"">We apologize for the inconvenience. Our team is ready to help you find a suitable time.</p>";

        return (subject, WrapEmailTemplate(WarningColor, "⚠ NOT AVAILABLE", "Alternative Time Needed", content));
        }

    // ========================================
    // FOLLOW-UP CREATED
    // ========================================

    public static (string Subject, string HtmlBody) FollowUpCreated_Patient(
        string patientName, string doctorName, DateTimeOffset dueBy, string? reason)
        {
        var subject = "Follow-Up Appointment Recommended – Clinix Hospital";

        var reasonSection = !string.IsNullOrWhiteSpace(reason)
            ? $@"<table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""margin: 20px 0;"">
                    <tr>
                        <td style=""padding: 16px; background-color: #e0f2fe; border-left: 4px solid {InfoColor}; border-radius: 4px;"">
                            <p style=""margin: 0 0 8px 0; color: {TextColor}; font-size: 13px; font-weight: 600;"">Purpose:</p>
                            <p style=""margin: 0; color: {TextColor}; font-size: 14px;"">{reason}</p>
                        </td>
                    </tr>
                </table>"
            : "";

        var content = $@"
            <p style=""margin: 0 0 30px 0; color: {TextColor}; font-size: 16px; line-height: 1.6;"">Dear <strong>{patientName}</strong>,</p>
            
            <p style=""margin: 0 0 30px 0; color: {TextColor}; font-size: 16px; line-height: 1.6;"">Based on your recent visit, <strong>Dr. {doctorName}</strong> has recommended a follow-up appointment.</p>

            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: {BackgroundColor}; border-radius: 6px; border: 1px solid {BorderColor};"">
                <tr>
                    <td style=""padding: 20px;"">
                        <p style=""margin: 0 0 4px 0; color: {LightTextColor}; font-size: 13px;"">Recommended By</p>
                        <p style=""margin: 0 0 16px 0; color: {TextColor}; font-size: 16px; font-weight: 600;"">Dr. {doctorName}</p>
                        <p style=""margin: 0 0 4px 0; color: {LightTextColor}; font-size: 13px;"">Schedule By</p>
                        <p style=""margin: 0; color: {TextColor}; font-size: 16px; font-weight: 600;"">{dueBy:dddd, MMMM dd, yyyy}</p>
                    </td>
                </tr>
            </table>

            {reasonSection}

            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""margin: 30px 0;"">
                <tr>
                    <td align=""center"">
                        <table cellpadding=""0"" cellspacing=""0"" border=""0"">
                            <tr>
                                <td align=""center"" style=""border-radius: 6px; background-color: {PrimaryColor};"">
                                    <a href=""https://www.clinixhms.com/appointments/schedule"" target=""_blank"" style=""display: inline-block; padding: 14px 32px; color: #ffffff; font-size: 15px; font-weight: 600; text-decoration: none; border-radius: 6px;"">Schedule Follow-Up</a>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>

            <p style=""margin: 20px 0 0 0; color: {LightTextColor}; font-size: 14px; text-align: center;"">Your health is our priority. Please schedule your follow-up at your earliest convenience.</p>";

        return (subject, WrapEmailTemplate(InfoColor, "🔄 FOLLOW-UP", "Follow-Up Recommended", content));
        }

    // ========================================
    // FOLLOW-UP REMINDER
    // ========================================

    public static (string Subject, string HtmlBody) FollowUpReminder_Patient(
        string patientName, string doctorName, DateTimeOffset dueBy, string? reason)
        {
        var subject = "Reminder: Follow-Up Appointment Due – Clinix Hospital";

        var reasonSection = !string.IsNullOrWhiteSpace(reason)
            ? $@"<p style=""margin: 16px 0 0 0; color: {TextColor}; font-size: 14px;""><strong>Purpose:</strong> {reason}</p>"
            : "";

        var content = $@"
            <p style=""margin: 0 0 30px 0; color: {TextColor}; font-size: 16px; line-height: 1.6;"">Dear <strong>{patientName}</strong>,</p>
            
            <p style=""margin: 0 0 30px 0; color: {TextColor}; font-size: 16px; line-height: 1.6;"">This is a friendly reminder about your pending follow-up appointment with <strong>Dr. {doctorName}</strong>.</p>

            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #fef3c7; border-radius: 6px; border: 1px solid {AlertColor};"">
                <tr>
                    <td style=""padding: 24px; text-align: center;"">
                        <p style=""margin: 0 0 8px 0; color: {AlertColor}; font-size: 13px; font-weight: 600; text-transform: uppercase;"">⚠️ Action Required</p>
                        <p style=""margin: 0; color: {TextColor}; font-size: 18px; font-weight: 600;"">Due By: {dueBy:MMMM dd, yyyy}</p>
                        {reasonSection}
                    </td>
                </tr>
            </table>

            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""margin: 30px 0;"">
                <tr>
                    <td align=""center"">
                        <table cellpadding=""0"" cellspacing=""0"" border=""0"">
                            <tr>
                                <td align=""center"" style=""border-radius: 6px; background-color: {PrimaryColor};"">
                                    <a href=""https://www.clinixhms.com/appointments/schedule"" target=""_blank"" style=""display: inline-block; padding: 14px 32px; color: #ffffff; font-size: 15px; font-weight: 600; text-decoration: none; border-radius: 6px;"">Schedule Now</a>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>

            <p style=""margin: 20px 0 0 0; color: {LightTextColor}; font-size: 14px; text-align: center;"">Please schedule this follow-up soon to ensure continuity of care.</p>";

        return (subject, WrapEmailTemplate(AlertColor, "⚠ REMINDER", "Follow-Up Reminder", content));
        }

    // ========================================
    // SMS TEMPLATES (Concise versions)
    // ========================================

    public static string AppointmentScheduled_SMS_Patient(string patientName, string doctorName, DateTimeOffset start)
        => $"Clinix: Appointment confirmed with Dr. {doctorName} on {start:MMM dd} at {start:hh:mm tt}. Arrive 15 min early. Manage: clinixhms.com";

    public static string AppointmentScheduled_SMS_Doctor(string doctorName, string patientName, DateTimeOffset start)
        => $"Clinix: New appointment - {patientName} scheduled for {start:MMM dd} at {start:hh:mm tt}. View: clinixhms.com/doctor";

    public static string AppointmentRescheduled_SMS_Patient(string patientName, string doctorName, DateTimeOffset newStart)
        => $"Clinix: Your appointment with Dr. {doctorName} moved to {newStart:MMM dd} at {newStart:hh:mm tt}. Questions? Call 1-800-CLINIX-CARE";

    public static string AppointmentRescheduled_SMS_Doctor(string patientName, DateTimeOffset newStart)
        => $"Clinix: Appointment with {patientName} rescheduled to {newStart:MMM dd} at {newStart:hh:mm tt}.";

    public static string AppointmentCancelled_SMS_Patient(string patientName, DateTimeOffset start)
        => $"Clinix: Your appointment on {start:MMM dd} at {start:hh:mm tt} has been cancelled. Reschedule: clinixhms.com or call 1-800-CLINIX-CARE";

    public static string AppointmentCancelled_SMS_Doctor(string patientName, DateTimeOffset start)
        => $"Clinix: Appointment with {patientName} on {start:MMM dd} at {start:hh:mm tt} cancelled. Time slot now available.";

    public static string AppointmentCompleted_SMS_Patient(string patientName, string doctorName)
        => $"Clinix: Visit with Dr. {doctorName} completed. Check your portal for results: clinixhms.com/patient";

    public static string AppointmentCompleted_SMS_Doctor(string patientName)
        => $"Clinix: Appointment with {patientName} marked completed. Update visit notes if needed.";

    public static string AppointmentApproved_SMS_Patient(string patientName, string doctorName, DateTimeOffset start)
        => $"Clinix: Great news! Dr. {doctorName} approved your appointment for {start:MMM dd} at {start:hh:mm tt}. Arrive 15 min early.";

    public static string AppointmentRejected_SMS_Patient(string patientName, string doctorName, DateTimeOffset requestedStart)
        => $"Clinix: Appointment with Dr. {doctorName} for {requestedStart:MMM dd} at {requestedStart:hh:mm tt} not available. Choose alternative: clinixhms.com";

    public static string FollowUpReminder_SMS(string patientName, string doctorName, DateTimeOffset dueBy)
        => $"Clinix Reminder: Schedule your follow-up with Dr. {doctorName} by {dueBy:MMM dd}. Book: clinixhms.com or call 1-800-CLINIX-CARE";

    public static string AppointmentReminder_SMS(string patientName, string doctorName, DateTimeOffset start)
        => $"Clinix: Your appointment with Dr. {doctorName} is tomorrow at {start:hh:mm tt}. Arrive 15 min early. Reschedule: clinixhms.com";
    }
