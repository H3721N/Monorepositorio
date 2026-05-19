using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class DepartmentRepository : IDepartmentRepository
{
    private readonly AppDbContext _context;

    public DepartmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<Department>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Departments
            .AsNoTracking()
            .OrderBy(department => department.CountryId)
            .ThenBy(department => department.Name)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<Department?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Departments
            .FirstOrDefaultAsync(department => department.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Department>> GetByCountryIdAsync(int countryId, CancellationToken cancellationToken)
    {
        return await _context.Departments
            .AsNoTracking()
            .Where(department => department.CountryId == countryId)
            .OrderBy(department => department.Name)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Departments
            .AnyAsync(department => department.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByNameForCountryAsync(string name, int countryId, int? excludedId, CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim().ToUpper();

        return await _context.Departments
            .AnyAsync(department =>
                department.CountryId == countryId &&
                department.Name.ToUpper() == normalizedName &&
                (!excludedId.HasValue || department.Id != excludedId.Value),
                cancellationToken);
    }

    public async Task AddAsync(Department department, CancellationToken cancellationToken)
    {
        await _context.Departments.AddAsync(department, cancellationToken);
    }
}
