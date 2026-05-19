using Application.DTOs.Departments;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Authorize(Policy = "DepartmentAccess")]
[Route("api/[controller]")]
public sealed class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentsController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? countryId, CancellationToken cancellationToken)
    {
        if (countryId.HasValue)
        {
            var departmentsByCountry = await _departmentService.GetByCountryIdAsync(countryId.Value, cancellationToken);
            return Ok(departmentsByCountry);
        }

        var departments = await _departmentService.GetAllAsync(cancellationToken);
        return Ok(departments);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _departmentService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(new { errors = result.Errors });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentDto dto, CancellationToken cancellationToken)
    {
        var result = await _departmentService.CreateAsync(dto, cancellationToken);
        if (!result.IsSuccess)
        {
            return Conflict(new { errors = result.Errors });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDepartmentDto dto, CancellationToken cancellationToken)
    {
        var result = await _departmentService.UpdateAsync(id, dto, cancellationToken);
        if (!result.IsSuccess)
        {
            return result.Errors.Any(error => error.Contains("not found", StringComparison.OrdinalIgnoreCase))
                ? NotFound(new { errors = result.Errors })
                : Conflict(new { errors = result.Errors });
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _departmentService.DeleteAsync(id, cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : NotFound(new { errors = result.Errors });
    }
}
