using Domain.Common;

namespace Domain.Entities;

public sealed class Country : BaseEntity
{
    private readonly List<Department> _departments = [];

    private Country()
    {
    }

    public Country(string name, string isoCode)
    {
        SetName(name);
        SetIsoCode(isoCode);
    }

    public string Name { get; private set; } = string.Empty;
    public string IsoCode { get; private set; } = string.Empty;
    public IReadOnlyCollection<Department> Departments => _departments.AsReadOnly();

    public void Update(string name, string isoCode)
    {
        SetName(name);
        SetIsoCode(isoCode);
        MarkAsUpdated();
    }

    public void SoftDeleteWithDepartments()
    {
        SoftDelete();

        foreach (var department in _departments)
        {
            department.SoftDelete();
        }
    }

    private void SetName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (name.Length > 100)
        {
            throw new ArgumentException("Country name cannot exceed 100 characters.", nameof(name));
        }

        Name = name.Trim();
    }

    private void SetIsoCode(string isoCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(isoCode);

        var normalizedIsoCode = isoCode.Trim().ToUpperInvariant();
        if (normalizedIsoCode.Length != 2)
        {
            throw new ArgumentException("Country ISO code must contain exactly 2 characters.", nameof(isoCode));
        }

        IsoCode = normalizedIsoCode;
    }
}
