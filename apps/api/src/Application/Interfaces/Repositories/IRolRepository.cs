using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IRolRepository
{
    Task<Rol?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Rol?> GetByNombreAsync(string nombre, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Rol>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken);
    Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken);
    Task AddAsync(Rol rol, CancellationToken cancellationToken);
}
