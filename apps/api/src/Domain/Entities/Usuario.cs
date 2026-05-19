using Domain.Common;

namespace Domain.Entities;

public sealed class Usuario : BaseEntity
{
    private readonly List<Rol> _roles = [];

    private Usuario()
    {
    }

    public Usuario(string email, string passwordHash, string salt)
    {
        SetEmail(email);
        SetPassword(passwordHash, salt);
        Activo = true;
    }

    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Salt { get; private set; } = string.Empty;
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiryTime { get; private set; }
    public bool Activo { get; private set; }
    public IReadOnlyCollection<Rol> Roles => _roles.AsReadOnly();

    public void ChangePassword(string passwordHash, string salt)
    {
        SetPassword(passwordHash, salt);
        MarkAsUpdated();
    }

    public void SetRefreshToken(string refreshToken, DateTime expiryTime)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);

        RefreshToken = refreshToken;
        RefreshTokenExpiryTime = expiryTime;
        MarkAsUpdated();
    }

    public void ClearRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiryTime = null;
        MarkAsUpdated();
    }

    public void AssignRoles(IEnumerable<Rol> roles)
    {
        var roleList = roles.ToArray();
        if (roleList.Length == 0)
        {
            throw new ArgumentException("At least one role is required.", nameof(roles));
        }

        _roles.Clear();
        _roles.AddRange(roleList);
        MarkAsUpdated();
    }

    public void Deactivate()
    {
        Activo = false;
        ClearRefreshToken();
    }

    private void SetEmail(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        if (email.Length > 256)
        {
            throw new ArgumentException("Email cannot exceed 256 characters.", nameof(email));
        }

        Email = email.Trim().ToLowerInvariant();
    }

    private void SetPassword(string passwordHash, string salt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(salt);

        PasswordHash = passwordHash;
        Salt = salt;
    }
}
