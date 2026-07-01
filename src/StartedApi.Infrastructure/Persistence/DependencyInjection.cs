using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StartedApi.Application.Audit;
using StartedApi.Application.Auth;
using StartedApi.Application.Roles;
using StartedApi.Application.Security;
using StartedApi.Application.Users;
using StartedApi.Domain.Roles;
using StartedApi.Domain.Users;
using StartedApi.Infrastructure.Audit;
using StartedApi.Infrastructure.Authentication;
using StartedApi.Infrastructure.Auth;
using StartedApi.Infrastructure.Roles;
using StartedApi.Infrastructure.Security;
using StartedApi.Infrastructure.Users;

namespace StartedApi.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.Configure<JwtOptions>(
            configuration.GetSection(JwtOptions.SectionName));

        services
            .AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddSingleton<IRefreshTokenHasher, RefreshTokenHasher>();

        return services;
    }
}
