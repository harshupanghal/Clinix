// Clinix.Domain.FollowUp/Entities/Template.cs
using System;

namespace Clinix.Domain.Entities.FollowUps;

public class Template
    {
    public string Key { get; private set; } = string.Empty; // unique string key, e.g. "med_reminder_v1"
    public string Name { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty; // SMS body with tokens like {{PatientName}}
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public Template() { } // EF
    public Template(string key, string name, string body, string description = "")
        {
        Key = key ?? throw new ArgumentNullException(nameof(key));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Body = body ?? throw new ArgumentNullException(nameof(body));
        Description = description ?? string.Empty;
        }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
    }

