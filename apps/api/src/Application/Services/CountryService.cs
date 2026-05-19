using Application.Commands.Countries;
using Application.Common;
using Application.DTOs.Countries;
using Application.DTOs.Departments;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Validation;
using Domain.Entities;

namespace Application.Services;

public sealed class CountryService : ICountryService
{
    private readonly ICountryRepository _countryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateCountryCommand> _createValidator;
    private readonly IValidator<UpdateCountryCommand> _updateValidator;

    public CountryService(
        ICountryRepository countryRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateCountryCommand> createValidator,
        IValidator<UpdateCountryCommand> updateValidator)
    {
        _countryRepository = countryRepository;
        _unitOfWork = unitOfWork;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyCollection<CountryDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var countries = await _countryRepository.GetAllAsync(cancellationToken);
        return countries.Select(ToDto).ToArray();
    }

    public async Task<ServiceResult<CountryDetailDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var country = await _countryRepository.GetByIdWithDepartmentsAsync(id, cancellationToken);
        if (country is null)
        {
            return ServiceResult<CountryDetailDto>.Failure("Country was not found.");
        }

        var departments = country.Departments
            .Where(department => !department.IsDeleted)
            .Select(department => new DepartmentDto(department.Id, department.Name, department.CountryId))
            .ToArray();

        return ServiceResult<CountryDetailDto>.Success(new CountryDetailDto(country.Id, country.Name, country.IsoCode, departments));
    }

    public async Task<ServiceResult<CountryDto>> CreateAsync(CreateCountryDto dto, CancellationToken cancellationToken)
    {
        var command = new CreateCountryCommand(dto.Name, dto.IsoCode);
        var validation = _createValidator.Validate(command);
        if (!validation.IsValid)
        {
            return ServiceResult<CountryDto>.Failure(validation.Errors);
        }

        var duplicateErrors = await ValidateDuplicateCountryAsync(command.Name, command.IsoCode, null, cancellationToken);
        if (duplicateErrors.Count > 0)
        {
            return ServiceResult<CountryDto>.Failure(duplicateErrors);
        }

        var country = new Country(command.Name, command.IsoCode);
        await _countryRepository.AddAsync(country, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<CountryDto>.Success(ToDto(country));
    }

    public async Task<ServiceResult<CountryDto>> UpdateAsync(int id, UpdateCountryDto dto, CancellationToken cancellationToken)
    {
        var command = new UpdateCountryCommand(id, dto.Name, dto.IsoCode);
        var validation = _updateValidator.Validate(command);
        if (!validation.IsValid)
        {
            return ServiceResult<CountryDto>.Failure(validation.Errors);
        }

        var country = await _countryRepository.GetByIdAsync(id, cancellationToken);
        if (country is null)
        {
            return ServiceResult<CountryDto>.Failure("Country was not found.");
        }

        var duplicateErrors = await ValidateDuplicateCountryAsync(command.Name, command.IsoCode, id, cancellationToken);
        if (duplicateErrors.Count > 0)
        {
            return ServiceResult<CountryDto>.Failure(duplicateErrors);
        }

        country.Update(command.Name, command.IsoCode);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<CountryDto>.Success(ToDto(country));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var country = await _countryRepository.GetByIdWithDepartmentsAsync(id, cancellationToken);
        if (country is null)
        {
            return ServiceResult<bool>.Failure("Country was not found.");
        }

        country.SoftDeleteWithDepartments();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<bool>.Success(true);
    }

    private async Task<List<string>> ValidateDuplicateCountryAsync(string name, string isoCode, int? excludedId, CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        var normalizedName = name.Trim();
        var normalizedIsoCode = isoCode.Trim().ToUpperInvariant();

        if (await _countryRepository.ExistsByNameAsync(normalizedName, excludedId, cancellationToken))
        {
            errors.Add("A country with the same name already exists.");
        }

        if (await _countryRepository.ExistsByIsoCodeAsync(normalizedIsoCode, excludedId, cancellationToken))
        {
            errors.Add("A country with the same IsoCode already exists.");
        }

        return errors;
    }

    private static CountryDto ToDto(Country country)
    {
        return new CountryDto(country.Id, country.Name, country.IsoCode);
    }
}
