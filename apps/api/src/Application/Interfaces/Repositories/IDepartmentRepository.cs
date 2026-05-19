using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IDepartmentRepository
{
    Task<IReadOnlyCollection<Department>> GetAllAsync(CancellationToken cancellationToken);
    Task<Department?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Department>> GetByCountryIdAsync(int countryId, CancellationToken cancellationToken);
    Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken);
    Task<bool> ExistsByNameForCountryAsync(string name, int countryId, int? excludedId, CancellationToken cancellationToken);
    Task AddAsync(Department department, CancellationToken cancellationToken);
}
