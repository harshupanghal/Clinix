using System;

namespace Clinix.Domain.Enums;

/// <summary>
/// Current lifecycle status for a FollowUpRecord.
/// </summary>
public enum FollowUpStatus
    {
    Pending = 0,
    Active = 1,
    Completed = 2,
    Cancelled = 3,
    Archived = 4
    }

/// <summary>
/// Types of follow-up tasks (reminders, campaign steps etc).
/// </summary>
public enum FollowUpTaskType
    {
    MedicationReminder = 0,
    RevisitReminder = 1,
    CheckIn = 2,
    CampaignStep = 3,
    ManualAction = 4,
    AdminNotification = 5
    }

/// <summary>
/// Status of a follow-up task.
/// </summary>
public enum FollowUpTaskStatus
    {
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4,
    DeadLettered = 5
    }

