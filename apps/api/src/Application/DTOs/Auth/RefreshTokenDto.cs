using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth;

public sealed class RefreshTokenDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
