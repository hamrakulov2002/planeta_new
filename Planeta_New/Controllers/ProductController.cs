using Microsoft.AspNetCore.Mvc;
using Planeta.Application.DTOs.Catalog;
using Planeta.Application.Interfaces;
using Planeta.Domain.Entities;

namespace Planeta_New.Controllers;


[ApiController]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    [Route("/api/products")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllAsync(
        [FromQuery] int? categoryId,
        [FromQuery] int? brandId,
        [FromQuery] bool? isUsed)
    {
        var products = await _productService.GetCatalogAsync(categoryId, brandId, isUsed);
        return Ok(products);
        
    }


    [HttpGet]
    [Route("/api/products/{productId}")]
    public async Task<ActionResult<ProductDto>> GetByIdAsync(int productId) 
    {
        var product = await _productService.GetByIdAsync(productId);
        
        if (product == null)
        {
            return NotFound($"Product with id {productId} not found");
        }
        
        return Ok(product);
    }




    [HttpPost]
    [Route("/api/createpproduct")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // 1. Создаем продукт в базе и получаем его ID
        int productId = await _productService.AddAsync(request);
    
        // 2. Формируем URL для следующего шага (загрузки картинок)
        // Это создаст строку вида: /api/product/28/images
        var uploadImagesUrl = Url.Action(nameof(UploadImages), new { productId = productId });

        // 3. Возвращаем статус 201 Created. 
        // Он автоматически добавит заголовок Location и вернет JSON с ID для фронтенда
        return Created(uploadImagesUrl, new 
        { 
            ProductId = productId, 
            NextStep = uploadImagesUrl,
            Message = "Продукт успешно создан. Перенаправление на загрузку изображений." 
        });
    }

    [HttpPost("{productId}/images")]
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
    
    [HttpPut("{productId}")]
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

    [HttpDelete("{productId}")]
    public async Task<IActionResult> DeleteProduct(int productId)
    {
         await _productService.DeleteAsync(productId);
         return  Ok(new { message = "Product is deleted successfully" });
    }
    
}