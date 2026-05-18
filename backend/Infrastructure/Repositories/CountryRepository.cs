using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class CountryRepository : ICountryRepository
{
    private readonly AppDbContext _context;

    public CountryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<Country>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Countries
            .AsNoTracking()
            .OrderBy(country => country.Name)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<Country?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Countries
            .FirstOrDefaultAsync(country => country.Id == id, cancellationToken);
    }

    public async Task<Country?> GetByIdWithDepartmentsAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Countries
            .Include(country => country.Departments)
            .FirstOrDefaultAsync(country => country.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Countries
            .AnyAsync(country => country.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludedId, CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim().ToUpper();

        return await _context.Countries
            .AnyAsync(country =>
                country.Name.ToUpper() == normalizedName &&
                (!excludedId.HasValue || country.Id != excludedId.Value),
                cancellationToken);
    }

    public async Task<bool> ExistsByIsoCodeAsync(string isoCode, int? excludedId, CancellationToken cancellationToken)
    {
        var normalizedIsoCode = isoCode.Trim().ToUpper();

        return await _context.Countries
            .AnyAsync(country =>
                country.IsoCode.ToUpper() == normalizedIsoCode &&
                (!excludedId.HasValue || country.Id != excludedId.Value),
                cancellationToken);
    }

    public async Task AddAsync(Country country, CancellationToken cancellationToken)
    {
        await _context.Countries.AddAsync(country, cancellationToken);
    }
}
