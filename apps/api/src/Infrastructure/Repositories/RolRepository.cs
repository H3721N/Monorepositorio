using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class RolRepository : IRolRepository
{
    private readonly AppDbContext _context;

    public RolRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Rol?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Roles.FirstOrDefaultAsync(rol => rol.Id == id, cancellationToken);
    }

    public async Task<Rol?> GetByNombreAsync(string nombre, CancellationToken cancellationToken)
    {
        var normalizedNombre = nombre.Trim().ToUpper();
        return await _context.Roles.FirstOrDefaultAsync(rol => rol.Nombre == normalizedNombre, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Rol>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken)
    {
        var distinctIds = ids.Distinct().ToArray();

        return await _context.Roles
            .Where(rol => distinctIds.Contains(rol.Id))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Roles.AnyAsync(rol => rol.Id == id, cancellationToken);
    }

    public async Task AddAsync(Rol rol, CancellationToken cancellationToken)
    {
        await _context.Roles.AddAsync(rol, cancellationToken);
    }
}
