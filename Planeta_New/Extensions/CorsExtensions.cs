namespace Planeta_New.Extensions;

public static class CorsExtensions
{
    public static IServiceCollection AddPlanetaCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("PlanetaOpenCorsPolicy", builder =>
            {
                builder
                    .WithOrigins("http://localhost:5173")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }
}