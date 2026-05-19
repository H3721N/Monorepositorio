using Application.DTOs.Countries;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Authorize(Policy = "CountryAccess")]
[Route("api/[controller]")]
public sealed class CountriesController : ControllerBase
{
    private readonly ICountryService _countryService;

    public CountriesController(ICountryService countryService)
    {
        _countryService = countryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var countries = await _countryService.GetAllAsync(cancellationToken);
        return Ok(countries);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _countryService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(new { errors = result.Errors });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCountryDto dto, CancellationToken cancellationToken)
    {
        var result = await _countryService.CreateAsync(dto, cancellationToken);
        if (!result.IsSuccess)
        {
            return Conflict(new { errors = result.Errors });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCountryDto dto, CancellationToken cancellationToken)
    {
        var result = await _countryService.UpdateAsync(id, dto, cancellationToken);
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
        var result = await _countryService.DeleteAsync(id, cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : NotFound(new { errors = result.Errors });
    }
}
