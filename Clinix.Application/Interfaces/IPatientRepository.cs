using Clinix.Domain.Entities;

namespace Clinix.Application.Interfaces;

    public interface IPatientRepository
        {
        Task AddAsync(Patient patient, CancellationToken ct = default);
        }
    
