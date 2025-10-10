using System.ComponentModel.DataAnnotations;
using Clinix.Domain.Entities.ApplicationUsers;

namespace Clinix.Application.Mappings;
public static class UserMappers
    {
    public static User CreateForRole(string fullName, string email, string Phone, string role, string createdBy)
        {
        return new User
            {
            FullName = fullName.Trim(),
            Email = email.Trim(),
            Phone = Phone.Trim(),
            Role = role,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
            };
        }
    }

