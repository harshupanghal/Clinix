using Clinix.Application.Interfaces.RepoInterfaces;
using Clinix.Domain.Entities.ApplicationUsers;
using Clinix.Domain.Entities.Appointments;
using Clinix.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clinix.Infrastructure.Repositories;

public class AppointmentRepository : IAppointmentRepository
    {
    private readonly ClinixDbContext _db;


    public AppointmentRepository(ClinixDbContext db) => _db = db;


    public async Task<AppointmentSlot?> GetSlotByIdAsync(int id, CancellationToken ct)
    => await _db.AppointmentSlots.Include(s => s.Doctor).SingleOrDefaultAsync(s => s.Id == id, ct);


    public async Task<List<AppointmentSlot>> GetAvailableSlotsAsync(int doctorId, DateTime fromUtc, DateTime toUtc, CancellationToken ct)
    => await _db.AppointmentSlots
    .Where(s => s.DoctorId == doctorId && s.Status == SlotStatus.Available && s.StartUtc >= fromUtc && s.StartUtc < toUtc)
    .OrderBy(s => s.StartUtc)
    .ToListAsync(ct);


    public async Task<BookSlotResult> TryBookSlotAsync(int slotId, Appointment appt, CancellationToken ct)
        {
        // Use transaction + concurrency handling
        using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
            {
            var slot = await _db.AppointmentSlots.Where(s => s.Id == slotId).SingleOrDefaultAsync(ct);
            if (slot == null) return new(false, "Slot not found", null);
            if (slot.Status != SlotStatus.Available) return new(false, "Slot not available", null);


            slot.Status = SlotStatus.Booked;
            _db.AppointmentSlots.Update(slot);
            await _db.SaveChangesAsync(ct);


            appt.AppointmentSlotId = slot.Id;
            appt.DoctorId = slot.DoctorId;
            appt.CreatedAt = DateTime.UtcNow;
            appt.Status = AppointmentStatus.Confirmed;


            await _db.Appointments.AddAsync(appt, ct);
            await _db.SaveChangesAsync(ct);


            await tx.CommitAsync(ct);
            return new(true, null, appt);
            }
        catch (DbUpdateConcurrencyException ex)
            {
            return new(false, "Slot was taken by another user", null);
            }
        catch (Exception ex)
            {
            return new(false, ex.Message, null);
            }
        }


    public async Task<List<Doctor>> GetDoctorsBySpecialtyAsync(string specialty, CancellationToken ct)
    => await _db.Doctors.Where(d => d.Specialty == specialty).ToListAsync(ct);


    public async Task<Doctor?> GetDoctorByIdAsync(int id, CancellationToken ct)
    => await _db.Doctors.FindAsync(new object[] { id }, ct);


    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
    }