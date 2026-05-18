using Application.DTOs.Departments;

namespace Application.DTOs.Countries;

public sealed record CountryDetailDto(
    int Id,
    string Name,
    string IsoCode,
    IReadOnlyCollection<DepartmentDto> Departments);
