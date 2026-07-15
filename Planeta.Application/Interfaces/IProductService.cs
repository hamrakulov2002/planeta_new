using Planeta.Application.DTOs.Catalog;
using Planeta.Application.DTOs.Common;
using Planeta.Domain.Entities;

namespace Planeta.Application.Interfaces;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetAllProductAsync(
        int? categoryId,
        int? brandId,
        bool? IsUsed,
        string? search,
        int? pageNumber,
        int? pageSize);
    
    Task<IEnumerable<ProductDto>> GetCatalogAsync(int? categoryId, int? brandId, bool? IsUsed);
    Task<ProductDto> GetByIdAsync(int productId);
    
    Task UpdateProductAsync(int productId, UpdateProductDto productDto);
    
    Task<int> AddAsync(CreateProductRequest request);

    Task UploadImagesAsync(int productId, UploadProductImagesRequest request);

    Task<IEnumerable<ProductDto>> GetCompatibleProductsAsync(int productId);
    
    
    Task DeleteAsync(int productId);
}