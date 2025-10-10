using Clinix.Domain.Entities.ApplicationUsers;

namespace Clinix.Application.Interfaces.UserRepo;

public interface IStaffRepository
    {
    Task AddAsync(Staff staff, CancellationToken ct = default);
    }

