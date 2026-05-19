using Application.Interfaces.Auth;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class DatabaseInitializer : IDatabaseInitializer
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public DatabaseInitializer(AppDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (await HasLegacySingleRoleSchemaAsync(cancellationToken))
        {
            await MigrateLegacySingleRoleSchemaAsync(cancellationToken);
        }
        else
        {
            await _context.Database.EnsureCreatedAsync(cancellationToken);
        }

        await EnsureAuthTablesAsync(cancellationToken);
        await SeedRolesAndUsersAsync(cancellationToken);
    }

    private async Task<bool> HasLegacySingleRoleSchemaAsync(CancellationToken cancellationToken)
    {
        var connection = _context.Database.GetDbConnection();

        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Usuarios') WHERE name = 'RolId';";

        var result = await command.ExecuteScalarAsync(cancellationToken);
        var hasLegacySchema = Convert.ToInt32(result) > 0;

        await connection.CloseAsync();

        return hasLegacySchema;
    }

    private async Task MigrateLegacySingleRoleSchemaAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            PRAGMA foreign_keys = OFF;

            CREATE TABLE IF NOT EXISTS "Usuarios_new" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_Usuarios" PRIMARY KEY AUTOINCREMENT,
                "Email" TEXT NOT NULL,
                "PasswordHash" TEXT NOT NULL,
                "Salt" TEXT NOT NULL,
                "RefreshToken" TEXT NULL,
                "RefreshTokenExpiryTime" TEXT NULL,
                "Activo" INTEGER NOT NULL,
                "IsDeleted" INTEGER NOT NULL,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NULL,
                "DeletedAtUtc" TEXT NULL
            );

            INSERT INTO "Usuarios_new" (
                "Id",
                "Email",
                "PasswordHash",
                "Salt",
                "RefreshToken",
                "RefreshTokenExpiryTime",
                "Activo",
                "IsDeleted",
                "CreatedAtUtc",
                "UpdatedAtUtc",
                "DeletedAtUtc"
            )
            SELECT
                "Id",
                "Email",
                "PasswordHash",
                "Salt",
                "RefreshToken",
                "RefreshTokenExpiryTime",
                "Activo",
                "IsDeleted",
                "CreatedAtUtc",
                "UpdatedAtUtc",
                "DeletedAtUtc"
            FROM "Usuarios";

            DROP TABLE "Usuarios";
            ALTER TABLE "Usuarios_new" RENAME TO "Usuarios";
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_Usuarios_Email" ON "Usuarios" ("Email");

            CREATE TABLE IF NOT EXISTS "UsuarioRoles" (
                "UsuarioId" INTEGER NOT NULL,
                "RolId" INTEGER NOT NULL,
                CONSTRAINT "PK_UsuarioRoles" PRIMARY KEY ("UsuarioId", "RolId"),
                CONSTRAINT "FK_UsuarioRoles_Usuarios_UsuarioId" FOREIGN KEY ("UsuarioId") REFERENCES "Usuarios" ("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_UsuarioRoles_Roles_RolId" FOREIGN KEY ("RolId") REFERENCES "Roles" ("Id") ON DELETE CASCADE
            );

            CREATE INDEX IF NOT EXISTS "IX_UsuarioRoles_RolId" ON "UsuarioRoles" ("RolId");
            DELETE FROM "Roles" WHERE "Nombre" IN ('admin', 'backend', 'frontend');

            PRAGMA foreign_keys = ON;
            """;

        await _context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    private async Task EnsureAuthTablesAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS "Roles" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_Roles" PRIMARY KEY AUTOINCREMENT,
                "Nombre" TEXT NOT NULL,
                "IsDeleted" INTEGER NOT NULL,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NULL,
                "DeletedAtUtc" TEXT NULL
            );

            CREATE UNIQUE INDEX IF NOT EXISTS "IX_Roles_Nombre" ON "Roles" ("Nombre");

            CREATE TABLE IF NOT EXISTS "Usuarios" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_Usuarios" PRIMARY KEY AUTOINCREMENT,
                "Email" TEXT NOT NULL,
                "PasswordHash" TEXT NOT NULL,
                "Salt" TEXT NOT NULL,
                "RefreshToken" TEXT NULL,
                "RefreshTokenExpiryTime" TEXT NULL,
                "Activo" INTEGER NOT NULL,
                "IsDeleted" INTEGER NOT NULL,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NULL,
                "DeletedAtUtc" TEXT NULL
            );

            CREATE UNIQUE INDEX IF NOT EXISTS "IX_Usuarios_Email" ON "Usuarios" ("Email");

            CREATE TABLE IF NOT EXISTS "UsuarioRoles" (
                "UsuarioId" INTEGER NOT NULL,
                "RolId" INTEGER NOT NULL,
                CONSTRAINT "PK_UsuarioRoles" PRIMARY KEY ("UsuarioId", "RolId"),
                CONSTRAINT "FK_UsuarioRoles_Usuarios_UsuarioId" FOREIGN KEY ("UsuarioId") REFERENCES "Usuarios" ("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_UsuarioRoles_Roles_RolId" FOREIGN KEY ("RolId") REFERENCES "Roles" ("Id") ON DELETE CASCADE
            );

            CREATE INDEX IF NOT EXISTS "IX_UsuarioRoles_RolId" ON "UsuarioRoles" ("RolId");
            """;

        await _context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    private async Task SeedRolesAndUsersAsync(CancellationToken cancellationToken)
    {
        var roles = new[] { "COUNTRY", "DEPARTMENT", "USER_ADMIN" };

        foreach (var roleName in roles)
        {
            var exists = await _context.Roles.AnyAsync(rol => rol.Nombre == roleName, cancellationToken);
            if (!exists)
            {
                await _context.Roles.AddAsync(new Rol(roleName), cancellationToken);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        await SeedUserAsync("admin@ejemplo.com", "Admin123!", roles, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedUserAsync(string email, string password, IReadOnlyCollection<string> roleNames, CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var roles = await _context.Roles
            .Where(rol => roleNames.Contains(rol.Nombre))
            .ToArrayAsync(cancellationToken);

        var existingUser = await _context.Usuarios
            .Include(usuario => usuario.Roles)
            .FirstOrDefaultAsync(usuario => usuario.Email == normalizedEmail, cancellationToken);

        if (existingUser is not null)
        {
            var existingRoleIds = existingUser.Roles.Select(role => role.Id).OrderBy(id => id).ToArray();
            var seededRoleIds = roles.Select(role => role.Id).OrderBy(id => id).ToArray();

            if (!existingRoleIds.SequenceEqual(seededRoleIds))
            {
                existingUser.AssignRoles(roles);
            }

            return;
        }

        var salt = _passwordHasher.GenerateSalt();
        var passwordHash = _passwordHasher.HashPassword(password, salt);
        var usuario = new Usuario(normalizedEmail, passwordHash, salt);
        usuario.AssignRoles(roles);

        await _context.Usuarios.AddAsync(usuario, cancellationToken);
    }
}
