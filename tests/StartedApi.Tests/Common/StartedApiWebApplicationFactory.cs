using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StartedApi.Domain.Roles;
using StartedApi.Domain.Security;
using StartedApi.Domain.Users;
using StartedApi.Infrastructure.Persistence;

namespace StartedApi.Tests.Common;

internal sealed class StartedApiWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        _connection.Open();

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                service => service.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(_connection));

            using var scope = services.BuildServiceProvider().CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.EnsureCreated();
        });
    }

    public async Task<ApplicationUser> CreateConfirmedUserAsync(
        string email,
        string password,
        params string[] roles)
    {
        using var scope = Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var effectiveRoles = roles.Length == 0 ? new[] { AppRoles.User } : roles;

        foreach (var roleName in effectiveRoles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = roleName,
                    Description = $"{roleName} role.",
                    CreatedAtUtc = DateTime.UtcNow
                });
            }
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = email,
            FirstName = "Test",
            LastName = "User",
            EmailConfirmed = true,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, password);
        result.Succeeded.Should().BeTrue(string.Join(", ", result.Errors.Select(error => error.Description)));

        foreach (var roleName in effectiveRoles)
        {
            var roleResult = await userManager.AddToRoleAsync(user, roleName);
            roleResult.Succeeded.Should().BeTrue(string.Join(", ", roleResult.Errors.Select(error => error.Description)));
        }

        return user;
    }

    public override async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
        await base.DisposeAsync();
    }
}
