
namespace Clinix.Application.Dtos.Appointment;

public sealed class SymptomMappingDto
    {
    public long Id { get; set; }
    public string Keyword { get; set; } = string.Empty;
    public string SuggestedSpecialty { get; set; } = string.Empty;
    public List<long> SuggestedDoctorIds { get; set; } = new();
    public int Weight { get; set; } = 50;
    }
