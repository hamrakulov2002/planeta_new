using Microsoft.EntityFrameworkCore;
using Planeta.Domain.Auth;
using Planeta.Domain.Entities;
using Planeta.Domain.Entities.Auth;

namespace Planeta.Infrastructure.Persistence;

public class PlanetaDbContext : DbContext
{
    public PlanetaDbContext(DbContextOptions<PlanetaDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();

    public DbSet<ProductImage> ProductImages => Set<ProductImage>();

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<PhoneOptions> PhoneOptions => Set<PhoneOptions>();
    
    public DbSet<Planeta.Domain.Entities.Attribute> Attributes => Set<Planeta.Domain.Entities.Attribute>();
    public DbSet<ProductAttributeValue> ProductAttributeValues => Set<ProductAttributeValue>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.Entity<Role>()
        .HasMany(r => r.Permissions)
        .WithMany(p => p.Roles)
        .UsingEntity<Dictionary<string, object>>(
            "RolePermission",
            j => j.HasOne<Permission>().WithMany().HasForeignKey("PermissionsId"),
            j => j.HasOne<Role>().WithMany().HasForeignKey("RolesId"),
            je =>
            {
                je.HasKey("RolesId", "PermissionsId");
                je.ToTable("RolePermission");

                je.HasData(
                    new { RolesId = 2, PermissionsId = 1 }, // products.import
                    new { RolesId = 2, PermissionsId = 2 }, // users.manage
                    new { RolesId = 2, PermissionsId = 3 }, // reports.view
                    new { RolesId = 2, PermissionsId = 4 }, // products.manage

                    new { RolesId = 1, PermissionsId = 1 }, // products.import
                    new { RolesId = 1, PermissionsId = 2 }, // users.manage
                    new { RolesId = 1, PermissionsId = 3 }, // reports.view
                    new { RolesId = 1, PermissionsId = 4 }, // products.manage

                    new { RolesId = 3, PermissionsId = 4 }  // products.manage
                    
                );
            });

    var roles = Enum.GetValues<RolesEnum>()
        .Select(r => new Role
        {
            Id = (int)r,
            Name = r.ToString()
        })
        .ToArray();

    modelBuilder.Entity<Role>().HasData(roles);

    modelBuilder.Entity<Permission>().HasData(
        new Permission { Id = (int)PermissionsEnum.ProductsImport, Name = "products.import" },
        new Permission { Id = (int)PermissionsEnum.UsersManage, Name = "users.manage" },
        new Permission { Id = (int)PermissionsEnum.ReportsView, Name = "reports.view" },
        new Permission { Id = (int)PermissionsEnum.ProductsManage, Name = "products.manage" }
    );


    modelBuilder.Entity<ProductAttributeValue>(entity =>
    {
        entity.ToTable("ProductAttributeValues");

        entity.HasOne(pav => pav.Product)
            .WithMany(p => p.AttributeValues)
            .HasForeignKey(pav => pav.ProductId)
            .OnDelete(DeleteBehavior.Cascade); 

        entity.HasOne(pav => pav.Attribute)
            .WithMany() 
            .HasForeignKey(pav => pav.AttributeId)
            .OnDelete(DeleteBehavior.Restrict); 
    });

    modelBuilder.Entity<Planeta.Domain.Entities.Attribute>(entity =>
    {
        entity.ToTable("Attributes");
        
        entity.HasIndex(a => a.Name).IsUnique();
        entity.Property(a => a.Name).IsRequired().HasMaxLength(100);
    });


    modelBuilder.Entity<User>(entity =>
    {
        entity.HasIndex(u => u.Email).IsUnique();
        entity.Property(u => u.UserName).IsRequired().HasMaxLength(50);
    });

    modelBuilder.Entity<Product>(entity =>
    {
        entity.Property(p => p.Price).HasPrecision(18, 2);
        entity.Property(p => p.IMEI).HasMaxLength(15);
    });

    modelBuilder.Entity<OrderItem>()
        .Property(oi => oi.PriceAtPurchase)
        .HasPrecision(18, 2);
}
}