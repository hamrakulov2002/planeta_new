using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Planeta.Application.DTOs.Catalog;
using Planeta.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Planeta_New.Controllers;

[ApiController]
[Route("api/products")] // Базовый роут для всех методов
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    // GET: api/products
    [HttpGet]
    public async Task<IActionResult> GetAllAsync(
        [FromQuery] int? categoryId,
        [FromQuery] int? brandId,
        [FromQuery] bool? isUsed,
        [FromQuery] string? search,
        [FromQuery] int? pageNumber,
        [FromQuery] int? pageSize)
    {
        var products = await _productService.GetAllProductAsync(categoryId, brandId, isUsed, search, pageNumber, pageSize);
        return Ok(products);
    }

    // GET: api/products/{productId}
    [HttpGet("{productId}")]
    public async Task<IActionResult> GetByIdAsync(int productId) 
    {
        var product = await _productService.GetByIdAsync(productId);
        if (product == null)
        {
            return NotFound(new { message = $"Product with id {productId} not found" });
        }
        return Ok(product);
    }

    // GET: api/products/{productId}/compatible
    [HttpGet("{productId}/compatible")]
    public async Task<IActionResult> GetCompatibleAsync(int productId)
    {
        var products = await _productService.GetCompatibleProductsAsync(productId);
        return Ok(products);
    }

    // POST: api/products/create
    // ИСПРАВЛЕНО: Теперь путь красивый и системный: POST api/products/create
    [HttpPost("create")] 
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        int productId = await _productService.AddAsync(request);
        
        // Строим ссылку на метод UploadImages текущего контроллера
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
            return Ok(new { message = "Product images uploaded successfully" });
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(new { message = e.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Error saving images to server" });
        }
    }
    
    // PUT: api/products/{productId}
    [HttpPut("{productId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateProduct(int productId, [FromBody] UpdateProductDto updateProductDto)
    {
        try
        {
            await _productService.UpdateProductAsync(productId, updateProductDto);
            return Ok(new { message = "Product updated successfully" });
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(new { message = e.Message });
        }
        catch (Exception e)
        {
            return BadRequest(new { message = e.Message });
        }
    }

    // DELETE: api/products/{productId}
    [HttpDelete("{productId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteProduct(int productId)
    {
        try
        {
            await _productService.DeleteAsync(productId);
            return Ok(new { message = "Product deleted successfully" });
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(new { message = e.Message });
        }
        catch (Exception e)
        {
            return BadRequest(new { message = e.Message });
        }
    }
}