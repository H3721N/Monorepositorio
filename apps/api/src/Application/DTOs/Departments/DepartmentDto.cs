namespace Application.DTOs.Departments;

public sealed record DepartmentDto(
    int Id,
    string Name,
    int CountryId);
