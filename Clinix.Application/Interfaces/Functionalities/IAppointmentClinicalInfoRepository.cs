using System.Threading.Tasks;
using Clinix.Domain.Entities.Appointments;

namespace Clinix.Application.Interfaces.Functionalities;

public interface IAppointmentClinicalInfoRepository
    {
    Task<AppointmentClinicalInfo?> GetByAppointmentIdAsync(long appointmentId);
    Task AddOrUpdateAsync(AppointmentClinicalInfo clinicalInfo); // used by Doctors UI later
    }

