using System.Security.Claims;
using API.Controllers;
using Application.Common;
using Application.DTOs.Auth;
using Application.DTOs.Countries;
using Application.DTOs.Departments;
using Application.DTOs.Users;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace API.IntegrationTests;

public sealed class ControllerBranchTests
{
    [Fact]
    public async Task CountriesController_Update_WhenServiceReturnsNotFound_ShouldReturnNotFound()
    {
        var service = new Mock<ICountryService>();
        service
            .Setup(s => s.UpdateAsync(9, It.IsAny<UpdateCountryDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<CountryDto>.Failure("Country was not found."));
        var controller = new CountriesController(service.Object);

        var result = await controller.Update(9, new UpdateCountryDto(), CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task CountriesController_Update_WhenServiceReturnsConflict_ShouldReturnConflict()
    {
        var service = new Mock<ICountryService>();
        service
            .Setup(s => s.UpdateAsync(9, It.IsAny<UpdateCountryDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<CountryDto>.Failure("A country with the same name already exists."));
        var controller = new CountriesController(service.Object);

        var result = await controller.Update(9, new UpdateCountryDto(), CancellationToken.None);

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task DepartmentsController_Update_WhenServiceReturnsNotFound_ShouldReturnNotFound()
    {
        var service = new Mock<IDepartmentService>();
        service
            .Setup(s => s.UpdateAsync(9, It.IsAny<UpdateDepartmentDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<DepartmentDto>.Failure("Department was not found."));
        var controller = new DepartmentsController(service.Object);

        var result = await controller.Update(9, new UpdateDepartmentDto(), CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task DepartmentsController_GetAll_WhenCountryIdIsProvided_ShouldFilterByCountry()
    {
        var service = new Mock<IDepartmentService>();
        service
            .Setup(s => s.GetByCountryIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new DepartmentDto(2, "Guatemala", 1)]);
        var controller = new DepartmentsController(service.Object);

        var result = await controller.GetAll(1, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var departments = Assert.IsAssignableFrom<IReadOnlyCollection<DepartmentDto>>(ok.Value);
        Assert.Single(departments);
    }

    [Fact]
    public async Task AuthController_Logout_WhenNameIdentifierClaimIsMissing_ShouldReturnUnauthorized()
    {
        var service = new Mock<IAuthService>();
        var controller = new AuthController(service.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            }
        };

        var result = await controller.Logout(CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task AuthController_ChangePassword_WhenServiceReturnsFailure_ShouldReturnBadRequest()
    {
        var service = new Mock<IAuthService>();
        service
            .Setup(s => s.ChangePasswordAsync(1, It.IsAny<ChangePasswordDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<bool>.Failure("Current password is invalid."));
        var controller = new AuthController(service.Object)
        {
            ControllerContext = ControllerContextWithUserId(1)
        };

        var result = await controller.ChangePassword(new ChangePasswordDto(), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UserAdminController_Create_WhenServiceReturnsFailure_ShouldReturnBadRequest()
    {
        var service = new Mock<IUserAdminService>();
        service
            .Setup(s => s.CreateAsync(It.IsAny<CreateUserDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<UserDto>.Failure("One or more roles were not found."));
        var controller = new UserAdminController(service.Object);

        var result = await controller.Create(new CreateUserDto(), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UserAdminController_GetMe_WhenClaimIsMissing_ShouldReturnUnauthorized()
    {
        var service = new Mock<IUserAdminService>();
        var controller = new UserAdminController(service.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        var result = await controller.GetMe(CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task UserAdminController_Deactivate_WhenUserIsNotFound_ShouldReturnNotFound()
    {
        var service = new Mock<IUserAdminService>();
        service
            .Setup(s => s.DeactivateAsync(9, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<bool>.Failure("User was not found."));
        var controller = new UserAdminController(service.Object);

        var result = await controller.Deactivate(9, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    private static ControllerContext ControllerContextWithUserId(int userId)
    {
        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                ]))
            }
        };
    }
}
