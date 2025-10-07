using Clinix.Domain.Entities.ApplicationUsers;
using Clinix.Domain.Entities.Appointments;
using Clinix.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clinix.Infrastructure.Data;

public static class DbSeeder
    {
    public static async Task SeedAsync(ClinixDbContext db, CancellationToken ct)
        {
        if (!await db.Doctors.AnyAsync(ct))
            {
            var docs = new List<Doctor>
            {
                new() { Name = "Dr. A Sharma", Email = "dr.sharma@clinic.local", Specialty = "Dermatology" },
                new() { Name = "Dr. R Patel", Email = "dr.patel@clinic.local", Specialty = "General Physician" },
                new() { Name = "Dr. S Gupta", Email = "dr.gupta@clinic.local", Specialty = "Gastroenterology" }
            };
            db.Doctors.AddRange(docs);
            }

        if (!await db.SymptomSpecialtyMaps.AnyAsync(ct))
            {
            var maps = new List<SymptomSpecialtyMap>
            {
                new() { Keyword = "acne", Specialty = "Dermatology" },
                new() { Keyword = "rash", Specialty = "Dermatology" },
                new() { Keyword = "fever", Specialty = "General Physician" },
                new() { Keyword = "cough", Specialty = "Pulmonology" },
                new() { Keyword = "stomach", Specialty = "Gastroenterology" }
            };
            db.SymptomSpecialtyMaps.AddRange(maps);
            }

        await db.SaveChangesAsync(ct);
        }
    }
