using Clinix.Application.Dtos;
using Clinix.Domain.Entities.ApplicationUsers;

namespace Clinix.Application.Mappings;

public static class PatientMappers
    {
    public static Patient CreateFrom(User user, RegisterPatientRequest req)
        {
        return new Patient
            {
            User = user,
            DateOfBirth = req.DateOfBirth,
            Gender = req.Gender,
            BloodGroup = req.BloodGroup,
            EmergencyContact = req.EmergencyContact,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
            };
        }
    }

