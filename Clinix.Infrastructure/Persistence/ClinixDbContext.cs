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
        public DbSet<AppointmentSlot> AppointmentSlots => Set<AppointmentSlot>();
        //public DbSet<SymptomSpecialtyMap> SymptomSpecialtyMaps => Set<SymptomSpecialtyMap>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>(b =>
            {
                b.ToTable("Users");
                b.HasKey(x => x.Id);
                b.HasIndex(x => x.Email).IsUnique();
                b.Property(x => x.Email).HasMaxLength(320);
                b.Property(x => x.Phone).IsRequired().HasMaxLength(13);
                b.Property(x => x.FullName).IsRequired().HasMaxLength(100);
                b.Property(x => x.PasswordHash).IsRequired().HasMaxLength(500);
                b.Property(x => x.Role).IsRequired().HasMaxLength(50);
                b.Property(x => x.IsDeleted).HasDefaultValue(false);
            });

            // Patient
            modelBuilder.Entity<Patient>(b =>
            {
                b.ToTable("Patients");
                b.HasKey(x => x.PatientId);
                b.HasOne(p => p.User)
                 .WithOne()
                 .HasForeignKey<Patient>(p => p.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // Doctor
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
            });

            // AppointmentSlot
            modelBuilder.Entity<AppointmentSlot>(b =>
            {
                b.ToTable("AppointmentSlots");
                b.HasKey(s => s.Id);
                b.Property(s => s.StartUtc).IsRequired();
                b.Property(s => s.EndUtc).IsRequired();
                b.Property(s => s.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
                b.Property(s => s.RowVersion).IsRowVersion();
                b.HasOne(s => s.Doctor)
                 .WithMany(d => d.Slots)
                 .HasForeignKey(s => s.DoctorId)
                 .OnDelete(DeleteBehavior.Cascade);
                b.HasIndex(s => new { s.DoctorId, s.StartUtc });
            });

            // Appointment
            modelBuilder.Entity<Appointment>(b =>
            {
                b.ToTable("Appointments");
                b.HasKey(a => a.Id);
                b.Property(a => a.Reason).HasMaxLength(2000);
                b.Property(a => a.Status).HasMaxLength(50).IsRequired();
                b.Property(a => a.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                b.Property(a => a.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

                b.HasOne(a => a.AppointmentSlot)
                 .WithOne(s => s.Appointment)
                 .HasForeignKey<Appointment>(a => a.AppointmentSlotId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(a => a.Doctor)
                 .WithMany(d => d.Appointments)
                 .HasForeignKey(a => a.DoctorId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(a => a.Patient)
                 .WithMany(p => p.Appointments)
                 .HasForeignKey(a => a.PatientId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // SymptomSpecialtyMap
            modelBuilder.Entity<SymptomSpecialtyMap>(b =>
            {
                b.ToTable("SymptomSpecialtyMaps");
                b.HasKey(s => s.Id);
                b.Property(s => s.Keyword).IsRequired().HasMaxLength(200);
                b.Property(s => s.Specialty).IsRequired().HasMaxLength(100);
                b.HasIndex(s => s.Keyword);
            });

            // Staff
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

            // Inventory
            modelBuilder.Entity<InventoryItem>()
             .HasMany(i => i.Transactions)
             .WithOne(t => t.InventoryItem)
             .HasForeignKey(t => t.InventoryItemId);
            }
        }
    }
