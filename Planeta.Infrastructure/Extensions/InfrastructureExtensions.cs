using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Planeta.Application.Interfaces;
using Planeta.Application.Mappings;
using Planeta.Application.Services;
using Planeta.Domain.Interfaces;
using Planeta.Infrastructure.JWT;
using Planeta.Infrastructure.Persistence;
using Planeta.Infrastructure.Repositories;

namespace Planeta.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        services.AddDbContext<PlanetaDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IBrandRepository, BrandRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IJwtProvider, JwtProvider>();


        return services;
    }
}