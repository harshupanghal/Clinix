using Clinix.Domain.Entities;
using Clinix.Domain.Entities.ApplicationUsers;
using Clinix.Domain.Entities.Appointments;
using Clinix.Domain.Entities.FollowUps;
using Clinix.Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Clinix.Infrastructure.Persistence
    {
    public class ClinixDbContext : DbContext
        {
        public ClinixDbContext(DbContextOptions<ClinixDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<Doctor> Doctors => Set<Doctor>();
        public DbSet<Staff> Staffs => Set<Staff>();
        public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
        public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<AppointmentClinicalInfo> AppointmentClinicalInfos => Set<AppointmentClinicalInfo>();

        // Follow-up
        public DbSet<FollowUpRecord> FollowUpRecords => Set<FollowUpRecord>();
        public DbSet<FollowUpPrescriptionSnapshot> FollowUpPrescriptionSnapshots => Set<FollowUpPrescriptionSnapshot>();
        public DbSet<FollowUpTask> FollowUpTasks => Set<FollowUpTask>();

        public DbSet<SymptomMapping> SymptomMappings => Set<SymptomMapping>();
        public DbSet<DoctorWorkingHours> DoctorWorkingHours => Set<DoctorWorkingHours>();
        public DbSet<ScheduleLock> ScheduleLocks => Set<ScheduleLock>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
            base.OnModelCreating(modelBuilder);

            // ---------------------------
            // Users
            // ---------------------------
            modelBuilder.Entity<User>(b =>
            {
                b.ToTable("Users");
                b.HasKey(x => x.Id);
                b.Property(x => x.Email).HasMaxLength(320);
                b.Property(x => x.Phone).IsRequired().HasMaxLength(20);
                b.Property(x => x.FullName).IsRequired().HasMaxLength(100);
                b.Property(x => x.PasswordHash).IsRequired().HasMaxLength(500);
                b.Property(x => x.Role).IsRequired().HasMaxLength(50);
                b.Property(x => x.IsDeleted).HasDefaultValue(false);
                b.Property(x => x.IsProfileCompleted).HasDefaultValue(false);
                b.HasIndex(x => x.Phone).IsUnique();
                b.HasIndex(x => x.Email).IsUnique(false);
            });

            // ---------------------------
            // Patient
            // ---------------------------
            modelBuilder.Entity<Patient>(b =>
            {
                b.ToTable("Patients");
                b.HasKey(x => x.PatientId);
                b.HasOne(p => p.User)
                 .WithOne()
                 .HasForeignKey<Patient>(p => p.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ---------------------------
            // Doctor
            // ---------------------------
            modelBuilder.Entity<Doctor>(b =>
            {
                b.ToTable("Doctors");
                b.HasKey(x => x.DoctorId);
                b.HasOne(d => d.User)
                 .WithOne()
                 .HasForeignKey<Doctor>(d => d.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
                b.Property(d => d.Specialty).HasMaxLength(100);
                b.HasIndex(d => d.Specialty);
                b.Property(x => x.RowVersion).IsRowVersion();
            });

            // ---------------------------
            // Staff
            // ---------------------------
            modelBuilder.Entity<Staff>(b =>
            {
                b.ToTable("Staff");
                b.HasKey(x => x.UserId);
                b.HasOne(s => s.User)
                 .WithOne()
                 .HasForeignKey<Staff>(s => s.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
                b.Property(s => s.Position).IsRequired().HasMaxLength(100);
            });

            // ---------------------------
            // Inventory
            // ---------------------------
            modelBuilder.Entity<InventoryItem>(b =>
            {
                b.HasKey(x => x.Id);
                b.HasMany(i => i.Transactions)
                 .WithOne(t => t.InventoryItem)
                 .HasForeignKey(t => t.InventoryItemId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<InventoryTransaction>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Quantity).IsRequired();
            });

            // ---------------------------
            // Appointment
            // ---------------------------
            modelBuilder.Entity<Appointment>(b =>
            {
                b.ToTable("Appointments");
                b.HasKey(x => x.Id);
                b.Property(x => x.StartAt).IsRequired();
                b.Property(x => x.EndAt).IsRequired();
                b.Property(x => x.Status).HasConversion<string>().IsRequired();
                b.Property(x => x.Reason).HasMaxLength(1000);
                b.Property(x => x.Notes).HasMaxLength(2000);
                b.Property(x => x.CreatedAt).IsRequired();
                b.Property(x => x.UpdatedAt);
                b.Property(x => x.RowVersion).IsRowVersion();

                b.HasOne(a => a.Patient)
                 .WithMany(p => p.Appointments)
                 .HasForeignKey(a => a.PatientId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------------------------
            // AppointmentClinicalInfo (new)
            // ---------------------------
            modelBuilder.Entity<AppointmentClinicalInfo>(b =>
            {
                b.ToTable("AppointmentClinicalInfos");
                b.HasKey(x => x.Id);
                b.Property(x => x.AppointmentId).IsRequired();
                b.HasIndex(x => x.AppointmentId).IsUnique();
                b.Property(x => x.DiagnosisSummary).HasColumnType("nvarchar(max)");
                b.Property(x => x.IllnessDescription).HasColumnType("nvarchar(max)");
                // Persist medications as JSON in nvarchar(max)
                b.Property(x => x.MedicationsJson)
                 .HasColumnName("Medications")
                 .HasColumnType("nvarchar(max)");
                b.Property(x => x.DoctorNotes).HasColumnType("nvarchar(max)");
                b.Property(x => x.NextFollowUpDate);
                b.Property(x => x.CreatedAt).IsRequired();
                b.Property(x => x.UpdatedAt);
                b.HasOne(x => x.Appointment)
                 .WithOne(a => a.ClinicalInfo)
                 .HasForeignKey<AppointmentClinicalInfo>(x => x.AppointmentId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ---------------------------
            // FollowUpRecord
            // ---------------------------
            modelBuilder.Entity<FollowUpRecord>(b =>
            {
                b.ToTable("FollowUpRecords");
                b.HasKey(x => x.Id);
                b.Property(x => x.PatientId).IsRequired();
                b.Property(x => x.AppointmentId);
                b.Property(x => x.DoctorId);
                b.Property(x => x.DiagnosisSummary).HasColumnType("nvarchar(max)");
                b.Property(x => x.Notes).HasColumnType("nvarchar(max)");
                b.Property(x => x.PrescriptionId);
                b.Property(x => x.Status).HasConversion<int>().IsRequired();
                b.Property(x => x.CreatedAt).IsRequired();
                b.Property(x => x.UpdatedAt);
                b.Property(x => x.RowVersion).IsRowVersion();

                b.HasIndex(x => x.PatientId);
                b.HasIndex(x => x.AppointmentId);
                // Optional navigation to Appointment
                b.HasOne(x => x.Appointment)
                 .WithMany()
                 .HasForeignKey(x => x.AppointmentId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------------------------
            // FollowUpPrescriptionSnapshot
            // ---------------------------
            modelBuilder.Entity<FollowUpPrescriptionSnapshot>(b =>
            {
                b.ToTable("FollowUpPrescriptionSnapshots");
                b.HasKey(x => x.Id);
                b.Property(x => x.FollowUpRecordId).IsRequired();
                b.Property(x => x.MedicineName).IsRequired().HasMaxLength(500);
                b.Property(x => x.Dosage).HasMaxLength(200);
                b.Property(x => x.Frequency).HasMaxLength(200);
                b.Property(x => x.Duration).HasMaxLength(200);
                b.Property(x => x.Notes).HasColumnType("nvarchar(max)");
                b.Property(x => x.CreatedAt).IsRequired();

                b.HasOne(x => x.FollowUpRecord)
                 .WithMany(r => r.MedicationSnapshots)
                 .HasForeignKey(x => x.FollowUpRecordId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ---------------------------
            // FollowUpTask
            // ---------------------------
            modelBuilder.Entity<FollowUpTask>(b =>
            {
                b.ToTable("FollowUpTasks");
                b.HasKey(x => x.Id);
                b.Property(x => x.FollowUpRecordId).IsRequired();
                b.Property(x => x.TaskType).IsRequired().HasConversion<int>();
                b.Property(x => x.Payload).HasColumnType("nvarchar(max)").IsRequired();
                b.Property(x => x.ScheduledAt).IsRequired();
                b.Property(x => x.AttemptCount).HasDefaultValue(0);
                b.Property(x => x.MaxAttempts).HasDefaultValue(3);
                b.Property(x => x.Status).HasConversion<int>().IsRequired();
                b.Property(x => x.LastAttemptAt);
                b.Property(x => x.ResultMetadata).HasColumnType("nvarchar(max)");
                b.Property(x => x.CreatedAt).IsRequired();
                b.Property(x => x.UpdatedAt);
                b.Property(x => x.RowVersion).IsRowVersion();

                b.HasIndex(x => new { x.ScheduledAt, x.Status });
                b.HasOne(x => x.FollowUpRecord)
                 .WithMany()
                 .HasForeignKey(x => x.FollowUpRecordId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ---------------------------
            // SymptomMapping
            // ---------------------------
            modelBuilder.Entity<SymptomMapping>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Keyword).IsRequired().HasMaxLength(200);
                b.Property(x => x.SuggestedSpecialty).HasMaxLength(200);
                b.Property(x => x.Weight);
                b.Property(x => x.SuggestedDoctorIds).HasConversion(
                    v => string.Join(',', v),
                    v => string.IsNullOrWhiteSpace(v) ? new List<long>() : v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToList()
                );
            });

            // ---------------------------
            // DoctorWorkingHours
            // ---------------------------
            modelBuilder.Entity<DoctorWorkingHours>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.WeeklyHours).HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<DayOfWeek, List<(TimeSpan Start, TimeSpan End)>>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<DayOfWeek, List<(TimeSpan, TimeSpan)>>()
                );
            });

            // ---------------------------
            // ScheduleLock
            // ---------------------------
            modelBuilder.Entity<ScheduleLock>(b =>
            {
                b.HasKey(x => x.DoctorId);
                b.Property(x => x.LockedUntil).IsRequired(false);
            });

            // Add default schema-wide settings if needed (e.g. set delete behavior defaults) - omitted for brevity
            }
        }
    }
