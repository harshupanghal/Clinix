using Clinix.Application.Interfaces.Functionalities;
using Clinix.Application.Interfaces.UserRepo;
using Clinix.Domain.Entities.ApplicationUsers;
using Clinix.Domain.Entities.Inventory;
using Clinix.Domain.Entities;
using Clinix.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Clinix.Infrastructure.Data;

public static class DataSeeder
    {
    public static async Task SeedAsync(IServiceProvider sp, CancellationToken ct = default)
        {
        using var scope = sp.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger(typeof(DataSeeder).FullName!);

        logger.LogInformation("🚀 Starting Clinix Database Seeding process.");

        var config = serviceProvider.GetRequiredService<IConfiguration>();
        var userRepo = serviceProvider.GetRequiredService<IUserRepository>();
        var doctorRepo = serviceProvider.GetRequiredService<IDoctorRepository>();
        var patientRepo = serviceProvider.GetRequiredService<IPatientRepository>();
        var staffRepo = serviceProvider.GetRequiredService<IStaffRepository>();
        var inventoryRepo = serviceProvider.GetRequiredService<IInventoryService>();
        var providerRepo = serviceProvider.GetRequiredService<IProviderRepository>();
        var uow = serviceProvider.GetRequiredService<IUnitOfWork>();

        var passwordHasher = new PasswordHasher<User>();

        // Data existence check
        var anyDoctorExists = await doctorRepo.CountAsync(ct) > 0;
        var anyPatientExists = await patientRepo.CountAsync(ct) > 0;

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

            await uow.CommitAsync(ct);
            logger.LogInformation("Committed Doctor User accounts.");

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

            await uow.CommitAsync(ct);
            logger.LogInformation("Committed Doctor profiles.");

            await uow.BeginTransactionAsync(ct);

            // ---------------- DOCTOR SCHEDULES ----------------
            logger.LogInformation("Seeding Doctor Schedules...");

            // Dr. Ramesh Gupta (Cardiology) - Monday to Saturday, 9 AM - 5 PM
            var rameshSchedules = new List<DoctorSchedule>
            {
                new() { DoctorId = doctors[0].DoctorId, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(17, 0, 0), IsAvailable = true },
                new() { DoctorId = doctors[0].DoctorId, DayOfWeek = DayOfWeek.Tuesday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(17, 0, 0), IsAvailable = true },
                new() { DoctorId = doctors[0].DoctorId, DayOfWeek = DayOfWeek.Wednesday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(17, 0, 0), IsAvailable = true },
                new() { DoctorId = doctors[0].DoctorId, DayOfWeek = DayOfWeek.Thursday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(17, 0, 0), IsAvailable = true },
                new() { DoctorId = doctors[0].DoctorId, DayOfWeek = DayOfWeek.Friday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(17, 0, 0), IsAvailable = true },
                new() { DoctorId = doctors[0].DoctorId, DayOfWeek = DayOfWeek.Saturday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(13, 0, 0), IsAvailable = true },
                new() { DoctorId = doctors[0].DoctorId, DayOfWeek = DayOfWeek.Sunday, StartTime = new TimeSpan(0, 0, 0), EndTime = new TimeSpan(0, 0, 0), IsAvailable = false }
            };

            // Dr. Priya Nair (Orthopedics) - Monday to Friday, 10 AM - 6 PM
            var priyaSchedules = new List<DoctorSchedule>
            {
                new() { DoctorId = doctors[1].DoctorId, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(18, 0, 0), IsAvailable = true },
                new() { DoctorId = doctors[1].DoctorId, DayOfWeek = DayOfWeek.Tuesday, StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(18, 0, 0), IsAvailable = true },
                new() { DoctorId = doctors[1].DoctorId, DayOfWeek = DayOfWeek.Wednesday, StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(18, 0, 0), IsAvailable = true },
                new() { DoctorId = doctors[1].DoctorId, DayOfWeek = DayOfWeek.Thursday, StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(18, 0, 0), IsAvailable = true },
                new() { DoctorId = doctors[1].DoctorId, DayOfWeek = DayOfWeek.Friday, StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(18, 0, 0), IsAvailable = true },
                new() { DoctorId = doctors[1].DoctorId, DayOfWeek = DayOfWeek.Saturday, StartTime = new TimeSpan(0, 0, 0), EndTime = new TimeSpan(0, 0, 0), IsAvailable = false },
                new() { DoctorId = doctors[1].DoctorId, DayOfWeek = DayOfWeek.Sunday, StartTime = new TimeSpan(0, 0, 0), EndTime = new TimeSpan(0, 0, 0), IsAvailable = false }
            };

            // Dr. Arjun Verma (Gynecology) - All days, 9 AM - 4 PM
            var arjunSchedules = new List<DoctorSchedule>
            {
                new() { DoctorId = doctors[2].DoctorId, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(16, 0, 0), IsAvailable = true },
                new() { DoctorId = doctors[2].DoctorId, DayOfWeek = DayOfWeek.Tuesday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(16, 0, 0), IsAvailable = true },
                new() { DoctorId = doctors[2].DoctorId, DayOfWeek = DayOfWeek.Wednesday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(16, 0, 0), IsAvailable = true },
                new() { DoctorId = doctors[2].DoctorId, DayOfWeek = DayOfWeek.Thursday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(16, 0, 0), IsAvailable = true },
                new() { DoctorId = doctors[2].DoctorId, DayOfWeek = DayOfWeek.Friday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(16, 0, 0), IsAvailable = true },
                new() { DoctorId = doctors[2].DoctorId, DayOfWeek = DayOfWeek.Saturday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(16, 0, 0), IsAvailable = true },
                new() { DoctorId = doctors[2].DoctorId, DayOfWeek = DayOfWeek.Sunday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(16, 0, 0), IsAvailable = true }
            };

            doctors[0].Schedules = rameshSchedules;
            doctors[1].Schedules = priyaSchedules;
            doctors[2].Schedules = arjunSchedules;

            foreach (var doctor in doctors)
                await doctorRepo.UpdateAsync(doctor, ct);

            logger.LogInformation("Doctor schedules seeded successfully.");

            // ---------------- PROVIDERS (for appointments) ----------------
            var providers = new List<Provider>
            {
                // Cardiology - matches Dr. Ramesh Gupta
                new("Dr. Ramesh Gupta", "Cardiology",
                    "chest pain,heart attack,palpitations,angina,cardiac arrest,heart disease,hypertension,high blood pressure,chest discomfort,breathlessness",
                    DateTime.Today.AddHours(9), DateTime.Today.AddHours(17)),
                
                // Orthopedics - matches Dr. Priya Nair
                new("Dr. Priya Nair", "Orthopedics",
                    "bone pain,fracture,joint pain,arthritis,back pain,knee pain,sprain,ligament injury,sports injury,shoulder pain,neck pain",
                    DateTime.Today.AddHours(10), DateTime.Today.AddHours(18)),
                
                // Gynecology - matches Dr. Arjun Verma
                new("Dr. Arjun Verma", "Gynecology",
                    "pregnancy,menstrual problems,pcos,periods,cramps,ovarian cyst,infertility,prenatal care,postnatal,gynec issue",
                    DateTime.Today.AddHours(9), DateTime.Today.AddHours(16)),
                
                // Additional providers (no matching doctors yet)
                new("Dr. Anjali Reddy", "Dermatology",
                    "skin rash,acne,eczema,psoriasis,itching,allergy,skin infection,pigmentation,hair fall,fungal infection,warts",
                    DateTime.Today.AddHours(10), DateTime.Today.AddHours(17)),

                new("Dr. Vikram Menon", "Neurology",
                    "headache,migraine,seizure,stroke,paralysis,tremor,parkinson,epilepsy,nerve pain,numbness,dizziness",
                    DateTime.Today.AddHours(9), DateTime.Today.AddHours(18)),

                new("Dr. Sunil Iyer", "Gastroenterology",
                    "stomach pain,acidity,gastritis,ulcer,diarrhea,constipation,ibs,crohn,liver disease,jaundice,nausea,vomiting",
                    DateTime.Today.AddHours(8), DateTime.Today.AddHours(16)),

                new("Dr. Meera Joshi", "Pediatrics",
                    "child fever,vaccination,newborn,baby cold,infant care,growth issues,child cough,pediatric consultation,baby rash",
                    DateTime.Today.AddHours(9), DateTime.Today.AddHours(17)),

                new("Dr. Rajesh Kumar", "ENT",
                    "ear pain,sinus,throat infection,tonsillitis,hearing loss,vertigo,nose bleeding,voice problem,snoring,ear discharge",
                    DateTime.Today.AddHours(10), DateTime.Today.AddHours(18)),

                new("Dr. Kavita Shah", "Ophthalmology",
                    "eye pain,blurred vision,cataract,glaucoma,pink eye,conjunctivitis,dry eyes,retinal problem,eye infection,vision loss",
                    DateTime.Today.AddHours(9), DateTime.Today.AddHours(16)),

                new("Dr. Ashok Pillai", "Pulmonology",
                    "cough,asthma,breathlessness,copd,pneumonia,lung infection,bronchitis,chest congestion,tuberculosis,wheezing",
                    DateTime.Today.AddHours(9), DateTime.Today.AddHours(17)),

                new("Dr. Lakshmi Rao", "Endocrinology",
                    "diabetes,thyroid,hormonal imbalance,pcos,weight gain,obesity,metabolic syndrome,insulin resistance,growth hormone",
                    DateTime.Today.AddHours(10), DateTime.Today.AddHours(18)),

                new("Dr. Karan Desai", "Urology",
                    "kidney stone,uti,urinary infection,prostate,bladder pain,frequent urination,blood in urine,kidney pain",
                    DateTime.Today.AddHours(9), DateTime.Today.AddHours(17)),

                new("Dr. Ritu Bansal", "Psychiatry",
                    "depression,anxiety,stress,panic attack,bipolar,ocd,insomnia,mental health,ptsd,counseling,addiction",
                    DateTime.Today.AddHours(11), DateTime.Today.AddHours(19)),

                new("Dr. Anil Shetty", "General Medicine",
                    "fever,fatigue,weakness,cold,flu,body pain,general checkup,viral infection,common illness,routine consultation",
                    DateTime.Today.AddHours(8), DateTime.Today.AddHours(20)),

                new("Dr. Pooja Kapoor", "Rheumatology",
                    "rheumatoid arthritis,lupus,autoimmune disease,joint inflammation,muscle pain,gout,spondylitis,fibromyalgia",
                    DateTime.Today.AddHours(10), DateTime.Today.AddHours(17))
            };

            logger.LogInformation("Seeding {Count} Provider profiles with symptom mappings.", providers.Count);
            foreach (var p in providers)
                await providerRepo.AddAsync(p, ct);

            await uow.CommitAsync(ct);
            logger.LogInformation("Committed Provider profiles.");

            await uow.BeginTransactionAsync(ct);

            // ---------------- LINK DOCTORS TO PROVIDERS ----------------
            logger.LogInformation("Linking Doctors to their matching Providers...");

            doctors[0].ProviderId = providers[0].Id; // Ramesh -> Cardiology Provider
            doctors[1].ProviderId = providers[1].Id; // Priya -> Orthopedics Provider
            doctors[2].ProviderId = providers[2].Id; // Arjun -> Gynecology Provider

            foreach (var doctor in doctors)
                await doctorRepo.UpdateAsync(doctor, ct);

            logger.LogInformation("Doctors linked to Providers successfully.");

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

            await uow.CommitAsync(ct);
            logger.LogInformation("Committed Patient User accounts.");

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

            await uow.CommitAsync(ct);
            logger.LogInformation("Committed Staff User accounts.");

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

            await uow.CommitAsync(ct);
            logger.LogInformation("✅ Database seeding completed successfully.");
            }
        catch (Exception ex)
            {
            await uow.RollbackAsync(ct);
            logger.LogError(ex, "❌ Database seeding failed. Rolling back transaction.");
            throw;
            }
        }
    }
