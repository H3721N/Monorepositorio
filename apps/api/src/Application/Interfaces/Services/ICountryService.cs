using Application.Common;
using Application.DTOs.Countries;

namespace Application.Interfaces.Services;

public interface ICountryService
{
    Task<IReadOnlyCollection<CountryDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<ServiceResult<CountryDetailDto>> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<ServiceResult<CountryDto>> CreateAsync(CreateCountryDto dto, CancellationToken cancellationToken);
    Task<ServiceResult<CountryDto>> UpdateAsync(int id, UpdateCountryDto dto, CancellationToken cancellationToken);
    Task<ServiceResult<bool>> DeleteAsync(int id, CancellationToken cancellationToken);
}
