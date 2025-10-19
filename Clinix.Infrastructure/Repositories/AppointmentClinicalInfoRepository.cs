using Clinix.Application.Interfaces.Functionalities;
using Clinix.Domain.Entities.Appointments;
using Clinix.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clinix.Infrastructure.Repositories;

public class AppointmentClinicalInfoRepository : IAppointmentClinicalInfoRepository
    {
    private readonly ClinixDbContext _db;
    public AppointmentClinicalInfoRepository(ClinixDbContext db) => _db = db;

    public async Task<AppointmentClinicalInfo?> GetByAppointmentIdAsync(long appointmentId)
        {
        var entity = await _db.AppointmentClinicalInfos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.AppointmentId == appointmentId);
        if (entity == null) return null;

        // hydrate Medications list from JSON
        entity.Medications = AppointmentClinicalInfo.DeserializeMedications(entity.MedicationsJson);
        return entity;
        }

    public async Task AddOrUpdateAsync(AppointmentClinicalInfo clinicalInfo)
        {
        if (clinicalInfo == null) throw new ArgumentNullException(nameof(clinicalInfo));

        // keep MedicationsJson in sync
        clinicalInfo.GetType().GetProperty("MedicationsJson")!.SetValue(clinicalInfo, System.Text.Json.JsonSerializer.Serialize(clinicalInfo.Medications ?? new List<MedicationItem>()));

        var existing = await _db.AppointmentClinicalInfos.FirstOrDefaultAsync(x => x.AppointmentId == clinicalInfo.AppointmentId);
        if (existing == null)
            {
            await _db.AppointmentClinicalInfos.AddAsync(clinicalInfo);
            }
        else
            {
            existing.Update(clinicalInfo.DiagnosisSummary, clinicalInfo.IllnessDescription, clinicalInfo.Medications, clinicalInfo.DoctorNotes, clinicalInfo.NextFollowUpDate);
            _db.AppointmentClinicalInfos.Update(existing);
            }

        await _db.SaveChangesAsync();
        }
    }

