namespace Clinix.Domain.Entities;


public class SymptomSpecialtyMap
    {
    public int Id { get; set; }
    public string Keyword { get; set; } = string.Empty; // e.g. "acne"
    public string Specialty { get; set; } = string.Empty; // e.g. "Dermatology"
    }