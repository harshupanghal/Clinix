using System;
using System.Linq;
using Clinix.Application.Dtos;
using Clinix.Domain.Entities.ApplicationUsers;

namespace Clinix.Application.Mappings;

public static class DoctorMappers
    {
    public static Doctor CreateFrom(User user, CreateDoctorRequest req)
        {
        var doctor = new Doctor
            {
            User = user,
            Degree = req.Degree,
            Specialty = req.Specialty,
            LicenseNumber = req.LicenseNumber,
            ExperienceYears = req.ExperienceYears,
            RoomNumber = req.RoomNumber,
            ConsultationFee = req.ConsultationFee,
            ExtensionNumber = req.ExtensionNumber,
            Notes = req.Notes,
            CreatedBy = "Admin",
            UpdatedBy = "Admin",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
            };

        if (req.Schedules?.Any() == true)
            {
            doctor.Schedules = req.Schedules.Select(s => new DoctorSchedule
                {
                DayOfWeek = s.DayOfWeek,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                IsAvailable = s.IsAvailable
                }).ToList();
            }

        return doctor;
        }
    }
