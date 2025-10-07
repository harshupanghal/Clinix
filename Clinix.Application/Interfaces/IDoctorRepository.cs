using Clinix.Domain.Entities;


namespace Clinix.Application.Interfaces;

public interface IDoctorRepository
    {
    Task AddAsync(Doctor doctor, CancellationToken ct = default);
    }

