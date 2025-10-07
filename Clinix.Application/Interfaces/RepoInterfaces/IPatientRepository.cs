using Clinix.Domain.Entities.ApplicationUsers;

namespace Clinix.Application.Interfaces.RepoInterfaces;
    public interface IPatientRepository
        {
        Task AddAsync(Patient patient, CancellationToken ct = default);
        }
    
