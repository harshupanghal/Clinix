// Infrastructure/Data/DataSeeder.cs
using Clinix.Application.Interfaces.Functionalities;
using Clinix.Application.Interfaces.UserRepo;
using Clinix.Domain.Entities.ApplicationUsers;
using Clinix.Domain.Entities.Inventory;
using Clinix.Domain.Entities;
using Clinix.Domain.Entities.System;
using Clinix.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Clinix.Infrastructure.Data;

public static class DataSeeder
    {
    private const string SEED_NAME = "ClinixInitialData";
    private const string SEED_VERSION = "1.0.0";
    private const int MAX_RETRIES = 3;

    public static async Task SeedAsync(IServiceProvider sp, CancellationToken ct = default)
        {
        using var scope = sp.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger(typeof(DataSeeder).FullName!);

        logger.LogInformation("🔍 Checking seed status for '{SeedName}' v{Version}", SEED_NAME, SEED_VERSION);

        // ✅ STEP 1: Get SeedStatus repository
        var seedStatusRepo = serviceProvider.GetRequiredService<ISeedStatusRepository>();

        // ✅ STEP 2: Check if seed already completed
        if (await seedStatusRepo.IsSeedCompletedAsync(SEED_NAME, SEED_VERSION, ct))
            {
            logger.LogInformation("✅ Seed '{SeedName}' v{Version} already completed. Skipping.",
                SEED_NAME, SEED_VERSION);
            return;
            }

        // ✅ STEP 3: Check for failed previous attempts
        var existingSeedStatus = await seedStatusRepo.GetBySeedNameAsync(SEED_NAME, SEED_VERSION, ct);
        if (existingSeedStatus != null && existingSeedStatus.RetryCount >= MAX_RETRIES)
            {
            logger.LogError(
                "❌ Seed '{SeedName}' v{Version} failed {Count} times. Manual intervention required. Error: {Error}",
                SEED_NAME, SEED_VERSION, existingSeedStatus.RetryCount, existingSeedStatus.ErrorMessage);
            return;
            }

        var config = serviceProvider.GetRequiredService<IConfiguration>();
        var userRepo = serviceProvider.GetRequiredService<IUserRepository>();
        var doctorRepo = serviceProvider.GetRequiredService<IDoctorRepository>();
        var patientRepo = serviceProvider.GetRequiredService<IPatientRepository>();
        var staffRepo = serviceProvider.GetRequiredService<IStaffRepository>();
        var inventoryRepo = serviceProvider.GetRequiredService<IInventoryService>();
        var providerRepo = serviceProvider.GetRequiredService<IProviderRepository>();
        var uow = serviceProvider.GetRequiredService<IUnitOfWork>();

        // ✅ STEP 4: Create or update seed status
        var seedStatus = existingSeedStatus ?? new SeedStatus
            {
            SeedName = SEED_NAME,
            Version = SEED_VERSION,
            ExecutedAt = DateTime.UtcNow,
            IsCompleted = false,
            RetryCount = 0,
            ExecutedBy = "System"
            };

        seedStatus.RetryCount++;
        seedStatus.ExecutedAt = DateTime.UtcNow;

        if (existingSeedStatus == null)
            await seedStatusRepo.AddAsync(seedStatus, ct);
        else
            await seedStatusRepo.UpdateAsync(seedStatus, ct);

        logger.LogInformation("🚀 Starting seed attempt {Retry} of {Max}",
            seedStatus.RetryCount, MAX_RETRIES);

        var passwordHasher = new PasswordHasher<User>();

        await uow.BeginTransactionAsync(ct);
        try
            {
            // ✅ ADMIN SEEDING (Idempotent)
            var adminEmail = config["SeedAdmin:Email"] ?? "admin@hms.local";
            var adminPassword = config["SeedAdmin:Password"] ?? "Admin@123#";

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
            else
                {
                logger.LogInformation("Admin user already exists. Skipping.");
                }

            // ✅ DOCTOR USER ACCOUNTS (Idempotent)
            var doctorUserData = new[]
            {
                ("Dr. Ramesh Gupta", "ramesh@hms.local", "9000000001"),
                ("Dr. Priya Nair", "priya@hms.local", "9000000002"),
                ("Dr. Arjun Verma", "arjun@hms.local", "9000000003")
            };

            var doctorUsers = new List<User>();
            foreach (var (name, email, phone) in doctorUserData)
                {
                var existing = await userRepo.GetByEmailAsync(email, ct);
                if (existing != null)
                    {
                    logger.LogInformation("Doctor user {Email} already exists. Skipping.", email);
                    doctorUsers.Add(existing);
                    }
                else
                    {
                    logger.LogInformation("Seeding Doctor user: {Email}", email);
                    var user = new User
                        {
                        FullName = name,
                        Email = email,
                        Phone = phone,
                        Role = "Doctor",
                        CreatedBy = "system",
                        CreatedAt = DateTime.UtcNow
                        };
                    user.PasswordHash = passwordHasher.HashPassword(user, "Doctor@123");
                    await userRepo.AddAsync(user, ct);
                    doctorUsers.Add(user);
                    }
                }

            await uow.CommitAsync(ct);
            logger.LogInformation("Committed Doctor User accounts.");

            await uow.BeginTransactionAsync(ct);

            // ✅ DOCTOR PROFILES (Idempotent - check by UserId)
            var doctorProfileData = new[]
            {
                (doctorUsers[0].Id, "MBBS, MD", "Cardiology", "DOC001", 12, "A101", 500m),
                (doctorUsers[1].Id, "MBBS, MS", "Orthopedics", "DOC002", 8, "B202", 400m),
                (doctorUsers[2].Id, "MBBS, DGO", "Gynecology", "DOC003", 10, "C303", 450m)
            };

            var doctors = new List<Doctor>();
            foreach (var (userId, degree, specialty, license, exp, room, fee) in doctorProfileData)
                {
                // Check if doctor profile exists for this user
                var existing = await doctorRepo.GetByUserIdAsync(userId, ct);
                if (existing != null)
                    {
                    logger.LogInformation("Doctor profile for UserId {UserId} already exists. Skipping.", userId);
                    doctors.Add(existing);
                    }
                else
                    {
                    logger.LogInformation("Seeding Doctor profile for UserId {UserId}", userId);
                    var doctor = new Doctor
                        {
                        UserId = userId,
                        Degree = degree,
                        Specialty = specialty,
                        LicenseNumber = license,
                        ExperienceYears = exp,
                        RoomNumber = room,
                        ConsultationFee = fee
                        };
                    await doctorRepo.AddAsync(doctor, ct);
                    doctors.Add(doctor);
                    }
                }

            await uow.CommitAsync(ct);
            logger.LogInformation("Committed Doctor profiles.");

            await uow.BeginTransactionAsync(ct);

            // ✅ DOCTOR SCHEDULES (Idempotent - check existing)
            logger.LogInformation("Seeding Doctor Schedules...");

            var scheduleData = new[]
            {
                (doctors[0].DoctorId, new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                    DayOfWeek.Thursday, DayOfWeek.Friday }, new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0)),
                (doctors[0].DoctorId, new[] { DayOfWeek.Saturday }, new TimeSpan(9, 0, 0), new TimeSpan(13, 0, 0)),
                (doctors[1].DoctorId, new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                    DayOfWeek.Thursday, DayOfWeek.Friday }, new TimeSpan(10, 0, 0), new TimeSpan(18, 0, 0)),
                (doctors[2].DoctorId, new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                    DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday },
                    new TimeSpan(9, 0, 0), new TimeSpan(16, 0, 0))
            };

            var scheduleRepo = serviceProvider.GetRequiredService<IDoctorScheduleRepository>();

            foreach (var (doctorId, days, start, end) in scheduleData)
                {
                foreach (var day in days)
                    {
                    var existing = await scheduleRepo.GetByDoctorAndDayAsync(doctorId, day, ct);
                    if (existing == null)
                        {
                        var schedule = new DoctorSchedule
                            {
                            DoctorId = doctorId,
                            DayOfWeek = day,
                            StartTime = start,
                            EndTime = end,
                            IsAvailable = true
                            };
                        await scheduleRepo.AddRangeAsync(new List<DoctorSchedule> { schedule }, ct);
                        }
                    }
                }

            // Add unavailable days
            foreach (var doctor in doctors)
                {
                var allDays = Enum.GetValues<DayOfWeek>();
                foreach (var day in allDays)
                    {
                    var existing = await scheduleRepo.GetByDoctorAndDayAsync(doctor.DoctorId, day, ct);
                    if (existing == null)
                        {
                        var schedule = new DoctorSchedule
                            {
                            DoctorId = doctor.DoctorId,
                            DayOfWeek = day,
                            StartTime = TimeSpan.Zero,
                            EndTime = TimeSpan.Zero,
                            IsAvailable = false
                            };
                        await scheduleRepo.AddRangeAsync(new List<DoctorSchedule> { schedule }, ct);
                        }
                    }
                }

            logger.LogInformation("Doctor schedules seeded successfully.");

            // ✅ PROVIDERS (Idempotent - check existing)
            var existingProviders = await providerRepo.SearchAsync(Array.Empty<string>(), ct);

            if (existingProviders.Count == 0)
                {
                logger.LogInformation("No providers found. Seeding 15 providers...");

                var providers = new List<Provider>
                {
                    new("Dr. Ramesh Gupta", "Cardiology",
                        "chest pain,heart attack,palpitations,angina,cardiac arrest,heart disease,hypertension",
                        DateTime.Today.AddHours(9), DateTime.Today.AddHours(17)),

                    new("Dr. Priya Nair", "Orthopedics",
                        "bone pain,fracture,joint pain,arthritis,back pain,knee pain,sprain",
                        DateTime.Today.AddHours(10), DateTime.Today.AddHours(18)),

                    new("Dr. Arjun Verma", "Gynecology",
                        "pregnancy,menstrual problems,pcos,periods,cramps,ovarian cyst",
                        DateTime.Today.AddHours(9), DateTime.Today.AddHours(16)),

                    new("Dr. Anjali Reddy", "Dermatology",
                        "skin rash,acne,eczema,psoriasis,itching,allergy,skin infection",
                        DateTime.Today.AddHours(10), DateTime.Today.AddHours(17)),

                    new("Dr. Vikram Menon", "Neurology",
                        "headache,migraine,seizure,stroke,paralysis,tremor,parkinson",
                        DateTime.Today.AddHours(9), DateTime.Today.AddHours(18)),

                    new("Dr. Sunil Iyer", "Gastroenterology",
                        "stomach pain,acidity,gastritis,ulcer,diarrhea,constipation,ibs",
                        DateTime.Today.AddHours(8), DateTime.Today.AddHours(16)),

                    new("Dr. Meera Joshi", "Pediatrics",
                        "child fever,vaccination,newborn,baby cold,infant care",
                        DateTime.Today.AddHours(9), DateTime.Today.AddHours(17)),

                    new("Dr. Rajesh Kumar", "ENT",
                        "ear pain,sinus,throat infection,tonsillitis,hearing loss",
                        DateTime.Today.AddHours(10), DateTime.Today.AddHours(18)),

                    new("Dr. Kavita Shah", "Ophthalmology",
                        "eye pain,blurred vision,cataract,glaucoma,pink eye",
                        DateTime.Today.AddHours(9), DateTime.Today.AddHours(16)),

                    new("Dr. Ashok Pillai", "Pulmonology",
                        "cough,asthma,breathlessness,copd,pneumonia,lung infection",
                        DateTime.Today.AddHours(9), DateTime.Today.AddHours(17)),

                    new("Dr. Lakshmi Rao", "Endocrinology",
                        "diabetes,thyroid,hormonal imbalance,pcos,weight gain",
                        DateTime.Today.AddHours(10), DateTime.Today.AddHours(18)),

                    new("Dr. Karan Desai", "Urology",
                        "kidney stone,uti,urinary infection,prostate,bladder pain",
                        DateTime.Today.AddHours(9), DateTime.Today.AddHours(17)),

                    new("Dr. Ritu Bansal", "Psychiatry",
                        "depression,anxiety,stress,panic attack,bipolar,ocd",
                        DateTime.Today.AddHours(11), DateTime.Today.AddHours(19)),

                    new("Dr. Anil Shetty", "General Medicine",
                        "fever,fatigue,weakness,cold,flu,body pain,general checkup",
                        DateTime.Today.AddHours(8), DateTime.Today.AddHours(20)),

                    new("Dr. Pooja Kapoor", "Rheumatology",
                        "rheumatoid arthritis,lupus,autoimmune disease,joint inflammation",
                        DateTime.Today.AddHours(10), DateTime.Today.AddHours(17))
                };

                foreach (var p in providers)
                    await providerRepo.AddAsync(p, ct);

                logger.LogInformation("✅ Seeded {Count} providers.", providers.Count);

                await uow.CommitAsync(ct);
                await uow.BeginTransactionAsync(ct);

                // Link doctors to providers
                var refreshedProviders = await providerRepo.SearchAsync(Array.Empty<string>(), ct);
                doctors[0].ProviderId = refreshedProviders.First(p => p.Name == "Dr. Ramesh Gupta").Id;
                doctors[1].ProviderId = refreshedProviders.First(p => p.Name == "Dr. Priya Nair").Id;
                doctors[2].ProviderId = refreshedProviders.First(p => p.Name == "Dr. Arjun Verma").Id;

                foreach (var doctor in doctors)
                    await doctorRepo.UpdateAsync(doctor, ct);

                logger.LogInformation("✅ Linked doctors to providers.");
                }
            else
                {
                logger.LogInformation("✅ {Count} providers already exist. Skipping provider seed.",
                    existingProviders.Count);
                }

            // ✅ PATIENTS (Idempotent)
            if (await patientRepo.CountAsync(ct) == 0)
                {
                logger.LogInformation("Seeding test patients...");

                var patientUserData = new[]
                {
                    ("Ravi Mehta", "ravi@hms.local", "9111111111"),
                    ("Neha Sharma", "neha@hms.local", "9222222222"),
                    ("Suresh Kumar", "suresh@hms.local", "9333333333")
                };

                var patientUsers = new List<User>();
                foreach (var (name, email, phone) in patientUserData)
                    {
                    var user = new User
                        {
                        FullName = name,
                        Email = email,
                        Phone = phone,
                        Role = "Patient",
                        CreatedBy = "system",
                        CreatedAt = DateTime.UtcNow
                        };
                    user.PasswordHash = passwordHasher.HashPassword(user, "Patient@123");
                    await userRepo.AddAsync(user, ct);
                    patientUsers.Add(user);
                    }

                await uow.CommitAsync(ct);
                await uow.BeginTransactionAsync(ct);

                var patientData = new[]
                {
                    (patientUsers[0].Id, "Male", "B+", "Sunita", "9800000001"),
                    (patientUsers[1].Id, "Female", "O+", "Raj", "9800000002"),
                    (patientUsers[2].Id, "Male", "A-", "Manish", "9800000003")
                };

                foreach (var (userId, gender, blood, contact, phone) in patientData)
                    {
                    var patient = new Patient
                        {
                        UserId = userId,
                        Gender = gender,
                        BloodGroup = blood,
                        EmergencyContactName = contact,
                        EmergencyContactNumber = phone
                        };
                    await patientRepo.AddAsync(patient, ct);
                    }

                logger.LogInformation("✅ Seeded test patients.");
                }
            else
                {
                logger.LogInformation("✅ Patients already exist. Skipping.");
                }

            // ✅ STAFF (Idempotent)
            var staffCount = await userRepo.GetByEmailAsync("pooja@hms.local", ct) != null ? 1 : 0;

            if (staffCount == 0)
                {
                logger.LogInformation("Seeding staff members...");

                var staffUserData = new[]
                {
                    ("Pooja Joshi", "pooja@hms.local", "9444444444", "Receptionist", "Front Desk"),
                    ("Vikram Singh", "vikram@hms.local", "9555555555", "Pharmacist", "Pharmacy")
                };

                foreach (var (name, email, phone, position, dept) in staffUserData)
                    {
                    var user = new User
                        {
                        FullName = name,
                        Email = email,
                        Phone = phone,
                        Role = "Staff",
                        CreatedBy = "system",
                        CreatedAt = DateTime.UtcNow
                        };
                    user.PasswordHash = passwordHasher.HashPassword(user, "Staff@123");
                    await userRepo.AddAsync(user, ct);

                    await uow.CommitAsync(ct);
                    await uow.BeginTransactionAsync(ct);

                    var staff = new Staff
                        {
                        UserId = user.Id,
                        Position = position,
                        Department = dept
                        };
                    await staffRepo.AddAsync(staff, ct);
                    }

                logger.LogInformation("✅ Seeded staff members.");
                }
            else
                {
                logger.LogInformation("✅ Staff already exist. Skipping.");
                }

            // ✅ INVENTORY (Idempotent - basic check)
            logger.LogInformation("Seeding inventory items...");
            var inventoryItems = new[]
            {
                ("Paracetamol", "Medicine", "Analgesic", "Tablets", 50, 200),
                ("Syringe 5ml", "Consumable", "Injection", "Pieces", 100, 500),
                ("Stethoscope", "Equipment", "Diagnostic", "Pieces", 5, 15),
                ("Amoxicillin", "Medicine", "Antibiotic", "Capsules", 40, 120),
                ("Glucose Bottle", "Consumable", "IV Fluid", "Bottles", 30, 80)
            };

            foreach (var (name, type, category, unit, min, current) in inventoryItems)
                {
                var item = new InventoryItem
                    {
                    Name = name,
                    Type = type,
                    Category = category,
                    Unit = unit,
                    MinStock = min,
                    CurrentStock = current
                    };
                await inventoryRepo.AddItemAsync(item);
                }

            logger.LogInformation("✅ Seeded inventory items.");

            // ✅ MARK SEED AS COMPLETED
            seedStatus.IsCompleted = true;
            seedStatus.ErrorMessage = null;
            await seedStatusRepo.UpdateAsync(seedStatus, ct);

            await uow.CommitAsync(ct);
            logger.LogInformation("✅ Seed '{SeedName}' v{Version} completed successfully on attempt {Retry}.",
                SEED_NAME, SEED_VERSION, seedStatus.RetryCount);
            }
        catch (Exception ex)
            {
            await uow.RollbackAsync(ct);

            seedStatus.ErrorMessage = ex.Message.Length > 1000
                ? ex.Message.Substring(0, 1000)
                : ex.Message;
            await seedStatusRepo.UpdateAsync(seedStatus, ct);

            logger.LogError(ex,
                "❌ Seed '{SeedName}' v{Version} failed on attempt {Retry}/{Max}. Error: {Error}",
                SEED_NAME, SEED_VERSION, seedStatus.RetryCount, MAX_RETRIES, ex.Message);

            throw;
            }
        }
    }
