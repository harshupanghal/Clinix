using Clinix.Application.DTOs;
using Clinix.Domain.Entities.ApplicationUsers;

namespace Clinix.Application.Mappings;
public static class StaffMappers
    {
    public static Staff CreateFrom(User user, CreateStaffRequest req)
        {
        return new Staff
            {
            User = user,
            Position = req.Position,
            Department = req.Department,
            ShiftJson = req.ShiftJson,
            AssignedLocation = req.AssignedLocation,
            SupervisorName = req.SupervisorName,
            Notes = req.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
            };
        }
    }

