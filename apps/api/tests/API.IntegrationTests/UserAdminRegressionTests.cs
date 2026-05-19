using System.Net;
using System.Net.Http.Json;
using API.IntegrationTests.Infrastructure;

namespace API.IntegrationTests;

public sealed class UserAdminRegressionTests
{
    [Fact]
    public async Task GetMe_WhenAuthorized_ShouldReturnCurrentUserAndRoles()
    {
        await using var factory = new TestApiFactory();
        var client = factory.CreateClient();
        var tokens = await client.LoginAsAdminAsync();
        client.AuthorizeWith(tokens.AccessToken);

        var response = await client.GetAsync("/api/admin/users/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.ReadJsonAsync();
        Assert.Equal("admin@ejemplo.com", json.GetProperty("email").GetString());
        Assert.Contains(json.GetProperty("roles").EnumerateArray(), role => role.GetString() == "USER_ADMIN");
    }

    [Fact]
    public async Task CreateUser_WhenRolesExist_ShouldCreateUserAndAllowLogin()
    {
        await using var factory = new TestApiFactory();
        var client = factory.CreateClient();
        var tokens = await client.LoginAsAdminAsync();
        client.AuthorizeWith(tokens.AccessToken);

        var create = await client.PostAsJsonAsync("/api/admin/users", new
        {
            email = "nuevo@ejemplo.com",
            password = "User123!",
            roleIds = new[] { 1, 2 }
        });
        var login = await client.PostAsJsonAsync("/api/Auth/login", new
        {
            email = "nuevo@ejemplo.com",
            password = "User123!"
        });

        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    [Fact]
    public async Task CreateUser_WhenRoleDoesNotExist_ShouldReturnBadRequest()
    {
        await using var factory = new TestApiFactory();
        var client = factory.CreateClient();
        var tokens = await client.LoginAsAdminAsync();
        client.AuthorizeWith(tokens.AccessToken);

        var response = await client.PostAsJsonAsync("/api/admin/users", new
        {
            email = "nuevo@ejemplo.com",
            password = "User123!",
            roleIds = new[] { 99 }
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UserAdminEndpoints_WhenUserHasWrongRole_ShouldReturnForbidden()
    {
        await using var factory = new TestApiFactory();
        var client = factory.CreateClient();
        var adminTokens = await client.LoginAsAdminAsync();
        client.AuthorizeWith(adminTokens.AccessToken);

        await client.PostAsJsonAsync("/api/admin/users", new
        {
            email = "country-only@ejemplo.com",
            password = "User123!",
            roleIds = new[] { 1 }
        });

        var countryLogin = await client.PostAsJsonAsync("/api/Auth/login", new
        {
            email = "country-only@ejemplo.com",
            password = "User123!"
        });
        var countryTokens = (await countryLogin.Content.ReadFromJsonAsync<TokenResponse>())!;
        client.AuthorizeWith(countryTokens.AccessToken);

        var response = await client.PostAsJsonAsync("/api/admin/users", new
        {
            email = "blocked@ejemplo.com",
            password = "User123!",
            roleIds = new[] { 1 }
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ChangeRoles_WhenUserExists_ShouldReplaceRolesAndInvalidateOldRefreshToken()
    {
        await using var factory = new TestApiFactory();
        var client = factory.CreateClient();
        var adminTokens = await client.LoginAsAdminAsync();
        client.AuthorizeWith(adminTokens.AccessToken);

        var create = await client.PostAsJsonAsync("/api/admin/users", new
        {
            email = "role-change@ejemplo.com",
            password = "User123!",
            roleIds = new[] { 1 }
        });
        var created = await create.ReadJsonAsync();
        var userId = created.GetProperty("id").GetInt32();

        var userLogin = await client.PostAsJsonAsync("/api/Auth/login", new
        {
            email = "role-change@ejemplo.com",
            password = "User123!"
        });
        var userTokens = (await userLogin.Content.ReadFromJsonAsync<TokenResponse>())!;

        client.AuthorizeWith(adminTokens.AccessToken);
        var updateRoles = await client.PutAsJsonAsync($"/api/admin/users/{userId}/roles", new
        {
            roleIds = new[] { 2 }
        });
        var refreshOldToken = await client.PostAsJsonAsync("/api/Auth/refresh", new
        {
            refreshToken = userTokens.RefreshToken
        });
        var deactivate = await client.DeleteAsync($"/api/admin/users/{userId}");

        Assert.Equal(HttpStatusCode.OK, updateRoles.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, refreshOldToken.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, deactivate.StatusCode);
    }
}
