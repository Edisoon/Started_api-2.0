using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using StartedApi.Application.Auth;
using StartedApi.Domain.Security;
using StartedApi.Tests.Common;

namespace StartedApi.Tests.Users;

public sealed class UserEndpointsTests
{
    [Fact]
    public async Task ListUsers_ReturnsForbidden_ForNonAdminUser()
    {
        await using var factory = new StartedApiWebApplicationFactory();
        await factory.CreateConfirmedUserAsync("regular@example.com", "Password123!");
        using var client = factory.CreateClient();
        await AuthenticateAsync(client, "regular@example.com", "Password123!");

        using var response = await client.GetAsync("/api/users");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ListUsers_ReturnsUsers_ForAdminUser()
    {
        await using var factory = new StartedApiWebApplicationFactory();
        await factory.CreateConfirmedUserAsync("admin@example.com", "Password123!", AppRoles.Admin);
        await factory.CreateConfirmedUserAsync("listed@example.com", "Password123!");
        using var client = factory.CreateClient();
        await AuthenticateAsync(client, "admin@example.com", "Password123!");

        using var response = await client.GetAsync("/api/users");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("listed@example.com");
        content.Should().NotContain("PasswordHash");
    }

    private static async Task AuthenticateAsync(HttpClient client, string email, string password)
    {
        using var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, password));

        var json = await JsonDocument.ParseAsync(await loginResponse.Content.ReadAsStreamAsync());
        var accessToken = json.RootElement.GetProperty("data").GetProperty("accessToken").GetString();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
    }
}
