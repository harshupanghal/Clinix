using Clinix.Domain.Entities.ApplicationUsers;

namespace Clinix.Application.Interfaces.UserRepo;

public interface IDoctorRepository
    {
    Task AddAsync(Doctor doctor, CancellationToken ct = default);
    }

