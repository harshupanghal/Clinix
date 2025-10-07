using Clinix.Application.DTOs;
using Clinix.Domain.Entities.ApplicationUsers;

namespace Clinix.Application.Mappers;
public static class StaffMappers
    {
    public static Staff CreateFrom(User user, CreateStaffRequest req)
        {
        return new Staff
            {
            User = user,
            Position = req.Position,
            Department = req.Department,
            ShiftInfo = req.ShiftInfo,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
            };
        }
    }

