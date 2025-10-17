using Clinix.Domain.Entities.Appointments;

namespace Clinix.Application.Interfaces.Functionalities;

public interface ISymptomMappingRepository
    {
    Task<IEnumerable<SymptomMapping>> GetAllMappingsAsync();
    Task<List<SymptomMapping>> SearchByKeywordsAsync(IEnumerable<string> keywords);
    Task AddOrUpdateAsync(SymptomMapping mapping);
    Task DeleteAsync(long id);
    }

