using Microsoft.AspNetCore.Http;

namespace Planeta.Application.DTOs.Catalog;

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public int? BrandId { get; set; }
    public bool IsUsed { get; set; }
    public string? Imei { get; set; }
    public int StockQuantity { get; set; }

    /*public IFormFile? MainImage { get; set; }        // Главное фото товара
    public List<IFormFile>? ExtraImages { get; set; } // Дополнительные фотографии*/

    public List<ProductAttributeInputDto> Attributes { get; set; } = new();
}