namespace Clinix.Domain.Entities.System;

public class SeedStatus
    {
    public int Id { get; set; }
    public string SeedName { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public DateTime ExecutedAt { get; set; }
    public bool IsCompleted { get; set; }
    public string ExecutedBy { get; set; } = "System";
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    }
