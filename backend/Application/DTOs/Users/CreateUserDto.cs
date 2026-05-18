using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Users;

public sealed class CreateUserDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    public List<int> RoleIds { get; set; } = [];
}
