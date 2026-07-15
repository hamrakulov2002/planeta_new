using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Planeta.Application.DTOs.Catalog;
using Planeta.Application.DTOs.Common;
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

    /*public async Task<PagedResult<ProductDto>> GetAllProductAsync(int? categoryId, int? brandId, bool? IsUsed,
        string? search,
        int? pageNumber, int? pageSize)
    {
        int validPageNumber = pageNumber ?? 1;
        int validPageSize = pageSize ?? 10;


        if (validPageNumber < 1) validPageNumber = 1;
        if (validPageSize < 1) validPageSize = 10;


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

        int totalCount = query.Count();

        var paginatedQuery = query
            .OrderByDescending(product => product.Id)
            .Skip((validPageNumber - 1) * validPageSize)
            .Take(validPageSize);

        var products = await _productRepository.ToListAsync(paginatedQuery);

        var producttDtos = _mapper.Map<IEnumerable<ProductDto>>(products);

        return new  PagedResult<ProductDto>(producttDtos, totalCount, validPageNumber, validPageSize);

    }*/

    public async Task<PagedResult<ProductDto>> GetAllProductAsync(int? categoryId, int? brandId, bool? IsUsed,
        string? search, int? pageNumber, int? pageSize)
    {
        int validPageNumber = pageNumber ?? 1;
        int validPageSize = pageSize ?? 10;

        if (validPageNumber < 1) validPageNumber = 1;
        if (validPageSize < 1) validPageSize = 10;

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

        if (!string.IsNullOrWhiteSpace(search))
        {
            var lowerSearch = search.ToLower().Trim();

            // ЖЕСТКОЕ ПРАВИЛО: Исключаем сами телефоны/смартфоны из выдачи рекомендаций
            if (validPageSize == 6)
            {
                query = query.Where(product =>
                    !product.Category.Name.ToLower().Contains("телефон") &&
                    !product.Category.Name.ToLower().Contains("смартфон")
                );
            }

            // 1. Слова для мягкого поиска универсальных гаджетов (например, "iphone", "15")
            var searchWords = lowerSearch.Split(new[] { ' ', ',', '-', '/' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(word => word.Length > 2)
                .Where(word => word != "pro" && word != "max" && word != "ultra" && word != "plus")
                .ToList();

            // 2. Ищем типы разъемов в названии телефона
            var connectorKeywords = new List<string> { "usb-c", "usb c", "lightning", "micro usb", "type-c", "type c" };
            var foundConnectors = connectorKeywords.Where(c => lowerSearch.Contains(c)).ToList();

            query = query.Where(product =>
                // Базовый поиск по названию/описанию
                product.Name.ToLower().Contains(lowerSearch) ||
                (product.Description != null && product.Description.ToLower().Contains(lowerSearch)) ||

                // Проверка по характеристикам
                product.AttributeValues.Any(av =>
                    // А. Для ЧЕХЛОВ и СТЁКОЛ — проверяем СТРОГОЕ вхождение модели
                    ((product.Category.Name.ToLower().Contains("чехол") ||
                      product.Category.Name.ToLower().Contains("стекл") ||
                      product.Category.Name.ToLower().Contains("пленк"))
                        ? (av.Attribute.Name.ToLower() == "совместимость" && lowerSearch.Contains(av.Value.ToLower()))

                        // Б. Для ОСТАЛЬНЫХ (Повербанки) — мягкий поиск по словам ("Apple", "Samsung")
                        : (searchWords.Any(word => av.Value.ToLower().Contains(word)) ||
                           searchWords.Any(word => word.Contains(av.Value.ToLower())))
                    ) ||

                    // В. Проверка одинаковых разъёмов (Кабели, Зарядки)
                    // ТЕПЕРЬ С ДОПОЛНИТЕЛЬНОЙ ПРОВЕРКОЙ: 
                    // Зарядка подходит по разъёму ТОЛЬКО ЕСЛИ у неё в совместимости НЕТ противоречащих моделей 
                    // (то есть либо совместимость пустая/универсальная, либо содержит открытую модель телефона)
                    ((av.Attribute.Name.ToLower() == "тип разъёма" || av.Attribute.Name.ToLower() == "тип разъема") &&
                     (foundConnectors.Any(c => av.Value.ToLower().Contains(c)) ||
                      av.Value.ToLower().Contains("usb-c") || av.Value.ToLower().Contains("type-c")) &&

                     // Проверяем, что у этого товара нет полей "совместимость", которые НЕ содержат имя нашего телефона
                     !product.AttributeValues.Any(subAv =>
                             subAv.Attribute.Name.ToLower() == "совместимость" &&
                             !lowerSearch.Contains(subAv.Value
                                 .ToLower()) && // Текст телефона не содержит эту модель (например, iPhone 15 не содержит iphone 14 pro)
                             (subAv.Value.ToLower().Contains("iphone") ||
                              subAv.Value.ToLower().Contains("galaxy")) // Проверяем только конкретные модели
                     )
                    )
                )
            );
        }
        // ==============================================================================

        int totalCount = query.Count();

        var paginatedQuery = query
            .OrderByDescending(product => product.Id)
            .Skip((validPageNumber - 1) * validPageSize)
            .Take(validPageSize);

        var products = await _productRepository.ToListAsync(paginatedQuery);

        var producttDtos = _mapper.Map<IEnumerable<ProductDto>>(products);

        return new PagedResult<ProductDto>(producttDtos, totalCount, validPageNumber, validPageSize);
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

    public async Task<IEnumerable<ProductDto>> GetCompatibleProductsAsync(int productId)
    {
        // 1. Запрашиваем из репозитория сам товар
        var currentProduct = await _productRepository.GetByIdAsync(productId);
        if (currentProduct == null) return Enumerable.Empty<ProductDto>();

        // 2. Модель телефона — это просто его Name (по договорённости с фронтом/админкой)
        var phoneName = currentProduct.Name;

        // 3. Достаём типы разъёмов телефона (если это телефон с зарядкой через USB-C/Lightning)
        var connectorTypes = currentProduct.AttributeValues
            .Where(av => av.Attribute.Name.Equals("тип разъёма", StringComparison.OrdinalIgnoreCase)
                         || av.Attribute.Name.Equals("тип разъема", StringComparison.OrdinalIgnoreCase))
            .Select(av => av.Value.ToLower())
            .ToList();

        // 4. Вызываем метод репозитория, который ищет чехлы/зарядники/повербанки одним запросом
        var compatibleProducts = await _productRepository
            .GetCompatibleProductsByAttributesAsync(productId, phoneName, connectorTypes);

        // 5. Маппим сущности в DTO и возвращаем контроллеру
        return _mapper.Map<IEnumerable<ProductDto>>(compatibleProducts);
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