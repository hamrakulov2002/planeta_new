using Planeta.Domain.Entities;

namespace Planeta.Domain.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<IEnumerable<Product>> GetAllAsync(); // Оставляем для совместимости
    IQueryable<Product> GetQueryable();       // Для быстрой фильтрации каталога в Postgres
    Task<List<Product>> ToListAsync(IQueryable<Product> query);
    
    Task AddAsync(Product product);
    void Update(Product product);
    void Delete(Product product);
    
    Task<Product?> GetProductWithImagesAsync(int id); // Оставляем для совместимости с сервисом
    Task<Product?> GetProductWithImagesAndAttributesAsync(int id); // Для обновления характеристик
    Task<Planeta.Domain.Entities.Attribute> GetOrCreateAttributeByNameAsync(string name); // Магия EAV
    Task AddAttributeValueAsync(ProductAttributeValue attributeValue);
    Task AddImagesRangeAsync(IEnumerable<ProductImage> images);
    Task<IEnumerable<Product>> GetCompatibleProductsByAttributesAsync(
        int excludeProductId,
        string phoneName,
        List<string> connectorTypes);
    
    
    Task<IEnumerable<Brand>> GetBrandsByCategoryIdAsync(int categoryId);
    Task SaveChangesAsync();
}