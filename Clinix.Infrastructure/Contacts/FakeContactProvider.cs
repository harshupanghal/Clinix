using Clinix.Application.Interfaces;

namespace Clinix.Infrastructure.Contacts;

public sealed class FakeContactProvider : IContactProvider
    {
    public Task<(string? Email, string? Phone)> GetPatientContactAsync(long patientId, CancellationToken ct = default)
        => Task.FromResult<(string?, string?)>(($"patient-{patientId}@example.test", "+10000000000"));
    }
