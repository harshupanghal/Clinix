// File: Clinix.Application.Mappers/SymptomMappingMapper.cs
using Clinix.Application.Dtos.Appointment;
using Clinix.Application.DTOs;
using Clinix.Domain.Entities.Appointments;

namespace Clinix.Application.Mappings;

public static class SymptomMappingMapper
    {
    public static SymptomMappingDto ToDto(this SymptomMapping e) =>
        new()
            {
            Id = e.Id,
            Keyword = e.Keyword,
            SuggestedSpecialty = e.SuggestedSpecialty,
            SuggestedDoctorIds = e.SuggestedDoctorIds?.ToList() ?? new List<long>(),
            Weight = e.Weight
            };

    public static SymptomMapping ToEntity(this SymptomMappingDto dto) =>
        new()
            {
            // If your entity uses init-only props adjust accordingly — here we assume mutable props or adjust ctor
            // If entity has readonly init-only, create a new entity via ctor or factory.
            Id = dto.Id,
            Keyword = dto.Keyword,
            SuggestedSpecialty = dto.SuggestedSpecialty,
            SuggestedDoctorIds = dto.SuggestedDoctorIds.ToList(),
            Weight = dto.Weight
            };
    }

