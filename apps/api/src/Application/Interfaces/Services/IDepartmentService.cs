using Application.Common;
using Application.DTOs.Departments;

namespace Application.Interfaces.Services;

public interface IDepartmentService
{
    Task<IReadOnlyCollection<DepartmentDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<DepartmentDto>> GetByCountryIdAsync(int countryId, CancellationToken cancellationToken);
    Task<ServiceResult<DepartmentDto>> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<ServiceResult<DepartmentDto>> CreateAsync(CreateDepartmentDto dto, CancellationToken cancellationToken);
    Task<ServiceResult<DepartmentDto>> UpdateAsync(int id, UpdateDepartmentDto dto, CancellationToken cancellationToken);
    Task<ServiceResult<bool>> DeleteAsync(int id, CancellationToken cancellationToken);
}
