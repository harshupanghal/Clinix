using Clinix.Domain.Entities.ApplicationUsers;
using Clinix.Application.Dtos.Patient;
using System;

namespace Clinix.Application.Mappings
    {
    public static class PatientMappers
        {
        public static Patient CreateFrom(User user, RegisterPatientRequest request)
            {
            return new Patient
                {
                UserId = user.Id,
                CreatedBy = user.CreatedBy,
                UpdatedBy = user.CreatedBy,
                RegisteredAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                // Optional fields at registration
                BloodGroup = null,
                Gender = null,
                DateOfBirth = null,
                EmergencyContactName = null,
                EmergencyContactNumber = null,
                KnownAllergies = null,
                ExistingConditions = null,
                MedicalRecordNumber = $"MRN-{Guid.NewGuid():N}".Substring(0, 12)
                };
            }
        }
    }
