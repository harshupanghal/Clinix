using System;
using Clinix.Domain.Entities;
using Clinix.Domain.Entities.ApplicationUsers;
using Clinix.Domain.Entities.Appointments;
using Clinix.Domain.Entities.FollowUps;
using Clinix.Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Clinix.Infrastructure.Persistence;

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
    public DbSet<SymptomMapping> SymptomMappings => Set<SymptomMapping>();
    public DbSet<DoctorWorkingHours> DoctorWorkingHours => Set<DoctorWorkingHours>();
    public DbSet<ScheduleLock> ScheduleLocks => Set<ScheduleLock>();



    protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        base.OnModelCreating(modelBuilder);

        // User
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

            // Indexes
            b.HasIndex(x => x.Phone).IsUnique();
            b.HasIndex(x => x.Email).IsUnique(false); // email uniqueness optional; set true if you want unique email constraint
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
            b.Property(x => x.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<Appointment>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.StartAt).IsRequired();
            b.Property(x => x.EndAt).IsRequired();
            b.Property(x => x.Status).HasConversion<string>().IsRequired();
            b.Property(x => x.Reason).HasMaxLength(1000);
            b.Property(x => x.Notes).HasMaxLength(2000);
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt);
            b.Property(x => x.RowVersion).IsRowVersion();

            // ⚠️ Explicitly set ON DELETE to Restrict
            b.HasOne(a => a.Patient)
             .WithMany(p => p.Appointments)
             .HasForeignKey(a => a.PatientId)
             .OnDelete(DeleteBehavior.Restrict);  // ✅ this is key
        });




        // Fix for CS0029 and CS1662 in SymptomMapping conversion
        modelBuilder.Entity<SymptomMapping>(b =>
{
    b.HasKey(x => x.Id);
    b.Property(x => x.Keyword).IsRequired().HasMaxLength(200);
    b.Property(x => x.SuggestedSpecialty).HasMaxLength(200);
    b.Property(x => x.Weight);
    b.Property(x => x.SuggestedDoctorIds).HasConversion(
        v => string.Join(',', v),
        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToList()
    );
});


        modelBuilder.Entity<ScheduleLock>(b =>
        {
            b.HasKey(x => x.DoctorId);
            b.Property(x => x.LockedUntil).IsRequired(false);

            modelBuilder.Entity<DoctorWorkingHours>(b =>
        {
            b.HasKey(x => x.Id);
            // We'll store WeeklyHours as JSON
            b.Property(x => x.WeeklyHours).HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<DayOfWeek, List<(TimeSpan Start, TimeSpan End)>>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<DayOfWeek, List<(TimeSpan, TimeSpan)>>()
            );
        });
            //});

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

            // Inventory relations
            modelBuilder.Entity<InventoryItem>()
             .HasMany(i => i.Transactions)
             .WithOne(t => t.InventoryItem)
             .HasForeignKey(t => t.InventoryItemId);
        });
    }
    }


