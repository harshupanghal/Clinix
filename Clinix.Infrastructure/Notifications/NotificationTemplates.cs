using Clinix.Domain.Enums;

namespace Clinix.Infrastructure.Notifications;

/// <summary>
/// Professional, human-friendly notification templates for all appointment and follow-up scenarios.
/// Provides both email (detailed) and SMS (concise) versions with personalization.
/// </summary>
public static class NotificationTemplates
    {
    /// <summary>
    /// Appointment scheduled confirmation for PATIENT
    /// </summary>
    public static (string Subject, string Body) AppointmentScheduled_Patient(
        string patientName, string doctorName, DateTimeOffset start, DateTimeOffset end)
        {
        var subject = "✅ Appointment Confirmed - Clinix HMS";
        var body = $@"Dear {patientName},

Your appointment has been successfully scheduled!

📅 Date: {start:dddd, MMMM dd, yyyy}
🕐 Time: {start:hh:mm tt} - {end:hh:mm tt}
👨‍⚕️ Doctor: Dr. {doctorName}

Please arrive 10 minutes early for check-in.

If you need to reschedule or cancel, please contact us at least 24 hours in advance.

Best regards,
Clinix Hospital Management System
📞 Support: 1-800-CLINIX
🌐 www.clinixhms.com";

        return (subject, body);
        }


    /// <summary>
    /// Appointment scheduled notification for DOCTOR
    /// </summary>
    public static (string Subject, string Body) AppointmentScheduled_Doctor(
        string doctorName, string patientName, DateTimeOffset start, DateTimeOffset end, AppointmentType type)
        {
        var subject = "🔔 New Appointment Scheduled";
        var body = $@"Dear Dr. {doctorName},

A new appointment has been scheduled with you.

📋 Patient: {patientName}
📅 Date: {start:dddd, MMMM dd, yyyy}
🕐 Time: {start:hh:mm tt} - {end:hh:mm tt}
🏥 Type: {type}

Please review patient history before the appointment.

Clinix HMS
🌐 Dashboard: www.clinixhms.com/doctor/schedule";

        return (subject, body);
        }

    /// <summary>
    /// SMS version for appointment scheduled (Patient)
    /// </summary>
    public static string AppointmentScheduled_SMS_Patient(string patientName, string doctorName, DateTimeOffset start)
        => $"Hi {patientName}, your appointment with Dr. {doctorName} is confirmed for {start:MMM dd} at {start:hh:mm tt}. Arrive 10 min early. -Clinix HMS";

    /// <summary>
    /// Appointment cancelled notification for PATIENT
    /// </summary>
    public static (string Subject, string Body) AppointmentCancelled_Patient(
        string patientName, string doctorName, DateTimeOffset start, string? reason)
        {
        var subject = "❌ Appointment Cancelled - Clinix HMS";
        var reasonText = !string.IsNullOrWhiteSpace(reason) ? $"\n\nReason: {reason}" : "";
        var body = $@"Dear {patientName},

Your appointment has been cancelled.

📅 Original Date: {start:dddd, MMMM dd, yyyy}
🕐 Original Time: {start:hh:mm tt}
👨‍⚕️ Doctor: Dr. {doctorName}{reasonText}

To reschedule, please contact us or book online.

Best regards,
Clinix Hospital Management System
📞 Support: 1-800-CLINIX";

        return (subject, body);
        }

    /// <summary>
    /// Appointment cancelled notification for DOCTOR
    /// </summary>
    public static (string Subject, string Body) AppointmentCancelled_Doctor(
        string doctorName, string patientName, DateTimeOffset start)
        {
        var subject = "🔔 Appointment Cancelled";
        var body = $@"Dear Dr. {doctorName},

An appointment has been cancelled.

📋 Patient: {patientName}
📅 Date: {start:dddd, MMMM dd, yyyy}
🕐 Time: {start:hh:mm tt}

Your schedule has been updated accordingly.

Clinix HMS";

        return (subject, body);
        }

    /// <summary>
    /// SMS for appointment cancelled
    /// </summary>
    public static string AppointmentCancelled_SMS(string name, DateTimeOffset start)
        => $"Hi {name}, your appointment on {start:MMM dd} at {start:hh:mm tt} has been cancelled. Please reschedule if needed. -Clinix HMS";

    /// <summary>
    /// Appointment rescheduled for PATIENT
    /// </summary>
    public static (string Subject, string Body) AppointmentRescheduled_Patient(
        string patientName, string doctorName, DateTimeOffset oldStart, DateTimeOffset newStart, DateTimeOffset newEnd)
        {
        var subject = "📅 Appointment Rescheduled - Clinix HMS";
        var body = $@"Dear {patientName},

Your appointment has been rescheduled.

❌ Previous: {oldStart:MMM dd} at {oldStart:hh:mm tt}
✅ New: {newStart:dddd, MMMM dd, yyyy} at {newStart:hh:mm tt} - {newEnd:hh:mm tt}
👨‍⚕️ Doctor: Dr. {doctorName}

Please arrive 10 minutes early.

Best regards,
Clinix HMS
📞 Support: 1-800-CLINIX";

        return (subject, body);
        }

    /// <summary>
    /// 24-hour appointment reminder (SMS - concise)
    /// </summary>
    public static string AppointmentReminder_SMS(string patientName, string doctorName, DateTimeOffset start)
        => $"⏰ Reminder: Your appointment with Dr. {doctorName} is tomorrow at {start:hh:mm tt}. Please confirm or reschedule. -Clinix HMS";

    /// <summary>
    /// Follow-up created notification for PATIENT
    /// </summary>
    public static (string Subject, string Body) FollowUpCreated_Patient(
        string patientName, string doctorName, DateTimeOffset dueBy, string? reason)
        {
        var subject = "🔄 Follow-Up Required - Clinix HMS";
        var reasonText = !string.IsNullOrWhiteSpace(reason) ? $"\n\n📝 Reason: {reason}" : "";
        var body = $@"Dear {patientName},

A follow-up appointment has been recommended for you.

👨‍⚕️ Doctor: Dr. {doctorName}
📅 Due By: {dueBy:dddd, MMMM dd, yyyy}{reasonText}

Please schedule your follow-up appointment at your earliest convenience.

To book online, visit: www.clinixhms.com/appointments/schedule
Or call us at: 1-800-CLINIX

Your health is our priority!

Best regards,
Clinix Hospital Management System";

        return (subject, body);
        }

    /// <summary>
    /// Follow-up reminder (sent when due date approaches)
    /// </summary>
    public static (string Subject, string Body) FollowUpReminder_Patient(
        string patientName, string doctorName, DateTimeOffset dueBy, string? reason)
        {
        var subject = "⚠️ Follow-Up Reminder - Action Required";
        var reasonText = !string.IsNullOrWhiteSpace(reason) ? $"\n\n📝 Purpose: {reason}" : "";
        var body = $@"Dear {patientName},

This is a reminder about your pending follow-up appointment.

👨‍⚕️ Doctor: Dr. {doctorName}
📅 Due By: {dueBy:dddd, MMMM dd, yyyy}{reasonText}

⚠️ Please schedule this follow-up soon to ensure continuity of care.

📲 Book Now: www.clinixhms.com/appointments/schedule
📞 Call: 1-800-CLINIX

We care about your health!

Best regards,
Clinix HMS";

        return (subject, body);
        }

    /// <summary>
    /// SMS for follow-up reminder
    /// </summary>
    public static string FollowUpReminder_SMS(string patientName, string doctorName, DateTimeOffset dueBy)
        => $"⚠️ Hi {patientName}, reminder: Schedule your follow-up with Dr. {doctorName} by {dueBy:MMM dd}. Book at clinixhms.com or call 1-800-CLINIX";

    /// <summary>
    /// Marketing/Wellness message template (separate from medical notifications)
    /// </summary>
    public static (string Subject, string Body) WellnessMarketing_Patient(string patientName)
        {
        var subject = "💚 Your Health Matters - Clinix Wellness Tips";
        var body = $@"Dear {patientName},

Thank you for trusting Clinix HMS with your healthcare!

Here are some wellness tips for you:
✅ Stay hydrated - drink 8 glasses of water daily
✅ Exercise regularly - at least 30 minutes a day
✅ Get adequate sleep - 7-8 hours per night
✅ Schedule regular check-ups

We're here to support your health journey!

📲 Book Your Next Check-Up: www.clinixhms.com
📞 Questions? Call: 1-800-CLINIX

Stay healthy!
Clinix HMS Team";

        return (subject, body);
        }
    }
