using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface ICountryRepository
{
    Task<IReadOnlyCollection<Country>> GetAllAsync(CancellationToken cancellationToken);
    Task<Country?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Country?> GetByIdWithDepartmentsAsync(int id, CancellationToken cancellationToken);
    Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken);
    Task<bool> ExistsByNameAsync(string name, int? excludedId, CancellationToken cancellationToken);
    Task<bool> ExistsByIsoCodeAsync(string isoCode, int? excludedId, CancellationToken cancellationToken);
    Task AddAsync(Country country, CancellationToken cancellationToken);
}
