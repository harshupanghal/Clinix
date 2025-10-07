using Clinix.Domain.Entities.ApplicationUsers;

namespace Clinix.Application.Interfaces.ServiceInterfaces;

public interface IDoctorMatchingService
    {
    Task<IEnumerable<Doctor>> SuggestDoctorsAsync(string reason, CancellationToken ct);
    }