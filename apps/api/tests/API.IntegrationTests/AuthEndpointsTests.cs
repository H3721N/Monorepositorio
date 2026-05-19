using System.Net;
using System.Net.Http.Json;
using API.IntegrationTests.Infrastructure;

namespace API.IntegrationTests;

public sealed class AuthEndpointsTests
{
    [Fact]
    public async Task Login_WhenSeedUserCredentialsAreValid_ShouldReturnTokens()
    {
        await using var factory = new TestApiFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/Auth/login", new
        {
            email = "admin@ejemplo.com",
            password = "Admin123!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.ReadJsonAsync();
        Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("accessToken").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("refreshToken").GetString()));
    }

    [Fact]
    public async Task Login_WhenPasswordIsInvalid_ShouldReturnUnauthorized()
    {
        await using var factory = new TestApiFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/Auth/login", new
        {
            email = "admin@ejemplo.com",
            password = "wrong"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_WhenRefreshTokenIsValid_ShouldReturnNewTokens()
    {
        await using var factory = new TestApiFactory();
        var client = factory.CreateClient();
        var tokens = await client.LoginAsAdminAsync();

        var response = await client.PostAsJsonAsync("/api/Auth/refresh", new
        {
            refreshToken = tokens.RefreshToken
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.ReadJsonAsync();
        Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("accessToken").GetString()));
        Assert.NotEqual(tokens.RefreshToken, json.GetProperty("refreshToken").GetString());
    }

    [Fact]
    public async Task Refresh_WhenRefreshTokenIsInvalid_ShouldReturnUnauthorized()
    {
        await using var factory = new TestApiFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/Auth/refresh", new
        {
            refreshToken = "invalid"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_WhenTokenIsMissing_ShouldReturnUnauthorized()
    {
        await using var factory = new TestApiFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsync("/api/Auth/logout", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_WhenAuthorized_ShouldInvalidateRefreshToken()
    {
        await using var factory = new TestApiFactory();
        var client = factory.CreateClient();
        var tokens = await client.LoginAsAdminAsync();
        client.AuthorizeWith(tokens.AccessToken);

        var logout = await client.PostAsync("/api/Auth/logout", null);
        var refresh = await client.PostAsJsonAsync("/api/Auth/refresh", new
        {
            refreshToken = tokens.RefreshToken
        });

        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_WhenCurrentPasswordIsValid_ShouldRequireNewPasswordForLogin()
    {
        await using var factory = new TestApiFactory();
        var client = factory.CreateClient();
        var tokens = await client.LoginAsAdminAsync();
        client.AuthorizeWith(tokens.AccessToken);

        var change = await client.PostAsJsonAsync("/api/Auth/change-password", new
        {
            currentPassword = "Admin123!",
            newPassword = "NewAdmin123!"
        });
        var oldLogin = await client.PostAsJsonAsync("/api/Auth/login", new
        {
            email = "admin@ejemplo.com",
            password = "Admin123!"
        });
        var newLogin = await client.PostAsJsonAsync("/api/Auth/login", new
        {
            email = "admin@ejemplo.com",
            password = "NewAdmin123!"
        });

        Assert.Equal(HttpStatusCode.NoContent, change.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, oldLogin.StatusCode);
        Assert.Equal(HttpStatusCode.OK, newLogin.StatusCode);
    }
}
