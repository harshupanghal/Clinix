using Clinix.Application.DTOs;
using Clinix.Domain.Entities.ApplicationUsers;

namespace Clinix.Application.Mappers;

public static class DoctorMappers
    {
    public static Doctor CreateFrom(User user, CreateDoctorRequest req)
        {
        return new Doctor
            {
            User = user,
            Degree = req.Degree,
            Specialty = req.Specialty,
            LicenseNumber = req.LicenseNumber,
            ExperienceYears = req.ExperienceYears,
            RoomNumber = req.RoomNumber,
            WorkHoursJson = req.WorkHoursJson,
            ConsultationFee = req.ConsultationFee,
            ExtensionNumber = req.ExtensionNumber,

            CreatedBy = "Admin",
            UpdatedBy = "Admin",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
            };
        }
    }

