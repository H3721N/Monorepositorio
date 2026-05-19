using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Countries;

public sealed class CreateCountryDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(2, MinimumLength = 2)]
    public string IsoCode { get; set; } = string.Empty;
}
