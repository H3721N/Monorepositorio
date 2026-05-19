using Application.DTOs.Auth;
using Domain.Entities;

namespace Application.Interfaces.Auth;

public interface ITokenService
{
    TokenResponseDto GenerateTokens(Usuario usuario);
}
