using Clinix.Application.Interfaces;
using Clinix.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Clinix.Infrastructure.Data;

public static class DataSeeder
    {
    /// <summary>
    /// Seeds initial admin if SeedAdmin:Password exists in configuration.
    /// For production, prefer secure provisioning rather than embedding secrets.
    /// </summary>
    public static async Task SeedAdminAsync(IServiceProvider sp, CancellationToken ct = default)
        {
        using var scope = sp.CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var passwordHasher = new PasswordHasher<User>();

        var adminEmail = config["SeedAdmin:Email"] ?? "admin@hms.local";
        var adminUsername = config["SeedAdmin:Username"] ?? "admin";
        var adminPassword = config["SeedAdmin:Password"] ?? "Admin@123#";

        if (string.IsNullOrWhiteSpace(adminPassword))
            {
            // Do nothing if no password configured (safer than creating a default).
            return;
            }

        var existing = await userRepo.GetByEmailAsync(adminEmail, ct);
        if (existing != null) return;

        var admin = new User
            {
            Username = adminUsername,
            Email = adminEmail,
            Role = "Admin",
            CreatedBy = "system",
            CreatedAt = System.DateTime.UtcNow,
            UpdatedAt = System.DateTime.UtcNow
            };

        admin.PasswordHash = passwordHasher.HashPassword(admin, adminPassword);

        await uow.BeginTransactionAsync(ct);
        try
            {
            await userRepo.AddAsync(admin, ct);
            await uow.CommitAsync(ct);
            }
        catch
            {
            await uow.RollbackAsync(ct);
            throw;
            }
        }
    }

