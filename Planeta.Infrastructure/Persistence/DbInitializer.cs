using Microsoft.EntityFrameworkCore;
using Planeta.Domain.Auth;
using Planeta.Domain.Entities.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace Planeta.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InitializeAsync(PlanetaDbContext context)
    {
        // Автоматически накатит миграции, если база еще не создана
        await context.Database.MigrateAsync();

        // 1. Проверяем, есть ли роли в базе данных
        if (!await context.Roles.AnyAsync())
        {
            var roles = Enum.GetValues<RolesEnum>()
                .Select(r => new Role
                {
                    Id = (int)r,
                    Name = r.ToString()
                })
                .ToArray();

            await context.Roles.AddRangeAsync(roles);
        }

        // 2. Проверяем, есть ли права (Permissions)
        if (!await context.Permissions.AnyAsync())
        {
            var permissions = new List<Permission>
            {
                new() { Id = (int)PermissionsEnum.ProductsImport, Name = "products.import" },
                new() { Id = (int)PermissionsEnum.UsersManage, Name = "users.manage" },
                new() { Id = (int)PermissionsEnum.ReportsView, Name = "reports.view" },
                new() { Id = (int)PermissionsEnum.ProductsManage, Name = "products.manage" }
            };

            await context.Permissions.AddRangeAsync(permissions);
        }

        // Сохраняем всё в базу данных PostgreSQL
        await context.SaveChangesAsync();
    }
}
