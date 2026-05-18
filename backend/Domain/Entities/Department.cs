using Domain.Common;

namespace Domain.Entities;

public sealed class Department : BaseEntity
{
    private Department()
    {
    }

    public Department(string name, int countryId)
    {
        SetName(name);
        SetCountry(countryId);
    }

    public string Name { get; private set; } = string.Empty;
    public int CountryId { get; private set; }
    public Country Country { get; private set; } = null!;

    public void Update(string name, int countryId)
    {
        SetName(name);
        SetCountry(countryId);
        MarkAsUpdated();
    }

    private void SetName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (name.Length > 100)
        {
            throw new ArgumentException("Department name cannot exceed 100 characters.", nameof(name));
        }

        Name = name.Trim();
    }

    private void SetCountry(int countryId)
    {
        if (countryId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(countryId), "CountryId must be greater than zero.");
        }

        CountryId = countryId;
    }
}
