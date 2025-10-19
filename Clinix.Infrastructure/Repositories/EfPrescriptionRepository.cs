//using Clinix.Application.Interfaces.Functionalities;
//using Clinix.Domain.Entities.FollowUp;
//using Clinix.Infrastructure.Persistence;
//using Microsoft.EntityFrameworkCore;

//namespace Clinix.Infrastructure.Repositories;

///// <summary>
///// EF repository for prescriptions.
///// </summary>
//public class EfPrescriptionRepository : IPrescriptionRepository
//    {
//    private readonly ClinixDbContext _db;

//    public EfPrescriptionRepository(ClinixDbContext db)
//        {
//        _db = db ?? throw new ArgumentNullException(nameof(db));
//        }

//    public async Task<Prescription?> GetByAppointmentIdAsync(long appointmentId, CancellationToken ct = default)
//        {
//        return await _db.Prescriptions
//            .AsNoTracking()
//            .Include(p => p.Medications)
//            .FirstOrDefaultAsync(p => p.AppointmentId == appointmentId, ct);
//        }

//    public async Task<IEnumerable<Prescription>> GetLatestForPatientAsync(long patientId, int limit = 5, CancellationToken ct = default)
//        {
//        return await _db.Prescriptions
//            .AsNoTracking()
//            .Where(p => p.PatientId == patientId)
//            .OrderByDescending(p => p.CreatedAtUtc)
//            .Take(limit)
//            .Include(p => p.Medications)
//            .ToListAsync(ct);
//        }
//    }

