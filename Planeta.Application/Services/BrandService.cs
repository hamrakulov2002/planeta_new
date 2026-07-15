using AutoMapper;
using Planeta.Application.DTOs.Brand;
using Planeta.Application.Exceptions;
using Planeta.Application.Interfaces;
using Planeta.Domain.Entities;
using Planeta.Domain.Interfaces;

namespace Planeta.Application.Services;

public class BrandService : IBrandService
{
    private readonly IBrandRepository  _brandRepository;
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public BrandService(IProductRepository productRepository,IBrandRepository brandRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _brandRepository = brandRepository;
        _mapper = mapper;
    }
    
    public async Task<IEnumerable<BrandDto>> GetBrandsAsync()
    {
        var brands = await _brandRepository.GetAllAsync();
        
        return _mapper.Map<IEnumerable<BrandDto>>(brands);
    }

    public async Task<BrandDto?> GetBrandAsync(int id)
    {
        var brand = await _brandRepository.GetByIdAsync(id);

        if (brand == null)
        {
            throw new BrandNotFoundException(id);
        }
        
        return _mapper.Map<BrandDto>(brand);

    }

    public async Task<BrandDto?> CreateBrandAsync(CreateBrandDto createBrandDto)
    {
        var brands = await _brandRepository.GetAllAsync();

        if (brands.Any(b => b.Name.ToLower() == createBrandDto.Name.ToLower()))
        {
            throw new Exception("Brand name already exists");
        }
        
        var brand = _mapper.Map<Brand>(createBrandDto);
        
        await _brandRepository.AddAsync(brand);
        await _brandRepository.SaveChangesAsync();
        
        return _mapper.Map<BrandDto>(brand);
    }

    public async Task UpdateBrandAsync(int id, CreateBrandDto brandDto)
    {
        var brand = await _brandRepository.GetByIdAsync(id);

        if (brand == null)
        {
            throw new BrandNotFoundException(id);
        }
        
        brand.Name = brandDto.Name;
        await _brandRepository.SaveChangesAsync();
        
    }

    public async Task DeleteBrandAsync(int id)
    {
        var brand = await _brandRepository.GetByIdAsync(id);

        if (brand == null)
        {
            throw new BrandNotFoundException(id);
        }
        
        _brandRepository.Delete(brand);
        await _brandRepository.SaveChangesAsync();
        
        
    }

    public async Task<IEnumerable<BrandDto>> GetBrandsByCategoryAsync(int categoryId)
    {
        var brands = await _productRepository.GetBrandsByCategoryIdAsync(categoryId);
        return _mapper.Map<IEnumerable<BrandDto>>(brands);
    }
}