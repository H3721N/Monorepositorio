using System.Net;
using System.Net.Http.Json;
using API.IntegrationTests.Infrastructure;

namespace API.IntegrationTests;

public sealed class CountryDepartmentRegressionTests
{
    [Fact]
    public async Task Countries_WhenTokenIsMissing_ShouldReturnUnauthorized()
    {
        await using var factory = new TestApiFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/Countries");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CountriesAndDepartments_WhenUsingAdminToken_ShouldSupportCrudAndRejectDuplicates()
    {
        await using var factory = new TestApiFactory();
        var client = factory.CreateClient();
        var tokens = await client.LoginAsAdminAsync();
        client.AuthorizeWith(tokens.AccessToken);

        var createCountry = await client.PostAsJsonAsync("/api/Countries", new
        {
            name = "Guatemala",
            isoCode = "GT"
        });
        var duplicateCountry = await client.PostAsJsonAsync("/api/Countries", new
        {
            name = "guatemala",
            isoCode = "gt"
        });

        Assert.Equal(HttpStatusCode.Created, createCountry.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, duplicateCountry.StatusCode);

        var countryJson = await createCountry.ReadJsonAsync();
        var countryId = countryJson.GetProperty("id").GetInt32();

        var countryDetail = await client.GetAsync($"/api/Countries/{countryId}");
        Assert.Equal(HttpStatusCode.OK, countryDetail.StatusCode);

        var createDepartment = await client.PostAsJsonAsync("/api/Departments", new
        {
            name = "Guatemala",
            countryId
        });
        var duplicateDepartment = await client.PostAsJsonAsync("/api/Departments", new
        {
            name = "guatemala",
            countryId
        });

        Assert.Equal(HttpStatusCode.Created, createDepartment.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, duplicateDepartment.StatusCode);

        var departmentJson = await createDepartment.ReadJsonAsync();
        var departmentId = departmentJson.GetProperty("id").GetInt32();

        var departmentsByCountry = await client.GetAsync($"/api/Departments?countryId={countryId}");
        var updateDepartment = await client.PutAsJsonAsync($"/api/Departments/{departmentId}", new
        {
            name = "Sacatepequez",
            countryId
        });
        var deleteDepartment = await client.DeleteAsync($"/api/Departments/{departmentId}");
        var deletedDepartment = await client.GetAsync($"/api/Departments/{departmentId}");
        var deleteCountry = await client.DeleteAsync($"/api/Countries/{countryId}");
        var deletedCountry = await client.GetAsync($"/api/Countries/{countryId}");

        Assert.Equal(HttpStatusCode.OK, departmentsByCountry.StatusCode);
        Assert.Equal(HttpStatusCode.OK, updateDepartment.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, deleteDepartment.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, deletedDepartment.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, deleteCountry.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, deletedCountry.StatusCode);
    }

    [Fact]
    public async Task Departments_WhenCountryDoesNotExist_ShouldReturnConflict()
    {
        await using var factory = new TestApiFactory();
        var client = factory.CreateClient();
        var tokens = await client.LoginAsAdminAsync();
        client.AuthorizeWith(tokens.AccessToken);

        var response = await client.PostAsJsonAsync("/api/Departments", new
        {
            name = "Missing",
            countryId = 999
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
