using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Departments;

public sealed class CreateDepartmentDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int CountryId { get; set; }
}
