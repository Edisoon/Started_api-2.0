using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StartedApi.Domain.Roles;
using StartedApi.Domain.Security;

namespace StartedApi.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(IdentitySeeder));

        await EnsureRoleAsync(roleManager, AppRoles.Admin, "System administrator role.");
        await EnsureRoleAsync(roleManager, AppRoles.User, "Default application user role.");

        logger.LogInformation("Identity roles seed completed.");
    }

    private static async Task EnsureRoleAsync(
        RoleManager<ApplicationRole> roleManager,
        string roleName,
        string description)
    {
        if (await roleManager.RoleExistsAsync(roleName))
        {
            return;
        }

        var role = new ApplicationRole
        {
            Name = roleName,
            Description = description,
            CreatedAtUtc = DateTime.UtcNow
        };

        var result = await roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Could not seed role '{roleName}': {errors}");
        }
    }
}
