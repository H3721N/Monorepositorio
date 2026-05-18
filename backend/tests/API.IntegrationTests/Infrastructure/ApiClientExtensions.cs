using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace API.IntegrationTests.Infrastructure;

internal static class ApiClientExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task<TokenResponse> LoginAsAdminAsync(this HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/Auth/login", new
        {
            email = "admin@ejemplo.com",
            password = "Admin123!"
        });

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TokenResponse>(JsonOptions))!;
    }

    public static void AuthorizeWith(this HttpClient client, string accessToken)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    public static async Task<JsonElement> ReadJsonAsync(this HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(json).RootElement.Clone();
    }
}

internal sealed record TokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt);
