using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _context;

    public UsuarioRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Usuarios.FirstOrDefaultAsync(usuario => usuario.Id == id, cancellationToken);
    }

    public async Task<Usuario?> GetByIdWithRolesAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Usuarios
            .Include(usuario => usuario.Roles)
            .FirstOrDefaultAsync(usuario => usuario.Id == id, cancellationToken);
    }

    public async Task<Usuario?> GetByEmailWithRolesAsync(string email, CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLower();

        return await _context.Usuarios
            .Include(usuario => usuario.Roles)
            .FirstOrDefaultAsync(usuario => usuario.Email == normalizedEmail, cancellationToken);
    }

    public async Task<Usuario?> GetByRefreshTokenWithRolesAsync(string refreshToken, CancellationToken cancellationToken)
    {
        return await _context.Usuarios
            .Include(usuario => usuario.Roles)
            .FirstOrDefaultAsync(usuario => usuario.RefreshToken == refreshToken, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, int? excludedId, CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLower();

        return await _context.Usuarios.AnyAsync(usuario =>
            usuario.Email == normalizedEmail &&
            (!excludedId.HasValue || usuario.Id != excludedId.Value),
            cancellationToken);
    }

    public async Task AddAsync(Usuario usuario, CancellationToken cancellationToken)
    {
        await _context.Usuarios.AddAsync(usuario, cancellationToken);
    }
}
