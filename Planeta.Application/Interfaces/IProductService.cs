using Planeta.Application.DTOs.Catalog;
using Planeta.Domain.Entities;

namespace Planeta.Application.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetCatalogAsync(int? categoryId, int? brandId, bool? IsUsed);
    Task<ProductDto> GetByIdAsync(int productId);
    
    Task UpdateProductAsync(int productId, UpdateProductDto productDto);
    
    Task<int> AddAsync(CreateProductRequest request);

    Task UploadImagesAsync(int productId, UploadProductImagesRequest request);
    
    Task DeleteAsync(int productId);
}