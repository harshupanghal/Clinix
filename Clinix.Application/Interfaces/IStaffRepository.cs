using Clinix.Domain.Entities;

namespace Clinix.Application.Interfaces;

public interface IStaffRepository
    {
    Task AddAsync(Staff staff, CancellationToken ct = default);
    }

