using Clinix.Application.Interfaces.Functionalities;
using Clinix.Application.Interfaces.UserRepo;
using Clinix.Domain.Entities.ApplicationUsers;
using Clinix.Domain.Entities.Appointments;
using Clinix.Domain.Entities.Inventory;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging; // <-- Added for logging

namespace Clinix.Infrastructure.Data;

public static class DataSeeder
    {
    // New check: We can use the ILogger to log events.
    // Since this is a static method, we resolve the ILoggerFactory from the service provider.
    public static async Task SeedAsync(IServiceProvider sp, CancellationToken ct = default)
        {
        using var scope = sp.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        // 1. Resolve ILogger
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger(typeof(DataSeeder).FullName!);

        logger.LogInformation("🚀 Starting Clinix Database Seeding process.");

        var config = serviceProvider.GetRequiredService<IConfiguration>();
        var userRepo = serviceProvider.GetRequiredService<IUserRepository>();
        var doctorRepo = serviceProvider.GetRequiredService<IDoctorRepository>();
        var patientRepo = serviceProvider.GetRequiredService<IPatientRepository>();
        var staffRepo = serviceProvider.GetRequiredService<IStaffRepository>();
        var symptomRepo = serviceProvider.GetRequiredService<ISymptomMappingRepository>();
        var appointmentRepo = serviceProvider.GetRequiredService<IAppointmentRepository>();
        var inventoryRepo = serviceProvider.GetRequiredService<IInventoryService>();
        var uow = serviceProvider.GetRequiredService<IUnitOfWork>();

        var passwordHasher = new PasswordHasher<User>();

        // 2. Data Existence Check to prevent re-seeding errors
        var anyDoctorExists = await doctorRepo.CountAsync(ct) > 0; // Calls the implemented CountAsync
        var anyPatientExists = await patientRepo.CountAsync(ct) > 0; // Calls the implemented CountAsync

        if (anyDoctorExists || anyPatientExists)
            {
            logger.LogWarning("⚠️ Seed data already exists (Doctors or Patients found). Skipping full seed.");
            return;
            }

        logger.LogInformation("No primary seed data found. Proceeding with initial population.");

        await uow.BeginTransactionAsync(ct);
        try
            {
            // ---------------- ADMIN ----------------
            var adminEmail = config["SeedAdmin:Email"] ?? "admin@hms.local";
            var adminPassword = config["SeedAdmin:Password"] ?? "Admin@123#";

            // Check only the Admin here since we skipped the whole seed if other data exists
            if (await userRepo.GetByEmailAsync(adminEmail, ct) is null)
                {
                logger.LogInformation("Seeding Admin user: {Email}", adminEmail);
                var admin = new User
                    {
                    FullName = "Admin Kumar",
                    Email = adminEmail,
                    Phone = "9876054321",
                    Role = "Admin",
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                    };
                admin.PasswordHash = passwordHasher.HashPassword(admin, adminPassword);
                await userRepo.AddAsync(admin, ct);
                }

            // ---------------- DOCTORS (User Accounts) ----------------
            var doctorUsers = new List<User>
            {
                new() { FullName = "Dr. Ramesh Gupta", Email = "ramesh@hms.local", Phone = "9000000001", Role = "Doctor" },
                new() { FullName = "Dr. Priya Nair", Email = "priya@hms.local", Phone = "9000000002", Role = "Doctor" },
                new() { FullName = "Dr. Arjun Verma", Email = "arjun@hms.local", Phone = "9000000003", Role = "Doctor" }
            };

            logger.LogInformation("Seeding {Count} Doctor User accounts.", doctorUsers.Count);
            foreach (var u in doctorUsers)
                {
                u.PasswordHash = passwordHasher.HashPassword(u, "Doctor@123");
                await userRepo.AddAsync(u, ct);
                }

            await uow.CommitAsync(ct); // Commit User accounts first
            logger.LogInformation("Committed Doctor User accounts.");

            // Re-start transaction for Doctor profiles
            await uow.BeginTransactionAsync(ct);

            var doctors = new List<Doctor>
            {
                new() { UserId = doctorUsers[0].Id, Degree = "MBBS, MD", Specialty = "Cardiology", LicenseNumber = "DOC001", ExperienceYears = 12, RoomNumber = "A101", ConsultationFee = 500 },
                new() { UserId = doctorUsers[1].Id, Degree = "MBBS, MS", Specialty = "Orthopedics", LicenseNumber = "DOC002", ExperienceYears = 8, RoomNumber = "B202", ConsultationFee = 400 },
                new() { UserId = doctorUsers[2].Id, Degree = "MBBS, DGO", Specialty = "Gynecology", LicenseNumber = "DOC003", ExperienceYears = 10, RoomNumber = "C303", ConsultationFee = 450 }
            };

            logger.LogInformation("Seeding {Count} Doctor profiles.", doctors.Count);
            foreach (var d in doctors)
                await doctorRepo.AddAsync(d, ct);

            // ---------------- PATIENTS (User Accounts) ----------------
            var patientUsers = new List<User>
            {
                new() { FullName = "Ravi Mehta", Email = "ravi@hms.local", Phone = "9111111111", Role = "Patient" },
                new() { FullName = "Neha Sharma", Email = "neha@hms.local", Phone = "9222222222", Role = "Patient" },
                new() { FullName = "Suresh Kumar", Email = "suresh@hms.local", Phone = "9333333333", Role = "Patient" }
            };

            logger.LogInformation("Seeding {Count} Patient User accounts.", patientUsers.Count);
            foreach (var u in patientUsers)
                {
                u.PasswordHash = passwordHasher.HashPassword(u, "Patient@123");
                await userRepo.AddAsync(u, ct);
                }

            await uow.CommitAsync(ct); // Commit User accounts
            logger.LogInformation("Committed Patient User accounts.");

            // Re-start transaction for Patient profiles
            await uow.BeginTransactionAsync(ct);

            var patients = new List<Patient>
            {
                new() { UserId = patientUsers[0].Id, Gender = "Male", BloodGroup = "B+", EmergencyContactName = "Sunita", EmergencyContactNumber = "9800000001" },
                new() { UserId = patientUsers[1].Id, Gender = "Female", BloodGroup = "O+", EmergencyContactName = "Raj", EmergencyContactNumber = "9800000002" },
                new() { UserId = patientUsers[2].Id, Gender = "Male", BloodGroup = "A-", EmergencyContactName = "Manish", EmergencyContactNumber = "9800000003" }
            };

            logger.LogInformation("Seeding {Count} Patient profiles.", patients.Count);
            foreach (var p in patients)
                await patientRepo.AddAsync(p, ct);

            // ---------------- STAFF ----------------
            var staffUsers = new List<User>
            {
                new() { FullName = "Pooja Joshi", Email = "pooja@hms.local", Phone = "9444444444", Role = "Staff" },
                new() { FullName = "Vikram Singh", Email = "vikram@hms.local", Phone = "9555555555", Role = "Staff" }
            };

            logger.LogInformation("Seeding {Count} Staff User accounts.", staffUsers.Count);
            foreach (var u in staffUsers)
                {
                u.PasswordHash = passwordHasher.HashPassword(u, "Staff@123");
                await userRepo.AddAsync(u, ct);
                }

            await uow.CommitAsync(ct); // Commit User accounts
            logger.LogInformation("Committed Staff User accounts.");

            // Re-start transaction for Staff profiles and other data
            await uow.BeginTransactionAsync(ct);

            var staffMembers = new List<Staff>
            {
                new() { UserId = staffUsers[0].Id, Position = "Receptionist", Department = "Front Desk" },
                new() { UserId = staffUsers[1].Id, Position = "Pharmacist", Department = "Pharmacy" }
            };

            logger.LogInformation("Seeding {Count} Staff profiles.", staffMembers.Count);
            foreach (var s in staffMembers)
                await staffRepo.AddAsync(s, ct);

            // ---------------- INVENTORY ----------------
            var inventoryItems = new List<InventoryItem>
            {
                new() { Name = "Paracetamol", Type = "Medicine", Category = "Analgesic", Unit = "Tablets", MinStock = 50, CurrentStock = 200 },
                new() { Name = "Syringe 5ml", Type = "Consumable", Category = "Injection", Unit = "Pieces", MinStock = 100, CurrentStock = 500 },
                new() { Name = "Stethoscope", Type = "Equipment", Category = "Diagnostic", Unit = "Pieces", MinStock = 5, CurrentStock = 15 },
                new() { Name = "Amoxicillin", Type = "Medicine", Category = "Antibiotic", Unit = "Capsules", MinStock = 40, CurrentStock = 120 },
                new() { Name = "Glucose Bottle", Type = "Consumable", Category = "IV Fluid", Unit = "Bottles", MinStock = 30, CurrentStock = 80 }
            };

            logger.LogInformation("Seeding {Count} Inventory items.", inventoryItems.Count);
            foreach (var item in inventoryItems)
                await inventoryRepo.AddItemAsync(item);

            // ---------------- SYMPTOM MAPPINGS ----------------
            var mappings = new List<SymptomMapping>
            {
                new() { Keyword = "fever", SuggestedSpecialty = "General Medicine", SuggestedDoctorIds = new() { doctors[0].DoctorId }, Weight = 80 },
                new() { Keyword = "back pain", SuggestedSpecialty = "Orthopedics", SuggestedDoctorIds = new() { doctors[1].DoctorId }, Weight = 75 },
                new() { Keyword = "pregnancy", SuggestedSpecialty = "Gynecology", SuggestedDoctorIds = new() { doctors[2].DoctorId }, Weight = 90 }
            };

            logger.LogInformation("Seeding {Count} Symptom Mappings.", mappings.Count);
            foreach (var map in mappings)
                await symptomRepo.AddOrUpdateAsync(map);

            // ---------------- APPOINTMENTS ----------------
            var now = DateTimeOffset.UtcNow;
            var appointments = new List<Appointment>
            {
                new(doctors[0].DoctorId, patients[0].PatientId, now.AddDays(1).AddHours(9), now.AddDays(1).AddHours(9.30), "Routine check-up"),
                new(doctors[1].DoctorId, patients[1].PatientId, now.AddDays(2).AddHours(10), now.AddDays(2).AddHours(10.30), "Knee pain"),
                new(doctors[2].DoctorId, patients[2].PatientId, now.AddDays(3).AddHours(11), now.AddDays(3).AddHours(11.45), "Pregnancy follow-up")
            };

            logger.LogInformation("Seeding {Count} Appointment records.", appointments.Count);
            foreach (var appt in appointments)
                await appointmentRepo.AddAsync(appt);

            await uow.CommitAsync(ct);
            logger.LogInformation("✅ Database seeding completed successfully.");
            }
        catch (Exception ex)
            {
            await uow.RollbackAsync(ct);
            // Log the exception details at Error level
            logger.LogError(ex, "❌ Database seeding failed. Rolling back transaction.");
            throw;
            }
        }
    }