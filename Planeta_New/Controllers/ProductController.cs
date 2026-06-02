using Microsoft.AspNetCore.Authorization; // Обязательно добавляем этот using
using Microsoft.AspNetCore.Mvc;
using Planeta.Application.DTOs.Catalog;
using Planeta.Application.Interfaces;
using Planeta.Domain.Entities;

namespace Planeta_New.Controllers;

[ApiController]
[Route("api/products")] // Базовый роут для всех продуктов
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    // GET: api/products
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllAsync(
        [FromQuery] int? categoryId,
        [FromQuery] int? brandId,
        [FromQuery] bool? isUsed)
    {
        var products = await _productService.GetCatalogAsync(categoryId, brandId, isUsed);
        return Ok(products);
    }

    // GET: api/products/{productId}
    [HttpGet("{productId}")]
    public async Task<ActionResult<ProductDto>> GetByIdAsync(int productId) 
    {
        var product = await _productService.GetByIdAsync(productId);
        if (product == null)
        {
            return NotFound($"Product with id {productId} not found");
        }
        return Ok(product);
    }

    // POST: api/products
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        int productId = await _productService.AddAsync(request);
        var uploadImagesUrl = Url.Action(nameof(UploadImages), new { productId = productId });

        return Created(uploadImagesUrl, new 
        { 
            ProductId = productId, 
            NextStep = uploadImagesUrl,
            Message = "Продукт успешно создан. Перенаправление на загрузку изображений." 
        });
    }

    // POST: api/products/{productId}/images
    [HttpPost("{productId}/images")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UploadImages(int productId, [FromForm] UploadProductImagesRequest request)
    {
        try
        {
            await _productService.UploadImagesAsync(productId, request);
            return Ok(new { message = "Product is uploaded successfully" });
        }
        catch (KeyNotFoundException e)
        {
            return NotFound( new  { message = e.Message });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { message = "Error to save images" });
        }
    }
    
    // PUT: api/products/{productId}
    [HttpPut("{productId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<UpdateProductDto>> UpdateProduct(int productId, [FromBody] UpdateProductDto updateProductDto)
    {
        try
        {
            await _productService.UpdateProductAsync(productId, updateProductDto);
            return Ok(new { message = "Product is updated successfully" });
        }
        catch (Exception e)
        {
            return NotFound(e.Message);
        }
    }

    // DELETE: api/products/{productId}
    [HttpDelete("{productId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteProduct(int productId)
    {
         await _productService.DeleteAsync(productId);
         return Ok(new { message = "Product is deleted successfully" });
    }
}