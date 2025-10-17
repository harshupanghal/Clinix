using Clinix.Application.Interfaces.Functionalities;
using Clinix.Domain.Entities.Appointments;
using Clinix.Domain.Enums;
using Clinix.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clinix.Infrastructure.Repositories;

public class EfAppointmentRepository : IAppointmentRepository
    {
    private readonly ClinixDbContext _db;

    public EfAppointmentRepository(ClinixDbContext db) => _db = db;

    public async Task AddAsync(Appointment appointment)
        {
        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync();
        }

    public async Task DeleteAsync(long id)
        {
        var exist = await _db.Appointments.FindAsync(id);
        if (exist is null) return;
        _db.Appointments.Remove(exist);
        await _db.SaveChangesAsync();
        }

    public async Task<Appointment?> GetByIdAsync(long id) => await _db.Appointments.FindAsync(id);

    public async Task<IEnumerable<Appointment>> GetAppointmentsForPatientAsync(long patientId)
        {
        return await _db.Appointments
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .Where(a => a.PatientId == patientId)
            .OrderBy(a => a.StartAt)
            .ToListAsync();
        }
    public async Task<List<Appointment>> GetAppointmentsForDoctorInRangeAsync(long doctorId, DateTimeOffset rangeStart, DateTimeOffset rangeEnd)
        {
        return await _db.Appointments
            .Where(a => a.DoctorId == doctorId && a.StartAt < rangeEnd && a.EndAt > rangeStart && a.Status != AppointmentStatus.Cancelled)
            .OrderBy(a => a.StartAt)
            .ToListAsync();
        }

    public async Task<List<Appointment>> GetUpcomingAppointmentsForDoctorAsync(long doctorId, DateTimeOffset from)
        {
        return await _db.Appointments
            .Include(a => a.Patient)
                .ThenInclude(p => p.User)  // eager load the User through Patient
            .Where(a => a.DoctorId == doctorId && a.StartAt >= from && a.Status != AppointmentStatus.Cancelled)
            .OrderBy(a => a.StartAt)
            .ToListAsync();
        }


    public async Task UpdateAsync(Appointment appointment)
        {
        _db.Appointments.Update(appointment);
        await _db.SaveChangesAsync();
        }
    }
