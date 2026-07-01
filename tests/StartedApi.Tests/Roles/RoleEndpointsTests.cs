using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using StartedApi.Application.Auth;
using StartedApi.Application.Roles;
using StartedApi.Domain.Security;
using StartedApi.Tests.Common;

namespace StartedApi.Tests.Roles;

public sealed class RoleEndpointsTests
{
    [Fact]
    public async Task Admin_CanCreateAssignAndReadUserRoles()
    {
        await using var factory = new StartedApiWebApplicationFactory();
        await factory.CreateConfirmedUserAsync("role-admin@example.com", "Password123!", AppRoles.Admin);
        var user = await factory.CreateConfirmedUserAsync("role-user@example.com", "Password123!");
        using var client = factory.CreateClient();
        await AuthenticateAsync(client, "role-admin@example.com", "Password123!");

        using var createResponse = await client.PostAsJsonAsync(
            "/api/roles",
            new CreateRoleRequest("Manager", "Management role"));
        using var assignResponse = await client.PostAsJsonAsync(
            "/api/roles/assign",
            new AssignRoleRequest(user.Id, "Manager"));
        using var rolesResponse = await client.GetAsync($"/api/roles/users/{user.Id}");

        var rolesContent = await rolesResponse.Content.ReadAsStringAsync();

        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        assignResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        rolesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        rolesContent.Should().Contain("Manager");
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
