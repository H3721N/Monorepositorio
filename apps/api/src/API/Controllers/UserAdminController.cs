using System.Security.Claims;
using Application.DTOs.Users;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/admin/users")]
public sealed class UserAdminController : ControllerBase
{
    private readonly IUserAdminService _userAdminService;

    public UserAdminController(IUserAdminService userAdminService)
    {
        _userAdminService = userAdminService;
    }

    [HttpPost]
    [Authorize(Policy = "UserAdminAccess")]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto, CancellationToken cancellationToken)
    {
        var result = await _userAdminService.CreateAsync(dto, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetMe), result.Value)
            : BadRequest(new { errors = result.Errors });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _userAdminService.GetMeAsync(userId.Value, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(new { errors = result.Errors });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "UserAdminAccess")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
    {
        var result = await _userAdminService.DeactivateAsync(id, cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : NotFound(new { errors = result.Errors });
    }

    [HttpPut("{id:int}/roles")]
    [Authorize(Policy = "UserAdminAccess")]
    public async Task<IActionResult> ChangeRoles(int id, [FromBody] UpdateUserRolesDto dto, CancellationToken cancellationToken)
    {
        var result = await _userAdminService.ChangeRolesAsync(id, dto, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { errors = result.Errors });
    }

    private int? GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userId, out var parsedUserId) ? parsedUserId : null;
    }
}
