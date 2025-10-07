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
            ClinicAddress = req.ClinicAddress,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
            };
        }
    }

