using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Planeta.Application.DTOs.Catalog;
using Planeta.Application.Interfaces;
using Planeta.Domain.Entities;
using Planeta.Domain.Interfaces;

namespace Planeta.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ProductService(
        IProductRepository productRepository,
        IMapper mapper,
        IWebHostEnvironment webHostEnvironment)
    {
        _productRepository = productRepository;
        _mapper = mapper;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IEnumerable<ProductDto>> GetCatalogAsync(int? categoryId, int? brandId, bool? IsUsed)
    {
        // Берем IQueryable, чтобы тяжелые фильтры улетали в Postgres, а не грузились в ОЗУ бэкенда
        var query = _productRepository.GetQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(product => product.CategoryId == categoryId);
        }

        if (brandId.HasValue)
        {
            query = query.Where(product => product.BrandId == brandId);
        }

        if (IsUsed.HasValue)
        {
            query = query.Where(product => product.IsUsed == IsUsed);
        }

        var products = await _productRepository.ToListAsync(query);
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<ProductDto?> GetByIdAsync(int productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);

        if (product == null) return null;

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<int> AddAsync(CreateProductRequest request)
    {
        // 1. Маппинг и базовая проверка
        var product = _mapper.Map<Product>(request);
        product.Id = 0; // Гарантируем INSERT
        product.AttributeValues = new List<ProductAttributeValue>();

        // 2. Сохраняем сам продукт, чтобы получить Id
        await _productRepository.AddAsync(product);
        await _productRepository.SaveChangesAsync();

        // 3. Сохраняем характеристики (Attributes приходят уже заполненными!)
        if (request.Attributes != null && request.Attributes.Any())
        {
            foreach (var attrDto in request.Attributes)
            {
                if (string.IsNullOrWhiteSpace(attrDto.AttributeName)) continue;

                var attribute = await _productRepository.GetOrCreateAttributeByNameAsync(attrDto.AttributeName);

                var jointValue = new ProductAttributeValue
                {
                    ProductId = product.Id,
                    AttributeId = attribute.Id,
                    Value = attrDto.Value ?? string.Empty
                };

                await _productRepository.AddAttributeValueAsync(jointValue);
            }

            await _productRepository.SaveChangesAsync();
        }

        // Возвращаем ID созданного продукта, чтобы фронтенд знал, куда загружать фото
        return product.Id;
    }
    
    public async Task UploadImagesAsync(int productId, UploadProductImagesRequest request)
    {
        // Проверяем, существует ли вообще такой продукт
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null) throw new KeyNotFoundException("Продукт не найден");

        var webRoot = _webHostEnvironment.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
        var uploadFolder = Path.Combine(webRoot, "uploads", "products");
        if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

        var productImages = new List<ProductImage>();

        // Сохраняем главную картинку
        if (request.MainImage != null && request.MainImage.Length > 0)
        {
            string dbImagePath = await SaveFileAsync(request.MainImage, uploadFolder);
            productImages.Add(new ProductImage 
            { 
                ProductId = productId, 
                ImagePath = dbImagePath, 
                IsMain = true 
            });
        }

        // Сохраняем дополнительные картинки
        if (request.ExtraImages != null && request.ExtraImages.Any())
        {
            foreach (var file in request.ExtraImages)
            {
                if (file.Length == 0) continue;
                string dbImagePath = await SaveFileAsync(file, uploadFolder);
                productImages.Add(new ProductImage 
                { 
                    ProductId = productId, 
                    ImagePath = dbImagePath, 
                    IsMain = false 
                });
            }
        }

        if (productImages.Any())
        {
            // Добавляешь в свой репозиторий метод для сохранения картинок (или через контекст)
            await _productRepository.AddImagesRangeAsync(productImages);
            await _productRepository.SaveChangesAsync();
        }
    }

    public async Task UpdateProductAsync(int productId, UpdateProductDto productDto)
    {
        // Тянем продукт со всеми картинками и характеристиками
        var existingProduct = await _productRepository.GetProductWithImagesAndAttributesAsync(productId);
        if (existingProduct == null) return;

        // 1. Обновляем базовые поля
        _mapper.Map(productDto, existingProduct);

        // 2. Обновляем картинки через полную перезапись
        existingProduct.Images.Clear();

        if (productDto.ImageUrls != null)
        {
            foreach (var url in productDto.ImageUrls)
            {
                existingProduct.Images.Add(new ProductImage
                {
                    ImagePath = url,
                    IsMain = (url == productDto.MainImageUrl)
                });
            }
        }

        // 3. Обновляем динамические характеристики через полную перезапись
        existingProduct.AttributeValues.Clear();

        if (productDto.Attributes != null)
        {
            foreach (var attrDto in productDto.Attributes)
            {
                if (string.IsNullOrWhiteSpace(attrDto.AttributeName)) continue;

                var attribute = await _productRepository.GetOrCreateAttributeByNameAsync(attrDto.AttributeName);

                existingProduct.AttributeValues.Add(new ProductAttributeValue
                {
                    Attribute = attribute,
                    Value = attrDto.Value,
                    Product = existingProduct // Указываем связь при обновлении
                });
            }
        }

        await _productRepository.SaveChangesAsync();
    }

    public async Task DeleteAsync(int productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);

        if (product == null)
            throw new KeyNotFoundException("Product not found");

        _productRepository.Delete(product);
        await _productRepository.SaveChangesAsync();
    }

    // Удобный хелпер для копирования IFormFile на диск
    private async Task<string> SaveFileAsync(IFormFile file, string targetFolder)
    {
        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        var fullPath = Path.Combine(targetFolder, uniqueFileName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/products/{uniqueFileName}";
    }
}