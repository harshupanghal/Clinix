using Clinix.Domain.Entities;

namespace Clinix.Application.Mappings;

public static class UserMappers
    {
    public static User CreateForRole(string username, string email, string role, string createdBy)
        {
        return new User
            {
            Username = username.Trim(),
            Email = email.Trim(),
            Role = role,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
            };
        }
    }

