using System;

namespace Clinix.Domain.Entities;

public class SymptomKeyword
    {
    public long Id { get; set; }

    // Normalized keyword, e.g. "chest pain", lowercased on write
    public string Keyword { get; set; } = null!;

    // Optional synonyms stored as JSON array (["angina","retrosternal pain"])
    public string? SynonymsJson { get; set; }

    // The specialty name (string to keep it simple and compatible with your Doctor.Specialty)
    public string Specialty { get; set; } = null!;

    // Weight to indicate importance / relevance
    public int Weight { get; set; } = 10;

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedBy { get; set; }
    }

