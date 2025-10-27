using Clinix.Application.Interfaces.Functionalities;
using Clinix.Application.Interfaces.UserRepo;
using Clinix.Domain.Entities.ApplicationUsers;
using Clinix.Domain.Entities.Inventory;
using Clinix.Domain.Entities;
using Clinix.Domain.Entities.System;
using Clinix.Domain.Interfaces;
using Clinix.Domain.ValueObjects;
using Clinix.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Clinix.Infrastructure.Events;

namespace Clinix.Infrastructure.Data;

/// <summary>
/// Comprehensive data seeder with proper entity relationships and flow.
/// Ensures all user data is unique with guaranteed phone/email ranges.
/// </summary>
public static class DataSeeder
    {
    private const string SEED_NAME = "ClinixComprehensiveData";
    private const string SEED_VERSION = "3.0.0";
    private const int MAX_RETRIES = 3;

    #region Seed Data Definitions

    private static class SeedData
        {
        public const string DEFAULT_PASSWORD = "Test@123";
        public static string AdminEmail = "admin@hms.local";
        public static string AdminPassword = "Admin@123#";

        /// <summary>
        /// 15 Doctor users - Phone: 9100000001-9100000015
        /// </summary>
        public static readonly List<UserSeedData> Doctors = new()
        {
            new("Dr. Ramesh Gupta", "ramesh.gupta@hms.local", "9100000001"),
            new("Dr. Priya Nair", "priya.nair@hms.local", "9100000002"),
            new("Dr. Arjun Verma", "arjun.verma@hms.local", "9100000003"),
            new("Dr. Anjali Reddy", "anjali.reddy@hms.local", "9100000004"),
            new("Dr. Vikram Menon", "vikram.menon@hms.local", "9100000005"),
            new("Dr. Sunil Iyer", "sunil.iyer@hms.local", "9100000006"),
            new("Dr. Meera Joshi", "meera.joshi@hms.local", "9100000007"),
            new("Dr. Rajesh Kumar", "rajesh.kumar@hms.local", "9100000008"),
            new("Dr. Kavita Shah", "kavita.shah@hms.local", "9100000009"),
            new("Dr. Ashok Pillai", "ashok.pillai@hms.local", "9100000010"),
            new("Dr. Lakshmi Rao", "lakshmi.rao@hms.local", "9100000011"),
            new("Dr. Karan Desai", "karan.desai@hms.local", "9100000012"),
            new("Dr. Ritu Bansal", "ritu.bansal@hms.local", "9100000013"),
            new("Dr. Anil Shetty", "anil.shetty@hms.local", "9100000014"),
            new("Dr. Pooja Kapoor", "pooja.kapoor@hms.local", "9100000015")
        };

        /// <summary>
        /// 5 Patient users - Phone: 9200000001-9200000005
        /// </summary>
        public static readonly List<UserSeedData> Patients = new()
        {
            new("Ravi Mehta", "ravi.mehta@hms.local", "9200000001"),
            new("Neha Sharma", "neha.sharma@hms.local", "9200000002"),
            new("Suresh Kumar", "suresh.kumar@hms.local", "9200000003"),
            new("Anjali Desai", "anjali.desai@hms.local", "9200000004"),
            new("Karan Singh", "karan.singh@hms.local", "9200000005")
        };

        /// <summary>
        /// 2 Staff users - Phone: 9300000001-9300000002
        /// </summary>
        public static readonly List<StaffSeedData> Staff = new()
        {
            new("Pooja Joshi", "pooja.joshi@hms.local", "9300000001", "Receptionist", "Front Desk"),
            new("Vikram Singh", "vikram.singh@hms.local", "9300000002", "Chemist", "Pharmacy")
        };

        /// <summary>
        /// 15 Provider specialties - Maps 1:1 with Doctors
        /// </summary>
        public static readonly List<ProviderSeedData> Providers = new()
        {
            new("Cardiology", "chest pain,heart attack,palpitations,angina,cardiac arrest,hypertension", 9, 17),
            new("Orthopedics", "bone pain,fracture,joint pain,arthritis,back pain,knee pain,sprain", 10, 18),
            new("Gynecology", "pregnancy,menstrual problems,pcos,periods,cramps,ovarian cyst,fertility", 9, 16),
            new("Dermatology", "skin rash,acne,eczema,psoriasis,itching,allergy,dermatitis,hives", 10, 17),
            new("Neurology", "headache,migraine,seizure,stroke,paralysis,tremor,parkinson,epilepsy", 9, 18),
            new("Gastroenterology", "stomach pain,acidity,gastritis,ulcer,diarrhea,constipation,ibs", 8, 16),
            new("Pediatrics", "child fever,vaccination,newborn,baby cold,infant care,immunization", 9, 17),
            new("ENT", "ear pain,sinus,throat infection,tonsillitis,hearing loss,vertigo", 10, 18),
            new("Ophthalmology", "eye pain,blurred vision,cataract,glaucoma,pink eye,vision loss", 9, 16),
            new("Pulmonology", "cough,asthma,breathlessness,copd,pneumonia,bronchitis", 9, 17),
            new("Endocrinology", "diabetes,thyroid,hormonal imbalance,pcos,weight gain,metabolic", 10, 18),
            new("Urology", "kidney stone,uti,urinary infection,prostate,bladder pain,kidney pain", 9, 17),
            new("Psychiatry", "depression,anxiety,stress,panic attack,bipolar,ocd,mental health", 11, 19),
            new("General Medicine", "fever,fatigue,weakness,cold,flu,body pain,general checkup", 8, 20),
            new("Rheumatology", "rheumatoid arthritis,lupus,autoimmune disease,joint inflammation", 10, 17)
        };

        /// <summary>
        /// Patient profiles - Emergency contacts: 9400000001-9400000005
        /// </summary>
        public static readonly List<PatientProfileData> PatientProfiles = new()
        {
            new("Male", new DateTime(1985, 5, 15), "B+", "Sunita Mehta", "9400000001", "MRN1000"),
            new("Female", new DateTime(1992, 8, 22), "O+", "Raj Sharma", "9400000002", "MRN1001"),
            new("Male", new DateTime(1978, 3, 10), "A-", "Manish Kumar", "9400000003", "MRN1002"),
            new("Female", new DateTime(1995, 11, 5), "AB+", "Vikram Desai", "9400000004", "MRN1003"),
            new("Male", new DateTime(1988, 7, 30), "O-", "Priya Singh", "9400000005", "MRN1004")
        };

        /// <summary>
        /// Doctor profiles with unique license numbers
        /// </summary>
        public static readonly List<DoctorProfileData> DoctorProfiles = new()
        {
            new("MBBS, MD (Cardiology)", 12, "A101", 800m, "LIC100001"),
            new("MBBS, MS (Orthopedics)", 10, "A102", 750m, "LIC100002"),
            new("MBBS, DGO", 8, "B101", 700m, "LIC100003"),
            new("MBBS, MD (Dermatology)", 7, "B102", 650m, "LIC100004"),
            new("MBBS, DM (Neurology)", 15, "C101", 1000m, "LIC100005"),
            new("MBBS, MD (Gastroenterology)", 11, "C102", 850m, "LIC100006"),
            new("MBBS, MD (Pediatrics)", 9, "D101", 700m, "LIC100007"),
            new("MBBS, MS (ENT)", 8, "D102", 650m, "LIC100008"),
            new("MBBS, MS (Ophthalmology)", 10, "E101", 700m, "LIC100009"),
            new("MBBS, MD (Pulmonology)", 12, "E102", 800m, "LIC100010"),
            new("MBBS, MD (Endocrinology)", 9, "A103", 750m, "LIC100011"),
            new("MBBS, MS (Urology)", 11, "B103", 850m, "LIC100012"),
            new("MBBS, MD (Psychiatry)", 14, "C103", 900m, "LIC100013"),
            new("MBBS, MD", 6, "D103", 600m, "LIC100014"),
            new("MBBS, MD (Rheumatology)", 10, "E103", 750m, "LIC100015")
        };

        /// <summary>
        /// 8 Inventory items
        /// </summary>
        public static readonly List<InventorySeedData> Inventory = new()
        {
            new("Paracetamol", "Medicine", "Analgesic", "Tablets", 50, 200),
            new("Amoxicillin", "Medicine", "Antibiotic", "Capsules", 40, 120),
            new("Syringe 5ml", "Consumable", "Injection", "Pieces", 100, 500),
            new("Glucose Bottle", "Consumable", "IV Fluid", "Bottles", 30, 80),
            new("Stethoscope", "Equipment", "Diagnostic", "Pieces", 5, 15),
            new("Blood Pressure Monitor", "Equipment", "Diagnostic", "Pieces", 10, 25),
            new("Surgical Gloves", "Consumable", "PPE", "Pairs", 200, 1000),
            new("Face Masks", "Consumable", "PPE", "Pieces", 500, 2000)
        };
        }

    #region Data Models
    private record UserSeedData(string FullName, string Email, string Phone);
    private record StaffSeedData(string FullName, string Email, string Phone, string Position, string Department);
    private record ProviderSeedData(string Specialty, string Keywords, int StartHour, int EndHour);
    private record PatientProfileData(string Gender, DateTime DateOfBirth, string BloodGroup, string EmergencyName, string EmergencyPhone, string MRN);
    private record DoctorProfileData(string Degree, int ExperienceYears, string RoomNumber, decimal ConsultationFee, string LicenseNumber);
    private record InventorySeedData(string Name, string Type, string Category, string Unit, int MinStock, int CurrentStock);
    #endregion

    #endregion

    public static async Task SeedAsync(IServiceProvider sp, CancellationToken ct = default)
        {
        using var scope = sp.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger(typeof(DataSeeder).FullName!);

        DomainEventDispatcher.IsSeeding = true;

        try
            {
            logger.LogInformation("🔍 Checking seed status for '{SeedName}' v{Version}", SEED_NAME, SEED_VERSION);

            var seedStatusRepo = serviceProvider.GetRequiredService<ISeedStatusRepository>();

            if (await seedStatusRepo.IsSeedCompletedAsync(SEED_NAME, SEED_VERSION, ct))
                {
                logger.LogInformation("✅ Seed '{SeedName}' v{Version} already completed.", SEED_NAME, SEED_VERSION);
                return;
                }

            var existingSeedStatus = await seedStatusRepo.GetBySeedNameAsync(SEED_NAME, SEED_VERSION, ct);
            if (existingSeedStatus != null && existingSeedStatus.RetryCount >= MAX_RETRIES)
                {
                logger.LogError("❌ Seed '{SeedName}' v{Version} failed {Count} times.", SEED_NAME, SEED_VERSION, existingSeedStatus.RetryCount);
                return;
                }

            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var userRepo = serviceProvider.GetRequiredService<IUserRepository>();
            var doctorRepo = serviceProvider.GetRequiredService<IDoctorRepository>();
            var patientRepo = serviceProvider.GetRequiredService<IPatientRepository>();
            var staffRepo = serviceProvider.GetRequiredService<IStaffRepository>();
            var inventoryRepo = serviceProvider.GetRequiredService<IInventoryService>();
            var providerRepo = serviceProvider.GetRequiredService<IProviderRepository>();
            var scheduleRepo = serviceProvider.GetRequiredService<IDoctorScheduleRepository>();
            var appointmentRepo = serviceProvider.GetRequiredService<IAppointmentRepository>();
            var uow = serviceProvider.GetRequiredService<IUnitOfWork>();

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

            logger.LogInformation("🌱 Starting seed attempt {Retry} of {Max}", seedStatus.RetryCount, MAX_RETRIES);

            SeedData.AdminEmail = config["SeedAdmin:Email"] ?? SeedData.AdminEmail;
            SeedData.AdminPassword = config["SeedAdmin:Password"] ?? SeedData.AdminPassword;

            var passwordHasher = new PasswordHasher<User>();

            await uow.BeginTransactionAsync(ct);
            try
                {
                // ==================================================================
                // STEP 1: Seed Admin User
                // ==================================================================
                await SeedAdminUser(userRepo, passwordHasher, logger, ct);
                await uow.CommitAsync(ct);
                await uow.BeginTransactionAsync(ct);

                // ==================================================================
                // STEP 2: Seed Providers (15 independent entities)
                // ==================================================================
                var providers = await SeedProviders(providerRepo, logger, ct);
                await uow.CommitAsync(ct);
                await uow.BeginTransactionAsync(ct);

                // ==================================================================
                // STEP 3: Seed Doctor Users (15 users)
                // ==================================================================
                var doctorUsers = await SeedDoctorUsers(userRepo, passwordHasher, logger, ct);
                await uow.CommitAsync(ct);
                await uow.BeginTransactionAsync(ct);

                // ==================================================================
                // STEP 4: Seed Doctor Profiles (link UserId + ProviderId)
                // ==================================================================
                var doctors = await SeedDoctorProfiles(doctorRepo, doctorUsers, providers, logger, ct);
                await uow.CommitAsync(ct);
                await uow.BeginTransactionAsync(ct);

                // ==================================================================
                // STEP 5: Seed Doctor Schedules (using Doctor.DoctorId)
                // ==================================================================
                await SeedDoctorSchedules(scheduleRepo, doctors, logger, ct);
                await uow.CommitAsync(ct);
                await uow.BeginTransactionAsync(ct);

                // ==================================================================
                // STEP 6: Seed Patient Users (5 users)
                // ==================================================================
                var patientUsers = await SeedPatientUsers(userRepo, passwordHasher, logger, ct);
                await uow.CommitAsync(ct);
                await uow.BeginTransactionAsync(ct);

                // ==================================================================
                // STEP 7: Seed Patient Profiles
                // ==================================================================
                var patients = await SeedPatientProfiles(patientRepo, patientUsers, logger, ct);
                await uow.CommitAsync(ct);
                await uow.BeginTransactionAsync(ct);

                // ==================================================================
                // STEP 8: Seed Appointments (using Patient.PatientId + Provider.Id)
                // ==================================================================
                await SeedAppointments(appointmentRepo, patients, providers, logger, ct);
                await uow.CommitAsync(ct);
                await uow.BeginTransactionAsync(ct);

                // ==================================================================
                // STEP 9: Seed Staff Users (2 users)
                // ==================================================================
                var staffUsers = await SeedStaffUsers(userRepo, passwordHasher, logger, ct);
                await uow.CommitAsync(ct);
                await uow.BeginTransactionAsync(ct);

                // ==================================================================
                // STEP 10: Seed Staff Profiles (UserId only - PK/FK)
                // ==================================================================
                await SeedStaffProfiles(staffRepo, staffUsers, logger, ct);
                await uow.CommitAsync(ct);
                await uow.BeginTransactionAsync(ct);

                // ==================================================================
                // STEP 11: Seed Inventory (8 items)
                // ==================================================================
                await SeedInventory(inventoryRepo, logger, ct);

                seedStatus.IsCompleted = true;
                seedStatus.ErrorMessage = null;
                await seedStatusRepo.UpdateAsync(seedStatus, ct);

                await uow.CommitAsync(ct);

                logger.LogInformation("✅ Seed '{SeedName}' v{Version} completed successfully.", SEED_NAME, SEED_VERSION);
                LogSeedSummary(logger);
                }
            catch (Exception ex)
                {
                await uow.RollbackAsync(ct);
                seedStatus.ErrorMessage = ex.Message.Length > 1000 ? ex.Message[..1000] : ex.Message;
                await seedStatusRepo.UpdateAsync(seedStatus, ct);
                logger.LogError(ex, "❌ Seed failed: {Error}", ex.Message);
                throw;
                }
            }
        finally
            {
            DomainEventDispatcher.IsSeeding = false;
            }
        }

    #region Seeding Methods

    private static async Task SeedAdminUser(IUserRepository userRepo, PasswordHasher<User> passwordHasher, ILogger logger, CancellationToken ct)
        {
        if (await userRepo.GetByEmailAsync(SeedData.AdminEmail, ct) is not null)
            {
            logger.LogInformation("Admin already exists. Skipping.");
            return;
            }

        logger.LogInformation("Seeding Admin: {Email}", SeedData.AdminEmail);
        var admin = new User
            {
            FullName = "System Administrator",
            Email = SeedData.AdminEmail,
            Phone = "9876543210",
            Role = "Admin",
            CreatedBy = "system",
            CreatedAt = DateTime.UtcNow
            };
        admin.PasswordHash = passwordHasher.HashPassword(admin, SeedData.AdminPassword);
        await userRepo.AddAsync(admin, ct);
        logger.LogInformation("✅ Admin seeded.");
        }

    private static async Task<List<Provider>> SeedProviders(IProviderRepository providerRepo, ILogger logger, CancellationToken ct)
        {
        var existing = await providerRepo.SearchAsync(Array.Empty<string>(), ct);
        if (existing.Count >= SeedData.Providers.Count)
            {
            logger.LogInformation("Providers already exist. Skipping.");
            return existing;
            }

        logger.LogInformation("Seeding {Count} providers...", SeedData.Providers.Count);
        var providers = new List<Provider>();

        for (int i = 0; i < SeedData.Providers.Count; i++)
            {
            var data = SeedData.Providers[i];
            var doctorName = SeedData.Doctors[i].FullName;

            var provider = new Provider(
                doctorName,
                data.Specialty,
                data.Keywords,
                DateTime.Today.AddHours(data.StartHour),
                DateTime.Today.AddHours(data.EndHour)
            );

            await providerRepo.AddAsync(provider, ct);
            providers.Add(provider);
            }

        logger.LogInformation("✅ Seeded {Count} providers.", providers.Count);
        return providers;
        }

    private static async Task<List<User>> SeedDoctorUsers(IUserRepository userRepo, PasswordHasher<User> passwordHasher, ILogger logger, CancellationToken ct)
        {
        logger.LogInformation("Seeding {Count} doctor users...", SeedData.Doctors.Count);
        var doctorUsers = new List<User>();

        foreach (var data in SeedData.Doctors)
            {
            var existing = await userRepo.GetByEmailAsync(data.Email, ct);
            if (existing != null)
                {
                doctorUsers.Add(existing);
                continue;
                }

            var user = new User
                {
                FullName = data.FullName,
                Email = data.Email,
                Phone = data.Phone,
                Role = "Doctor",
                CreatedBy = "system",
                CreatedAt = DateTime.UtcNow
                };
            user.PasswordHash = passwordHasher.HashPassword(user, SeedData.DEFAULT_PASSWORD);
            await userRepo.AddAsync(user, ct);
            doctorUsers.Add(user);
            }

        logger.LogInformation("✅ Seeded {Count} doctor users.", doctorUsers.Count);
        return doctorUsers;
        }

    private static async Task<List<Doctor>> SeedDoctorProfiles(IDoctorRepository doctorRepo, List<User> doctorUsers, List<Provider> providers, ILogger logger, CancellationToken ct)
        {
        logger.LogInformation("Seeding {Count} doctor profiles...", doctorUsers.Count);
        var doctors = new List<Doctor>();

        for (int i = 0; i < doctorUsers.Count && i < providers.Count && i < SeedData.DoctorProfiles.Count; i++)
            {
            var user = doctorUsers[i];
            var provider = providers[i];
            var profileData = SeedData.DoctorProfiles[i];

            var existing = await doctorRepo.GetByUserIdAsync(user.Id, ct);
            if (existing != null)
                {
                doctors.Add(existing);
                continue;
                }

            var doctor = new Doctor
                {
                UserId = user.Id,
                ProviderId = provider.Id,
                Degree = profileData.Degree,
                Specialty = provider.Specialty,
                LicenseNumber = profileData.LicenseNumber,
                ExperienceYears = profileData.ExperienceYears,
                RoomNumber = profileData.RoomNumber,
                ConsultationFee = profileData.ConsultationFee,
                IsActive = true,
                IsOnDuty = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
                };

            await doctorRepo.AddAsync(doctor, ct);
            doctors.Add(doctor);
            }

        logger.LogInformation("✅ Seeded {Count} doctor profiles.", doctors.Count);
        return doctors;
        }

    private static async Task SeedDoctorSchedules(IDoctorScheduleRepository scheduleRepo, List<Doctor> doctors, ILogger logger, CancellationToken ct)
        {
        logger.LogInformation("Seeding doctor schedules...");

        foreach (var doctor in doctors)
            {
            var schedules = CreateSchedulesForDoctor(doctor);
            foreach (var schedule in schedules)
                {
                var existing = await scheduleRepo.GetByDoctorAndDayAsync(doctor.DoctorId, schedule.DayOfWeek, ct);
                if (existing == null)
                    {
                    await scheduleRepo.AddRangeAsync(new List<DoctorSchedule> { schedule }, ct);
                    }
                }
            }

        logger.LogInformation("✅ Seeded schedules for {Count} doctors.", doctors.Count);
        }

    private static async Task<List<User>> SeedPatientUsers(IUserRepository userRepo, PasswordHasher<User> passwordHasher, ILogger logger, CancellationToken ct)
        {
        logger.LogInformation("Seeding {Count} patient users...", SeedData.Patients.Count);
        var patientUsers = new List<User>();

        foreach (var data in SeedData.Patients)
            {
            var existing = await userRepo.GetByEmailAsync(data.Email, ct);
            if (existing != null)
                {
                patientUsers.Add(existing);
                continue;
                }

            var user = new User
                {
                FullName = data.FullName,
                Email = data.Email,
                Phone = data.Phone,
                Role = "Patient",
                CreatedBy = "system",
                CreatedAt = DateTime.UtcNow
                };
            user.PasswordHash = passwordHasher.HashPassword(user, SeedData.DEFAULT_PASSWORD);
            await userRepo.AddAsync(user, ct);
            patientUsers.Add(user);
            }

        logger.LogInformation("✅ Seeded {Count} patient users.", patientUsers.Count);
        return patientUsers;
        }

    private static async Task<List<Patient>> SeedPatientProfiles(IPatientRepository patientRepo, List<User> patientUsers, ILogger logger, CancellationToken ct)
        {
        logger.LogInformation("Seeding {Count} patient profiles...", patientUsers.Count);
        var patients = new List<Patient>();

        for (int i = 0; i < patientUsers.Count && i < SeedData.PatientProfiles.Count; i++)
            {
            var user = patientUsers[i];
            var profileData = SeedData.PatientProfiles[i];

            var existing = await patientRepo.GetByUserIdAsync(user.Id, ct);
            if (existing != null)
                {
                patients.Add(existing);
                continue;
                }

            var patient = new Patient
                {
                UserId = user.Id,
                Gender = profileData.Gender,
                DateOfBirth = profileData.DateOfBirth,
                BloodGroup = profileData.BloodGroup,
                EmergencyContactName = profileData.EmergencyName,
                EmergencyContactNumber = profileData.EmergencyPhone,
                MedicalRecordNumber = profileData.MRN
                };

            await patientRepo.AddAsync(patient, ct);
            patients.Add(patient);
            }

        logger.LogInformation("✅ Seeded {Count} patient profiles.", patients.Count);
        return patients;
        }

    private static async Task SeedAppointments(IAppointmentRepository appointmentRepo, List<Patient> patients, List<Provider> providers, ILogger logger, CancellationToken ct)
        {
        logger.LogInformation("Seeding 15 sample appointments...");

        var scenarios = new List<(int Days, AppointmentStatus Status, AppointmentType Type, string Notes)>
        {
            (-28, AppointmentStatus.Completed, AppointmentType.Consultation, "Initial consultation completed"),
            (-21, AppointmentStatus.Completed, AppointmentType.Consultation, "Follow-up after treatment"),
            (-14, AppointmentStatus.NoShow, AppointmentType.Consultation, "Patient did not arrive"),
            (-10, AppointmentStatus.Completed, AppointmentType.Procedure, "Procedure completed"),
            (-7, AppointmentStatus.Cancelled, AppointmentType.Consultation, "Patient cancelled"),
            (-5, AppointmentStatus.Completed, AppointmentType.FollowUp, "Follow-up post procedure"),
            (-3, AppointmentStatus.Completed, AppointmentType.Consultation, "Routine checkup"),
            (0, AppointmentStatus.Confirmed, AppointmentType.Consultation, "Today's appointment"),
            (2, AppointmentStatus.Scheduled, AppointmentType.Consultation, "Upcoming consultation"),
            (5, AppointmentStatus.Scheduled, AppointmentType.FollowUp, "Follow-up scheduled"),
            (7, AppointmentStatus.Confirmed, AppointmentType.Procedure, "Confirmed procedure"),
            (10, AppointmentStatus.Scheduled, AppointmentType.Telehealth, "Telehealth consultation"),
            (14, AppointmentStatus.Scheduled, AppointmentType.Consultation, "Future appointment"),
            (21, AppointmentStatus.Scheduled, AppointmentType.Consultation, "Future appointment 2"),
            (28, AppointmentStatus.Scheduled, AppointmentType.FollowUp, "Long-term follow-up")
        };

        for (int i = 0; i < scenarios.Count; i++)
            {
            var patient = patients[i % patients.Count];
            var provider = providers[i % providers.Count];
            var scenario = scenarios[i];

            var appointmentDate = DateTime.Today.AddDays(scenario.Days);
            var startHour = 9 + (i % 8);
            var start = new DateTimeOffset(appointmentDate.AddHours(startHour), DateTimeOffset.Now.Offset);
            var end = start.AddMinutes(30);

            var appointment = Appointment.Schedule(
                patient.PatientId,
                provider.Id,
                scenario.Type,
                new DateRange(start, end),
                scenario.Notes
            );

            switch (scenario.Status)
                {
                case AppointmentStatus.Completed:
                    appointment.Complete();
                    break;
                case AppointmentStatus.Cancelled:
                    appointment.Cancel("Demo cancellation");
                    break;
                case AppointmentStatus.Confirmed:
                    appointment.Approve();
                    break;
                }

            await appointmentRepo.AddAsync(appointment, ct);
            }

        logger.LogInformation("✅ Seeded 15 appointments.");
        }

    private static async Task<List<User>> SeedStaffUsers(IUserRepository userRepo, PasswordHasher<User> passwordHasher, ILogger logger, CancellationToken ct)
        {
        logger.LogInformation("Seeding {Count} staff users...", SeedData.Staff.Count);
        var staffUsers = new List<User>();

        foreach (var data in SeedData.Staff)
            {
            var existing = await userRepo.GetByEmailAsync(data.Email, ct);
            if (existing != null)
                {
                staffUsers.Add(existing);
                continue;
                }

            var user = new User
                {
                FullName = data.FullName,
                Email = data.Email,
                Phone = data.Phone,
                Role = "Staff",
                CreatedBy = "system",
                CreatedAt = DateTime.UtcNow
                };
            user.PasswordHash = passwordHasher.HashPassword(user, SeedData.DEFAULT_PASSWORD);
            await userRepo.AddAsync(user, ct);
            staffUsers.Add(user);
            }

        logger.LogInformation("✅ Seeded {Count} staff users.", staffUsers.Count);
        return staffUsers;
        }

    private static async Task SeedStaffProfiles(IStaffRepository staffRepo, List<User> staffUsers, ILogger logger, CancellationToken ct)
        {
        logger.LogInformation("Seeding {Count} staff profiles...", staffUsers.Count);

        for (int i = 0; i < staffUsers.Count && i < SeedData.Staff.Count; i++)
            {
            var user = staffUsers[i];
            var data = SeedData.Staff[i];

            var staff = new Staff
                {
                UserId = user.Id,
                Position = data.Position,
                Department = data.Department,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
                };

            await staffRepo.AddAsync(staff, ct);
            }

        logger.LogInformation("✅ Seeded {Count} staff profiles.", staffUsers.Count);
        }

    private static async Task SeedInventory(IInventoryService inventoryRepo, ILogger logger, CancellationToken ct)
        {
        logger.LogInformation("Seeding {Count} inventory items...", SeedData.Inventory.Count);

        foreach (var data in SeedData.Inventory)
            {
            var item = new InventoryItem
                {
                Name = data.Name,
                Type = data.Type,
                Category = data.Category,
                Unit = data.Unit,
                MinStock = data.MinStock,
                CurrentStock = data.CurrentStock
                };

            await inventoryRepo.AddItemAsync(item);
            }

        logger.LogInformation("✅ Seeded {Count} inventory items.", SeedData.Inventory.Count);
        }

    #endregion

    #region Helper Methods

    private static List<DoctorSchedule> CreateSchedulesForDoctor(Doctor doctor)
        {
        var schedules = new List<DoctorSchedule>();
        DayOfWeek[] workingDays;
        TimeSpan startTime, endTime;

        switch (doctor.Specialty)
            {
            case "Psychiatry":
                workingDays = new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday, DayOfWeek.Saturday };
                startTime = new TimeSpan(11, 0, 0);
                endTime = new TimeSpan(19, 0, 0);
                break;
            case "General Medicine":
                workingDays = Enum.GetValues<DayOfWeek>();
                startTime = new TimeSpan(8, 0, 0);
                endTime = new TimeSpan(20, 0, 0);
                break;
            case "Pediatrics":
            case "Gynecology":
                workingDays = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday };
                startTime = new TimeSpan(9, 0, 0);
                endTime = new TimeSpan(17, 0, 0);
                break;
            default:
                workingDays = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
                startTime = new TimeSpan(9, 0, 0);
                endTime = new TimeSpan(17, 0, 0);
                break;
            }

        foreach (var day in Enum.GetValues<DayOfWeek>())
            {
            var isWorking = workingDays.Contains(day);
            schedules.Add(new DoctorSchedule
                {
                DoctorId = doctor.DoctorId,
                DayOfWeek = day,
                StartTime = isWorking ? startTime : TimeSpan.Zero,
                EndTime = isWorking ? endTime : TimeSpan.Zero,
                IsAvailable = isWorking,
                Notes = isWorking ? $"Regular {doctor.Specialty} hours" : "Day off"
                });
            }

        return schedules;
        }

    private static void LogSeedSummary(ILogger logger)
        {
        logger.LogInformation("╔════════════════════════════════════╗");
        logger.LogInformation("║    SEED COMPLETED (v3.0.0)         ║");
        logger.LogInformation("╠════════════════════════════════════╣");
        logger.LogInformation("║ Admin:              1              ║");
        logger.LogInformation("║ Providers:         15              ║");
        logger.LogInformation("║ Doctors:           15              ║");
        logger.LogInformation("║ Patients:           5              ║");
        logger.LogInformation("║ Staff:              2              ║");
        logger.LogInformation("║ Appointments:      15              ║");
        logger.LogInformation("║ Inventory:          8              ║");
        logger.LogInformation("╠════════════════════════════════════╣");
        logger.LogInformation("║ Default Password: Test@123         ║");
        logger.LogInformation("╚════════════════════════════════════╝");
        }

    #endregion
    }
