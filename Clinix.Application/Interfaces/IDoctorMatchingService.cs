using Clinix.Domain.Entities;


namespace Clinix.Application.Interfaces;


public interface IDoctorMatchingService
    {
    Task<IEnumerable<Doctor>> SuggestDoctorsAsync(string reason, CancellationToken ct);
    }