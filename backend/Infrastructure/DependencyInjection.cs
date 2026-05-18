using Application.Auth;
using Application.Interfaces.Auth;
using Application.Interfaces.Repositories;
using Infrastructure.Auth;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string contentRootPath)
    {
        var connectionString = BuildSqliteConnectionString(contentRootPath);

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<ICountryRepository, CountryRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IRolRepository, RolRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();
        services.AddScoped<IPasswordHasher, Argon2idPasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.Configure<JwtOptions>(options =>
        {
            options.Issuer = configuration["Jwt:Issuer"] ?? string.Empty;
            options.Audience = configuration["Jwt:Audience"] ?? string.Empty;
            options.SecretKey = configuration["Jwt:SecretKey"] ?? string.Empty;
            options.AccessTokenMinutes = int.TryParse(configuration["Jwt:AccessTokenMinutes"], out var accessTokenMinutes)
                ? accessTokenMinutes
                : 15;
            options.RefreshTokenDays = int.TryParse(configuration["Jwt:RefreshTokenDays"], out var refreshTokenDays)
                ? refreshTokenDays
                : 7;
        });

        return services;
    }

    private static string BuildSqliteConnectionString(string contentRootPath)
    {
        var backendPath = Directory.GetParent(contentRootPath)?.FullName
            ?? throw new InvalidOperationException("Could not resolve backend project path.");
        var infrastructurePath = Path.Combine(backendPath, "Infrastructure");
        Directory.CreateDirectory(infrastructurePath);

        var databasePath = Path.Combine(infrastructurePath, "app.db");
        return $"Data Source={databasePath}";
    }
}
