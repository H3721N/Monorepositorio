using Application.Commands.Departments;
using Application.Common;
using Application.DTOs.Departments;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Validation;
using Domain.Entities;

namespace Application.Services;

public sealed class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ICountryRepository _countryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateDepartmentCommand> _createValidator;
    private readonly IValidator<UpdateDepartmentCommand> _updateValidator;

    public DepartmentService(
        IDepartmentRepository departmentRepository,
        ICountryRepository countryRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateDepartmentCommand> createValidator,
        IValidator<UpdateDepartmentCommand> updateValidator)
    {
        _departmentRepository = departmentRepository;
        _countryRepository = countryRepository;
        _unitOfWork = unitOfWork;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyCollection<DepartmentDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var departments = await _departmentRepository.GetAllAsync(cancellationToken);
        return departments.Select(ToDto).ToArray();
    }

    public async Task<IReadOnlyCollection<DepartmentDto>> GetByCountryIdAsync(int countryId, CancellationToken cancellationToken)
    {
        var departments = await _departmentRepository.GetByCountryIdAsync(countryId, cancellationToken);
        return departments.Select(ToDto).ToArray();
    }

    public async Task<ServiceResult<DepartmentDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var department = await _departmentRepository.GetByIdAsync(id, cancellationToken);
        if (department is null)
        {
            return ServiceResult<DepartmentDto>.Failure("Department was not found.");
        }

        return ServiceResult<DepartmentDto>.Success(ToDto(department));
    }

    public async Task<ServiceResult<DepartmentDto>> CreateAsync(CreateDepartmentDto dto, CancellationToken cancellationToken)
    {
        var command = new CreateDepartmentCommand(dto.Name, dto.CountryId);
        var validation = _createValidator.Validate(command);
        if (!validation.IsValid)
        {
            return ServiceResult<DepartmentDto>.Failure(validation.Errors);
        }

        var relationErrors = await ValidateDepartmentRelationsAsync(command.Name, command.CountryId, null, cancellationToken);
        if (relationErrors.Count > 0)
        {
            return ServiceResult<DepartmentDto>.Failure(relationErrors);
        }

        var department = new Department(command.Name, command.CountryId);
        await _departmentRepository.AddAsync(department, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<DepartmentDto>.Success(ToDto(department));
    }

    public async Task<ServiceResult<DepartmentDto>> UpdateAsync(int id, UpdateDepartmentDto dto, CancellationToken cancellationToken)
    {
        var command = new UpdateDepartmentCommand(id, dto.Name, dto.CountryId);
        var validation = _updateValidator.Validate(command);
        if (!validation.IsValid)
        {
            return ServiceResult<DepartmentDto>.Failure(validation.Errors);
        }

        var department = await _departmentRepository.GetByIdAsync(id, cancellationToken);
        if (department is null)
        {
            return ServiceResult<DepartmentDto>.Failure("Department was not found.");
        }

        var relationErrors = await ValidateDepartmentRelationsAsync(command.Name, command.CountryId, id, cancellationToken);
        if (relationErrors.Count > 0)
        {
            return ServiceResult<DepartmentDto>.Failure(relationErrors);
        }

        department.Update(command.Name, command.CountryId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<DepartmentDto>.Success(ToDto(department));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var department = await _departmentRepository.GetByIdAsync(id, cancellationToken);
        if (department is null)
        {
            return ServiceResult<bool>.Failure("Department was not found.");
        }

        department.SoftDelete();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<bool>.Success(true);
    }

    private async Task<List<string>> ValidateDepartmentRelationsAsync(string name, int countryId, int? excludedId, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        if (!await _countryRepository.ExistsByIdAsync(countryId, cancellationToken))
        {
            errors.Add("Country was not found.");
            return errors;
        }

        if (await _departmentRepository.ExistsByNameForCountryAsync(name.Trim(), countryId, excludedId, cancellationToken))
        {
            errors.Add("A department with the same name already exists for this country.");
        }

        return errors;
    }

    private static DepartmentDto ToDto(Department department)
    {
        return new DepartmentDto(department.Id, department.Name, department.CountryId);
    }
}
