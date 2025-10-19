//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Clinix.Domain.Entities.FollowUp;
    
//    /// <summary>
//    /// Represents a follow-up root record that links a patient to a follow-up workflow.
//    /// Created either automatically (from appointment) or manually by staff/admin.
//    /// </summary>
//    public class FollowUp
//        {
//        [Key]
//        public long Id { get; set; }

//        /// <summary>FK to patient</summary>
//        public long PatientId { get; set; }

//        /// <summary>Optional FK to originating appointment</summary>
//        public long? AppointmentId { get; set; }

//        [Required]
//        [MaxLength(64)]
//        public string Source { get; set; } = "Manual"; // AutoFromAppointment | Manual | Campaign

//        [Required]
//        [MaxLength(32)]
//        public string Status { get; set; } = FollowUpStatus.Active;

//        [MaxLength(1024)]
//        public string? Notes { get; set; }

//        /// <summary>Snapshot whether consent was given at creation time</summary>
//        public bool ConsentGiven { get; set; }

//        public int Priority { get; set; } = 1; // 0=Low,1=Normal,2=High

//        // Soft delete
//        public bool IsDeleted { get; set; } = false;

//        // Audit fields
//        public long CreatedByUserId { get; set; }
//        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
//        public long? LastModifiedByUserId { get; set; }
//        public DateTime? LastModifiedAtUtc { get; set; }

//                // Navigation placeholder
//        public ICollection<FollowUpItem>? Items { get; set; }
//    [Timestamp]
//    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

//    }

//public static class FollowUpStatus
//        {
//        public const string Active = "Active";
//        public const string Paused = "Paused";
//        public const string Cancelled = "Cancelled";
//        public const string Completed = "Completed";
//        }

//    /// <summary>
//    /// Individual scheduled message / reminder / interaction belonging to a FollowUp.
//    /// </summary>
//    public class FollowUpItem
//        {
//        [Key]
//        public long Id { get; set; }

//        public long FollowUpId { get; set; }

//              /// <summary>Reminder, CheckIn, Marketing, MedicationReminder, Survey</summary>
//                [Required]
//                [MaxLength(64)]
//        public string Type { get; set; } = "Reminder";

//        [Required]
//                [MaxLength(32)]
//        public string Channel { get; set; } = "InApp"; // Sms | Email | InApp | Push

//         public DateTime ScheduledAtUtc { get; set; }

//        public int AttemptCount { get; set; } = 0;
//        public int MaxAttempts { get; set; } = 3;
//        public DateTime? NextAttemptAtUtc { get; set; }

//        [Required]
//                [MaxLength(32)]
//        public string Status { get; set; } = FollowUpItemStatus.Pending;

//        public long? MessageTemplateId { get; set; }
//        public DateTime? LastAttemptedAtUtc { get; set; }
//        public DateTime? LastModifiedAtUtc { get; set; }
//        public DateTime? SentAtUtc { get; set; }
//        [MaxLength(1024)]
//        public string? FailureReason { get; set; }

//               // Allow provider-specific metadata
//        public string? ResultMetadataJson { get; set; }

//                // Audit
//        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
//        public long CreatedByUserId { get; set; }

//        public bool IsDeleted { get; set; } = false;
//        }

//     public static class FollowUpItemStatus
//        {
//        public const string Pending = "Pending";
//        public const string Sent = "Sent";
//        public const string Failed = "Failed";
//        public const string Skipped = "Skipped";
//        public const string Completed = "Completed";
//        }

//       /// <summary>
//         /// Message template that can be used for rendering sends. Stored in DB and editable by admin.
//         /// Channel determines which provider/format to use.
//         /// </summary>
//    public class MessageTemplate
//        {
//        [Key]
//        public long Id { get; set; }

//        [Required]
//                [MaxLength(200)]
//        public string Name { get; set; } = string.Empty;

//        [Required]
//                [MaxLength(32)]
//        public string Channel { get; set; } = "Email"; // Email | Sms | InApp | Push

//        [MaxLength(512)]
//        public string? Subject { get; set; } // for email

//        [Required]
//        public string Body { get; set; } = string.Empty; // templated body (Handlebars / Liquid)

//         public bool IsSystemTemplate { get; set; } = false;

//         public bool IsDeleted { get; set; } = false;

//         public long CreatedByUserId { get; set; }
//        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
//        }

//        /// <summary>
//         /// Minimal audit record for followup operations.
//         /// </summary>
//    public class FollowUpAudit
//        {
//        [Key]
//        public long Id { get; set; }
//        public string EntityType { get; set; } = "FollowUp";
//        public long EntityId { get; set; }
//        public string Action { get; set; } = string.Empty; // Created, Sent, Failed, Retried ...
//        public long ActorUserId { get; set; }
//        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
//        public string? DetailsJson { get; set; }
//        }

//        /// <summary>
//         /// Patient communication preferences / opt-out per channel.
//         /// </summary>
//    public class CommunicationPreference
//        {
//        [Key]
//        public long Id { get; set; }
//        public long PatientId { get; set; }
//        [MaxLength(32)]
//        public string Channel { get; set; } = "Sms"; // Sms | Email | InApp | Push
//        public bool IsOptedOut { get; set; }
//        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
//        public long UpdatedByUserId { get; set; }
//        }
    

