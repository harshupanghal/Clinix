//using System;
//using System.Threading.Tasks;
//using Clinix.Domain.Entities;
//using Clinix.Infrastructure.Persistence;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;

//namespace Clinix.Infrastructure.Seed;

///// <summary>
///// Provides initial seed data for the Clinix system.
///// Ensures that a default Director account exists for administration.
///// </summary>
//public static class SeedData
//    {
//    private const string DefaultDirectorEmail = "director@clinix.local";
//    private const string DefaultDirectorUsername = "director";

//    /// <summary>
//    /// Ensures that a default Director user exists in the database.
//    /// If none exists, creates one with the provided initial password.
//    /// </summary>
//    /// <param name="serviceProvider">The application service provider.</param>
//    /// <param name="initialPassword">The password for the seeded Director account.</param>
//    public static async Task EnsureSeedDirectorAsync(IServiceProvider serviceProvider, string initialPassword)
//        {
//        if (string.IsNullOrWhiteSpace(initialPassword))
//            throw new ArgumentException("Initial password cannot be null or empty.", nameof(initialPassword));

//        using var scope = serviceProvider.CreateScope();
//        var dbContext = scope.ServiceProvider.GetRequiredService<ClinixDbContext>();
//        var logger = scope.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger("SeedData");

//        // Normalize email for lookup
//        var normalizedEmail = DefaultDirectorEmail.ToLowerInvariant();

//        var existing = await dbContext.Users
//            .AsNoTracking()
//            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

//        if (existing != null)
//            {
//            logger?.LogInformation("Director account already exists: {Email}", DefaultDirectorEmail);
//            return;
//            }

//        var director = new User
//            {
//            UserName = DefaultDirectorUsername,
//            Email = DefaultDirectorEmail,
//            FirstName = "Default",
//            LastName = "Director",
//            Role = Role.Director
//            };

//        var hasher = new PasswordHasher<User>();
//        director.PasswordHash = hasher.HashPassword(director, initialPassword);

//        try
//            {
//            await dbContext.Users.AddAsync(director);
//            await dbContext.SaveChangesAsync();
//            logger?.LogInformation("Default Director account created successfully with email {Email}", DefaultDirectorEmail);
//            }
//        catch (DbUpdateException ex)
//            {
//            logger?.LogError(ex, "Failed to seed default Director account.");
//            throw new InvalidOperationException("Seeding Director account failed.", ex);
//            }
//        }
//    }
