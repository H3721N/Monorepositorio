using Application.Commands.Auth;
using Application.Commands.Countries;
using Application.Commands.Departments;
using Application.Commands.Users;
using Application.Interfaces.Services;
using Application.Interfaces.Validation;
using Application.Services;
using Application.Validators.Auth;
using Application.Validators.Countries;
using Application.Validators.Departments;
using Application.Validators.Users;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IValidator<CreateCountryCommand>, CreateCountryCommandValidator>();
        services.AddScoped<IValidator<UpdateCountryCommand>, UpdateCountryCommandValidator>();
        services.AddScoped<IValidator<CreateDepartmentCommand>, CreateDepartmentCommandValidator>();
        services.AddScoped<IValidator<UpdateDepartmentCommand>, UpdateDepartmentCommandValidator>();
        services.AddScoped<IValidator<LoginCommand>, LoginCommandValidator>();
        services.AddScoped<IValidator<ChangePasswordCommand>, ChangePasswordCommandValidator>();
        services.AddScoped<IValidator<CreateUserCommand>, CreateUserCommandValidator>();
        services.AddScoped<IValidator<UpdateUserRolesCommand>, UpdateUserRolesCommandValidator>();
        services.AddScoped<ICountryService, CountryService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserAdminService, UserAdminService>();

        return services;
    }
}
