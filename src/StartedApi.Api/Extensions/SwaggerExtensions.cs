using Microsoft.OpenApi.Models;

namespace StartedApi.Api.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddStartedApiSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Started API",
                Version = "v1",
                Description = "Reusable base API for users, authentication, authorization and audit."
            });

            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Enter a valid JWT Bearer token.",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            };

            options.AddSecurityDefinition("Bearer", securityScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                [securityScheme] = Array.Empty<string>()
            });
        });

        return services;
    }
}
