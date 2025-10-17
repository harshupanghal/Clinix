using Clinix.Domain.Entities.ApplicationUsers;
using Clinix.Domain.Entities.Appointments;

namespace Clinix.Application.Interfaces.Functionalities;

public interface IDoctorScheduleRepository
    {
    Task<Doctor?> GetDoctorAsync(long doctorId);
    Task<DoctorWorkingHours?> GetDoctorWorkingHoursAsync(long doctorId);

    /// <summary>
    /// Acquire an application-level lock / concurrency token for schedule operations.
    /// Implementation detail: for SQL this may map to a RowVersion or explicit DB lock.
    /// </summary>
    Task<bool> TryAcquireScheduleLockAsync(long doctorId, TimeSpan lockTimeout);
    Task ReleaseScheduleLockAsync(long doctorId);
    }

