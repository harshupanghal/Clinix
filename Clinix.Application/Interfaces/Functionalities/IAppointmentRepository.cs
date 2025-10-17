using Clinix.Domain.Entities.Appointments;

namespace Clinix.Application.Interfaces.Functionalities;

public interface IAppointmentRepository
    {
    Task<Appointment?> GetByIdAsync(long id);
    Task<List<Appointment>> GetAppointmentsForDoctorInRangeAsync(long doctorId, DateTimeOffset rangeStart, DateTimeOffset rangeEnd);
    Task<List<Appointment>> GetUpcomingAppointmentsForDoctorAsync(long doctorId, DateTimeOffset from);
    Task<IEnumerable<Appointment>> GetAppointmentsForPatientAsync(long patientId);
    Task AddAsync(Appointment appointment);
    Task UpdateAsync(Appointment appointment);
    Task DeleteAsync(long id);
    }
