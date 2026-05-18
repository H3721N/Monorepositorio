using Domain.Common;

namespace Domain.Entities;

public sealed class Rol : BaseEntity
{
    private readonly List<Usuario> _usuarios = [];

    private Rol()
    {
    }

    public Rol(string nombre)
    {
        SetNombre(nombre);
    }

    public string Nombre { get; private set; } = string.Empty;
    public IReadOnlyCollection<Usuario> Usuarios => _usuarios.AsReadOnly();

    private void SetNombre(string nombre)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);

        if (nombre.Length > 50)
        {
            throw new ArgumentException("Role name cannot exceed 50 characters.", nameof(nombre));
        }

        Nombre = nombre.Trim().ToUpperInvariant();
    }
}
