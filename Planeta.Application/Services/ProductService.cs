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

            if (validPageSize == 6)
            {
                query = query.Where(product =>
                    !product.Category.Name.ToLower().Contains("телефон") &&
                    !product.Category.Name.ToLower().Contains("смартфон")
                );
            }

            var searchWords = lowerSearch.Split(new[] { ' ', ',', '-', '/' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(word => word.Length > 2)
                .Where(word => word != "pro" && word != "max" && word != "ultra" && word != "plus")
                .ToList();

            var connectorKeywords = new List<string> { "usb-c", "usb c", "lightning", "micro usb", "type-c", "type c" };
            var foundConnectors = connectorKeywords.Where(c => lowerSearch.Contains(c)).ToList();

            query = query.Where(product =>
                product.Name.ToLower().Contains(lowerSearch) ||
                (product.Description != null && product.Description.ToLower().Contains(lowerSearch)) ||

                product.AttributeValues.Any(av =>
                    ((product.Category.Name.ToLower().Contains("чехол") ||
                      product.Category.Name.ToLower().Contains("стекл") ||
                      product.Category.Name.ToLower().Contains("пленк"))
                        ? (av.Attribute.Name.ToLower() == "совместимость" && lowerSearch.Contains(av.Value.ToLower()))

                        : (searchWords.Any(word => av.Value.ToLower().Contains(word)) ||
                           searchWords.Any(word => word.Contains(av.Value.ToLower())))
                    ) ||
((av.Attribute.Name.ToLower() == "тип разъёма" || av.Attribute.Name.ToLower() == "тип разъема") &&
                     (foundConnectors.Any(c => av.Value.ToLower().Contains(c)) ||
                      av.Value.ToLower().Contains("usb-c") || av.Value.ToLower().Contains("type-c")) &&
 !product.AttributeValues.Any(subAv =>
                             subAv.Attribute.Name.ToLower() == "совместимость" &&
                             !lowerSearch.Contains(subAv.Value
                                 .ToLower()) && 
                             (subAv.Value.ToLower().Contains("iphone") ||
                              subAv.Value.ToLower().Contains("galaxy")) 
                     )
                    )
                )
            );
        }

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
        var product = _mapper.Map<Product>(request);
        product.Id = 0; 
        product.AttributeValues = new List<ProductAttributeValue>();

        await _productRepository.AddAsync(product);
        await _productRepository.SaveChangesAsync();

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

        return product.Id;
    }

    public async Task UploadImagesAsync(int productId, UploadProductImagesRequest request)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null) throw new KeyNotFoundException("Продукт не найден");

        var webRoot = _webHostEnvironment.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
        var uploadFolder = Path.Combine(webRoot, "uploads", "products");
        if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

        var productImages = new List<ProductImage>();

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
            await _productRepository.AddImagesRangeAsync(productImages);
            await _productRepository.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<ProductDto>> GetCompatibleProductsAsync(int productId)
    {
        var currentProduct = await _productRepository.GetByIdAsync(productId);
        if (currentProduct == null) return Enumerable.Empty<ProductDto>();

        var phoneName = currentProduct.Name;

        var connectorTypes = currentProduct.AttributeValues
            .Where(av => av.Attribute.Name.Equals("тип разъёма", StringComparison.OrdinalIgnoreCase)
                         || av.Attribute.Name.Equals("тип разъема", StringComparison.OrdinalIgnoreCase))
            .Select(av => av.Value.ToLower())
            .ToList();

        var compatibleProducts = await _productRepository
            .GetCompatibleProductsByAttributesAsync(productId, phoneName, connectorTypes);

        return _mapper.Map<IEnumerable<ProductDto>>(compatibleProducts);
    }


    public async Task UpdateProductAsync(int productId, UpdateProductDto productDto)
    {
        var existingProduct = await _productRepository.GetProductWithImagesAndAttributesAsync(productId);
        if (existingProduct == null) return;

        _mapper.Map(productDto, existingProduct);

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
                    Product = existingProduct 
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