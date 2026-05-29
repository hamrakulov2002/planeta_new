using Microsoft.Extensions.DependencyInjection;
using Planeta.Application.Interfaces;
using Planeta.Application.Mappings;
using Planeta.Application.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Planeta.Application.ApplicationExtensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IBrandService, BrandService>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddAutoMapper(typeof(MappingProfile));

        return services;
    }
}
