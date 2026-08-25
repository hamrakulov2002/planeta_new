using Planeta.Domain.Entities;

namespace Planeta.Domain.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<IEnumerable<Product>> GetAllAsync(); 
    IQueryable<Product> GetQueryable();       
    Task<List<Product>> ToListAsync(IQueryable<Product> query);
    
    Task AddAsync(Product product);
    void Update(Product product);
    void Delete(Product product);
    
    Task<Product?> GetProductWithImagesAsync(int id); 
    Task<Product?> GetProductWithImagesAndAttributesAsync(int id); 
    Task<Planeta.Domain.Entities.Attribute> GetOrCreateAttributeByNameAsync(string name); 
    Task AddAttributeValueAsync(ProductAttributeValue attributeValue);
    Task AddImagesRangeAsync(IEnumerable<ProductImage> images);
    Task<IEnumerable<Product>> GetCompatibleProductsByAttributesAsync(
        int excludeProductId,
        string phoneName,
        List<string> connectorTypes);
    
    
    Task<IEnumerable<Brand>> GetBrandsByCategoryIdAsync(int categoryId);
    Task SaveChangesAsync();
}