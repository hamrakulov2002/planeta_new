using Microsoft.EntityFrameworkCore;
using Planeta.Domain.Entities;
using Planeta.Domain.Interfaces;
using Planeta.Infrastructure.Persistence;

namespace Planeta.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly PlanetaDbContext _dbContext;

    public ProductRepository(PlanetaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _dbContext.Products
            .Include(p => p.Images)
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.AttributeValues)
            .ThenInclude(av => av.Attribute)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .Include(p => p.AttributeValues)
            .ThenInclude(av => av.Attribute)
            .ToListAsync();
    }

    public IQueryable<Product> GetQueryable()
    {
        return _dbContext.Products
            .Include(p => p.Images)
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.AttributeValues)
            .ThenInclude(av => av.Attribute)
            .AsNoTracking();
    }

    public async Task<List<Product>> ToListAsync(IQueryable<Product> query)
    {
        return await query.ToListAsync();
    }

    public async Task AddAsync(Product product)
    {
        await _dbContext.Products.AddAsync(product);
    }

    public void Update(Product product)
    {
        var existingProduct = _dbContext.Products.FirstOrDefault(p => p.Id == product.Id);
        if (existingProduct != null)
        {
            _dbContext.Entry(existingProduct).CurrentValues.SetValues(product);
        }
    }

    public void Delete(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        _dbContext.Products.Remove(product);
    }

    public async Task<Product?> GetProductWithImagesAsync(int id)
    {
        return await _dbContext.Products
            .Include(p => p.Images)
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Product?> GetProductWithImagesAndAttributesAsync(int id)
    {
        return await _dbContext.Products
            .Include(p => p.Images)
            .Include(p => p.AttributeValues)
            .ThenInclude(av => av.Attribute)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    // ИСПРАВЛЕНО: Теперь новые атрибуты сохраняются мгновенно и получают валидный Id
    public async Task<Planeta.Domain.Entities.Attribute> GetOrCreateAttributeByNameAsync(string name)
    {
        var normalizedName = name.Trim();

        var attribute = await _dbContext.Attributes
            .FirstOrDefaultAsync(a => a.Name.ToLower() == normalizedName.ToLower());

        if (attribute == null)
        {
            attribute = new Planeta.Domain.Entities.Attribute { Name = normalizedName };
            await _dbContext.Attributes.AddAsync(attribute);

            // Фиксируем добавление в БД, чтобы база выдала объекту нормальный Id (например, 8, 9, 10...)
            await _dbContext.SaveChangesAsync();
        }

        return attribute;
    }

    public async Task AddImagesRangeAsync(IEnumerable<ProductImage> images)
    {
        if (images == null || !images.Any()) return;

        // Добавляем всю коллекцию картинок в DbSet изображений
        //await _dbContext.Set<ProductImage>().AddRangeAsync(images);

        // Если у тебя в репозитории контекст настроен напрямую на таблицу, можно так:
        await _dbContext.ProductImages.AddRangeAsync(images);
    }

    public async Task<IEnumerable<Product>> GetCompatibleProductsByAttributesAsync(
        int excludeProductId, string phoneName, List<string> connectorTypes)
    {
        var lowerPhoneName = phoneName.Trim().ToLower();
        var lowerConnectorTypes = connectorTypes.Select(c => c.ToLower()).ToList();

        // 1. Чехлы / стёкла / плёнки — строго по точному совпадению модели телефона
        var caseAccessoryIds = await _dbContext.Products
            .Where(p => p.Id != excludeProductId)
            .Where(p =>
                p.Category.Name.ToLower().Contains("чехол") ||
                p.Category.Name.ToLower().Contains("стекл") ||
                p.Category.Name.ToLower().Contains("пленк") ||
                p.Category.Name.ToLower().Contains("плёнк"))
            .Where(p => p.AttributeValues.Any(av =>
                av.Attribute.Name.ToLower() == "совместимость" &&
                av.Value.ToLower() == lowerPhoneName))
            .Select(p => p.Id)
            .ToListAsync();

        // 2. Зарядники / кабели / адаптеры — по совпадению типа разъёма
        var chargerAccessoryIds = new List<int>();
        if (lowerConnectorTypes.Any())
        {
            chargerAccessoryIds = await _dbContext.Products
                .Where(p => p.Id != excludeProductId)
                .Where(p =>
                    p.Category.Name.ToLower().Contains("зарядк") ||
                    p.Category.Name.ToLower().Contains("кабел") ||
                    p.Category.Name.ToLower().Contains("адаптер"))
                .Where(p => p.AttributeValues.Any(av =>
                    (av.Attribute.Name.ToLower() == "тип разъёма" ||
                     av.Attribute.Name.ToLower() == "тип разъема") &&
                    lowerConnectorTypes.Contains(av.Value.ToLower())))
                .Select(p => p.Id)
                .ToListAsync();
        }

        // 3. Повербанки и прочие универсальные аксессуары
        var universalAccessoryIds = await _dbContext.Products
            .Where(p => p.Id != excludeProductId)
            .Where(p => p.Category.Name.ToLower().Contains("повербанк"))
            .Select(p => p.Id)
            .ToListAsync();

        // Объединяем все ID в памяти (дёшево — это просто int'ы)
        var allIds = caseAccessoryIds
            .Union(chargerAccessoryIds)
            .Union(universalAccessoryIds)
            .Distinct()
            .Take(30)
            .ToList();

        if (!allIds.Any()) return Enumerable.Empty<Product>();

        // Один финальный запрос со всеми Include — материализуем полные сущности
        return await _dbContext.Products
            .Include(p => p.Images)
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.AttributeValues)
            .ThenInclude(av => av.Attribute)
            .Where(p => allIds.Contains(p.Id))
            .OrderByDescending(p => p.Id)
            .AsNoTracking()
            .ToListAsync();
    }


    public async Task<IEnumerable<Brand>> GetBrandsByCategoryIdAsync(int categoryId)
    {
        return await _dbContext.Products
            .Where(product => product.CategoryId == categoryId)
            .Select(product => product.Brand)
            .Distinct()
            .ToListAsync();
    }

    public async Task AddAttributeValueAsync(ProductAttributeValue attributeValue)
    {
        await _dbContext.ProductAttributeValues.AddAsync(attributeValue);
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}