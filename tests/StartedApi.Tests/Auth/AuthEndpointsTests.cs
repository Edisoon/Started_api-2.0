using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StartedApi.Application.Auth;
using StartedApi.Infrastructure.Persistence;
using StartedApi.Tests.Common;

namespace StartedApi.Tests.Auth;

public sealed class AuthEndpointsTests
{
    [Fact]
    public async Task Register_ReturnsSuccess_WithoutSensitiveFields()
    {
        await using var factory = new StartedApiWebApplicationFactory();
        using var client = factory.CreateClient();

        var request = new RegisterRequest(
            "edison@example.com",
            "Password123!",
            "Password123!",
            "Edison",
            "Lopez");

        using var response = await client.PostAsJsonAsync("/api/auth/register", request);
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("registered");
        content.Should().NotContain("Password123!");
        content.Should().NotContain("PasswordHash");
    }

    [Fact]
    public async Task Register_AutoConfirmsUser_AndAllowsLoginWithoutConfirmationEndpoint()
    {
        await using var factory = new StartedApiWebApplicationFactory();
        using var client = factory.CreateClient();

        var request = new RegisterRequest(
            "direct-login@example.com",
            "Password123!",
            "Password123!",
            "Direct",
            "Login");

        using var registerResponse = await client.PostAsJsonAsync("/api/auth/register", request);
        using var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest("direct-login@example.com", "Password123!"));

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await dbContext.Users.SingleAsync(user => user.Email == "direct-login@example.com");

        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        user.EmailConfirmed.Should().BeTrue();
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_ReturnsAccessToken_AndRefreshToken_ForConfirmedUser()
    {
        await using var factory = new StartedApiWebApplicationFactory();
        await factory.CreateConfirmedUserAsync("login@example.com", "Password123!");
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest("login@example.com", "Password123!"));

        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("accessToken");
        content.Should().Contain("refreshToken");
        content.Should().NotContain("Password123!");
    }

    [Fact]
    public async Task RefreshToken_RotatesRefreshToken_WhenTokenIsValid()
    {
        await using var factory = new StartedApiWebApplicationFactory();
        await factory.CreateConfirmedUserAsync("refresh@example.com", "Password123!");
        using var client = factory.CreateClient();

        using var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest("refresh@example.com", "Password123!"));
        var loginJson = await JsonDocument.ParseAsync(await loginResponse.Content.ReadAsStreamAsync());
        var refreshToken = loginJson.RootElement.GetProperty("data").GetProperty("refreshToken").GetString();

        using var refreshResponse = await client.PostAsJsonAsync(
            "/api/auth/refresh-token",
            new RefreshTokenRequest(refreshToken!));
        var refreshContent = await refreshResponse.Content.ReadAsStringAsync();

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        refreshContent.Should().Contain("accessToken");
        refreshContent.Should().Contain("refreshToken");
        refreshContent.Should().NotContain(refreshToken!);
    }

    [Fact]
    public async Task ChangePassword_ReturnsSuccess_ForAuthenticatedUser()
    {
        await using var factory = new StartedApiWebApplicationFactory();
        await factory.CreateConfirmedUserAsync("change-password@example.com", "Password123!");
        using var client = factory.CreateClient();

        using var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest("change-password@example.com", "Password123!"));
        var loginJson = await JsonDocument.ParseAsync(await loginResponse.Content.ReadAsStreamAsync());
        var accessToken = loginJson.RootElement.GetProperty("data").GetProperty("accessToken").GetString();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await client.PostAsJsonAsync(
            "/api/auth/change-password",
            new ChangePasswordRequest("Password123!", "Password456!", "Password456!"));
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("Password changed successfully");
        content.Should().NotContain("Password456!");
    }
}
