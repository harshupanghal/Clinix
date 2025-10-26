using Clinix.Domain.Entities;
using Clinix.Domain.Entities.ApplicationUsers;
using Clinix.Domain.Entities.Inventory;
using Clinix.Domain.Entities.System;
using Microsoft.EntityFrameworkCore;

namespace Clinix.Infrastructure.Persistence;

public class ClinixDbContext : DbContext
    {
    public ClinixDbContext(DbContextOptions<ClinixDbContext> options) : base(options) { }

    // Users & roles
    public DbSet<User> Users => Set<User>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Staff> Staffs => Set<Staff>();

    // Inventory
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();

    // others
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<DoctorSchedule> DoctorSchedules => Set<DoctorSchedule>();
    public DbSet<SymptomKeyword> SymptomKeywords => Set<SymptomKeyword>();
    public DbSet<FollowUp> FollowUps => Set<FollowUp>();
    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<SeedStatus> SeedStatuses { get; set; }
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        base.OnModelCreating(modelBuilder);

        
        // Users
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
        });

        // Patients
        modelBuilder.Entity<Patient>(b =>
        {
            b.ToTable("Patients");
            b.HasKey(x => x.PatientId);

            b.HasOne(p => p.User)
                .WithOne()
                .HasForeignKey<Patient>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Property(x => x.MedicalRecordNumber).HasMaxLength(50);
            b.Property(x => x.BloodGroup).HasMaxLength(5);
            b.Property(x => x.Gender).HasMaxLength(20);
            b.Property(x => x.EmergencyContactNumber).HasMaxLength(30);
        });

        // Doctors
        modelBuilder.Entity<Doctor>(b =>
        {
            b.ToTable("Doctors");
            b.HasKey(x => x.DoctorId);

            b.HasOne(d => d.User)
                .WithOne()
                .HasForeignKey<Doctor>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(d => d.ProviderId); 

            b.Property(d => d.Specialty).HasMaxLength(100);
            b.HasIndex(d => d.Specialty);
            b.Property(x => x.RowVersion).IsRowVersion();
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

        // DoctorSchedule
        modelBuilder.Entity<DoctorSchedule>(b =>
        {
            b.ToTable("DoctorSchedules");
            b.HasKey(x => x.Id);
            b.HasOne(s => s.Doctor)
                .WithMany(d => d.Schedules)
                .HasForeignKey(s => s.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);
            b.Property(s => s.DayOfWeek).IsRequired();
            b.Property(s => s.StartTime).IsRequired();
            b.Property(s => s.EndTime).IsRequired();
            b.Property(s => s.IsAvailable).HasDefaultValue(true);
        });

        // Provider
        modelBuilder.Entity<Provider>(b =>
        {
            b.ToTable("Providers");
            b.HasKey(p => p.Id);
            b.Property(p => p.Name).IsRequired().HasMaxLength(200);
            b.Property(p => p.Specialty).IsRequired().HasMaxLength(100);
            b.Property(p => p.Tags).HasMaxLength(1000);
            b.Property(p => p.WorkStartTime).HasColumnType("datetime2");
            b.Property(p => p.WorkEndTime).HasColumnType("datetime2");
            b.HasIndex(p => p.Specialty);
        });

        // Appointment
        modelBuilder.Entity<Appointment>(b =>
        {
            b.ToTable("Appointments");
            b.HasKey(a => a.Id);
            b.Property(a => a.PatientId).IsRequired();
            b.Property(a => a.ProviderId).IsRequired();
            b.Property(a => a.Type).IsRequired();
            b.Property(a => a.Status).IsRequired();
            b.Property(a => a.CreatedAt).IsRequired();
            b.OwnsOne(a => a.When, when =>
            {
                when.Property(p => p.Start).HasColumnName("Start").HasColumnType("datetimeoffset");
                when.Property(p => p.End).HasColumnName("End").HasColumnType("datetimeoffset");
            });
            b.HasMany(a => a.FollowUps)
             .WithOne()
             .HasForeignKey(f => f.AppointmentId)
             .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(a => a.ProviderId);
            b.HasIndex(a => a.PatientId);
            b.HasIndex(a => new { a.ProviderId, a.Status });
        });

        // FollowUp
        modelBuilder.Entity<FollowUp>(b =>
        {
            b.ToTable("FollowUps");
            b.HasKey(f => f.Id);
            b.Property(f => f.AppointmentId).IsRequired();
            b.Property(f => f.DueBy).HasColumnType("datetimeoffset");
            b.Property(f => f.LastRemindedAt).HasColumnType("datetimeoffset");
            b.Property(f => f.Status).IsRequired();
            b.Property(f => f.CreatedAt).IsRequired();
            b.HasIndex(f => f.AppointmentId);
            b.HasIndex(f => new { f.Status, f.DueBy });
        });

        // SymptomKeyword
        modelBuilder.Entity<SymptomKeyword>(b =>
        {
            b.ToTable("SymptomKeywords");
            b.HasKey(x => x.Id);
            b.Property(x => x.Keyword).IsRequired().HasMaxLength(100);
            b.Property(x => x.Specialty).IsRequired().HasMaxLength(100);
            b.HasIndex(x => x.Keyword);
        });

        // OutboxMessage
        modelBuilder.Entity<OutboxMessage>(b =>
        {
            b.ToTable("OutboxMessages");
            b.HasKey(x => x.Id);
            b.Property(x => x.Type).IsRequired().HasMaxLength(100);
            b.Property(x => x.PayloadJson).IsRequired();
            b.HasIndex(x => new { x.Processed, x.OccurredAtUtc });
        });

        // Inventory
        modelBuilder.Entity<InventoryItem>(b =>
        {
            b.ToTable("InventoryItems");
            b.HasKey(x => x.Id);
            b.HasMany(i => i.Transactions)
                .WithOne(t => t.InventoryItem)
                .HasForeignKey(t => t.InventoryItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InventoryTransaction>(b =>
        {
            b.ToTable("InventoryTransactions");
            b.HasKey(x => x.Id);
            b.Property(x => x.Quantity).IsRequired();
        });

        modelBuilder.Entity<SeedStatus>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.SeedName, e.Version }).IsUnique();
            entity.Property(e => e.SeedName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Version).IsRequired().HasMaxLength(20);
            entity.Property(e => e.ExecutedBy).HasMaxLength(100);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
        });
        }
    }
