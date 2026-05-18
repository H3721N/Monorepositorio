using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Users;

public sealed class UpdateUserRolesDto
{
    [Required]
    [MinLength(1)]
    public List<int> RoleIds { get; set; } = [];
}
