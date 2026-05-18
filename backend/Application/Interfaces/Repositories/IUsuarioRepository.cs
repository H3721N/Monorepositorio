using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Usuario?> GetByIdWithRolesAsync(int id, CancellationToken cancellationToken);
    Task<Usuario?> GetByEmailWithRolesAsync(string email, CancellationToken cancellationToken);
    Task<Usuario?> GetByRefreshTokenWithRolesAsync(string refreshToken, CancellationToken cancellationToken);
    Task<bool> ExistsByEmailAsync(string email, int? excludedId, CancellationToken cancellationToken);
    Task AddAsync(Usuario usuario, CancellationToken cancellationToken);
}
