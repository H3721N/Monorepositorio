namespace Application.Commands.Departments;

public sealed record UpdateDepartmentCommand(int Id, string Name, int CountryId);
