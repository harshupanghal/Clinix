using System;
using System.Collections.Generic;

namespace Clinix.Domain.Entities.Appointments;

/// <summary>
/// Admin-controlled mapping from symptom keywords to specialties or doctor suggestions.
/// Kept intentionally simple and extensible.
/// </summary>
public sealed class SymptomMapping
    {
    public long Id { get; init; } = default!;
    public string Keyword { get; init; } = string.Empty; // normalized keyword
    public string SuggestedSpecialty { get; init; } = string.Empty;
    public List<long> SuggestedDoctorIds { get; init; } = new();

    // Optional confidence weight (admin tunable)
    public int Weight { get; init; } = 50;
    }
