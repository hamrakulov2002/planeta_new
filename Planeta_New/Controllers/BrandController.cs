using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Planeta.Application.DTOs.Brand;
using Planeta.Application.Exceptions;
using Planeta.Application.Interfaces;

namespace Planeta_New.Controllers;

[ApiController]
public class BrandController : ControllerBase
{
    private readonly IBrandService _brandService;
    
    public BrandController(IBrandService brandService)
    {
        _brandService = brandService;
    }

    // Только для Админов и Менеджеров
    [HttpPost]
    [Route("/api/createbrand")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<CreateBrandDto>> CreateBrand([FromBody]CreateBrandDto brand)
    {
        var createdBrand = await _brandService.CreateBrandAsync(brand);
        return Ok(createdBrand);
    }

    // Только для Админов и Менеджеров
    [HttpPut]
    [Route("/api/updatebrand/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<BrandDto>> UpdateBrand(int id, [FromBody] CreateBrandDto brand)
    {
        try
        {
            await _brandService.UpdateBrandAsync(id, brand);
            return Ok(new {message = "Brand updated successfully"});
        }
        catch (Exception e)
        {
            throw new BrandNotFoundException(e.Message);
        }
    }

    // Доступно ВСЕМ
    [HttpGet]
    [Route("/api/brands")]
    public async Task<ActionResult<IEnumerable<BrandDto>>> GetBrands()
    {
        var brands = await _brandService.GetBrandsAsync();
        return Ok(brands);
    }

    // Доступно ВСЕМ
    [HttpGet]
    [Route("/api/brand/{id}")]
    public async Task<ActionResult<BrandDto>> GetBrandById(int id)
    {
        var brand = await _brandService.GetBrandAsync(id);
        return Ok(brand);
    }

    // Только для Админов и Менеджеров
    [HttpDelete]
    [Route("/api/brand/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteBrand(int id)
    {
        if (id == 0)
        {
            throw new BrandNotFoundException(id);
        }
        await _brandService.DeleteBrandAsync(id);
        return NoContent();
    }
}