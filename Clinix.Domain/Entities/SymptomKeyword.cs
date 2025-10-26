using System;

namespace Clinix.Domain.Entities;

public class SymptomKeyword
    {
    public long Id { get; set; }
    public string Keyword { get; set; } = null!;
    public string? SynonymsJson { get; set; }
    public string Specialty { get; set; } = null!;
    public int Weight { get; set; } = 10;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedBy { get; set; }
    }

