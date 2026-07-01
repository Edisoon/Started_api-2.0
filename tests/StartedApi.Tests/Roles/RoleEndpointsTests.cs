using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StartedApi.Application.Auth;
using StartedApi.Application.Roles;
using StartedApi.Domain.Audit;
using StartedApi.Domain.Security;
using StartedApi.Infrastructure.Persistence;
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

    [Fact]
    public async Task Admin_CanUpdateRole_AndAuditActorIsRecorded()
    {
        await using var factory = new StartedApiWebApplicationFactory();
        var admin = await factory.CreateConfirmedUserAsync("role-editor@example.com", "Password123!", AppRoles.Admin);
        using var client = factory.CreateClient();
        await AuthenticateAsync(client, "role-editor@example.com", "Password123!");

        using var createResponse = await client.PostAsJsonAsync(
            "/api/roles",
            new CreateRoleRequest("Support", "Support role"));
        var createJson = await JsonDocument.ParseAsync(await createResponse.Content.ReadAsStreamAsync());
        var roleId = createJson.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        using var updateResponse = await client.PutAsJsonAsync(
            $"/api/roles/{roleId}",
            new UpdateRoleRequest("CustomerSuccess", "Customer success role"));

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var role = await dbContext.Roles.SingleAsync(role => role.Id == roleId);
        var audit = await dbContext.AuditLogs.SingleAsync(log =>
            log.Action == AuditActions.RoleUpdated && log.EntityId == roleId.ToString());

        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        role.Name.Should().Be("CustomerSuccess");
        role.NormalizedName.Should().Be("CUSTOMERSUCCESS");
        role.Description.Should().Be("Customer success role");
        role.UpdatedByUserId.Should().Be(admin.Id);
        audit.UserId.Should().Be(admin.Id);
    }

    [Fact]
    public async Task Admin_CanDeactivateRole_AndInactiveRoleCannotBeAssigned()
    {
        await using var factory = new StartedApiWebApplicationFactory();
        var admin = await factory.CreateConfirmedUserAsync("role-status-admin@example.com", "Password123!", AppRoles.Admin);
        var user = await factory.CreateConfirmedUserAsync("role-status-user@example.com", "Password123!");
        var secondUser = await factory.CreateConfirmedUserAsync("role-status-second-user@example.com", "Password123!");
        using var client = factory.CreateClient();
        await AuthenticateAsync(client, "role-status-admin@example.com", "Password123!");

        using var createResponse = await client.PostAsJsonAsync(
            "/api/roles",
            new CreateRoleRequest("Temporary", "Temporary role"));
        var createJson = await JsonDocument.ParseAsync(await createResponse.Content.ReadAsStreamAsync());
        var roleId = createJson.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        using var initialAssignResponse = await client.PostAsJsonAsync(
            "/api/roles/assign",
            new AssignRoleRequest(user.Id, "Temporary"));
        using var statusResponse = await client.PatchAsJsonAsync(
            $"/api/roles/{roleId}/status",
            new UpdateRoleStatusRequest(false, "Created by mistake"));
        using var inactiveAssignResponse = await client.PostAsJsonAsync(
            "/api/roles/assign",
            new AssignRoleRequest(secondUser.Id, "Temporary"));
        using var userLoginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest("role-status-user@example.com", "Password123!"));
        var userLoginJson = await JsonDocument.ParseAsync(await userLoginResponse.Content.ReadAsStreamAsync());
        var tokenRoles = userLoginJson.RootElement
            .GetProperty("data")
            .GetProperty("user")
            .GetProperty("roles")
            .EnumerateArray()
            .Select(role => role.GetString())
            .ToArray();

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var role = await dbContext.Roles.SingleAsync(role => role.Id == roleId);
        var audit = await dbContext.AuditLogs.SingleAsync(log =>
            log.Action == AuditActions.RoleStatusChanged && log.EntityId == roleId.ToString());

        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        initialAssignResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        statusResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        inactiveAssignResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        userLoginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        tokenRoles.Should().NotContain("Temporary");
        role.IsActive.Should().BeFalse();
        role.DeactivatedAtUtc.Should().NotBeNull();
        role.DeactivatedByUserId.Should().Be(admin.Id);
        audit.UserId.Should().Be(admin.Id);
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
