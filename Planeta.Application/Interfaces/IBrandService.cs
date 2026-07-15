using Planeta.Application.DTOs.Brand;
using Planeta.Domain.Entities;

namespace Planeta.Application.Interfaces;

public interface IBrandService
{
    Task<IEnumerable<BrandDto>> GetBrandsAsync();
    Task<BrandDto?> GetBrandAsync(int id);
    Task<IEnumerable<BrandDto>> GetBrandsByCategoryAsync(int categoryId);
    Task<BrandDto?> CreateBrandAsync(CreateBrandDto createBrandDto);
    Task UpdateBrandAsync(int id, CreateBrandDto brandDto);
    Task DeleteBrandAsync(int id);
}