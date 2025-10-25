// Domain/Interfaces/Repositories.cs
namespace Clinix.Domain.Interfaces;

using Clinix.Domain.Entities;

public interface IAppointmentRepository
    {
    Task<Appointment?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<List<Appointment>> GetByPatientAsync(long patientId, CancellationToken ct = default);
    Task<List<Appointment>> GetByProviderAsync(long providerId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default);
    Task AddAsync(Appointment appointment, CancellationToken ct = default);
    Task UpdateAsync(Appointment appointment, CancellationToken ct = default);
    }

public interface IFollowUpRepository
    {
    Task<FollowUp?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<List<FollowUp>> GetByAppointmentAsync(long appointmentId, CancellationToken ct = default);
    Task<List<FollowUp>> GetPendingDueAsync(DateTimeOffset upTo, CancellationToken ct = default);
    Task AddAsync(FollowUp followUp, CancellationToken ct = default);
    Task UpdateAsync(FollowUp followUp, CancellationToken ct = default);
    }

public interface IProviderRepository
    {
    Task<Provider?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<List<Provider>> SearchAsync(string[] keywords, CancellationToken ct = default);
    Task AddAsync(Provider provider, CancellationToken ct = default);
    Task UpdateAsync(Provider provider, CancellationToken ct = default);
    }

