using Planeta.Application.DTOs.PhoneOptions;

namespace Planeta.Application.DTOs.Catalog;

public record UpdateProductDto(
    string Name,
    string Description,
    decimal Price,
    int CategoryId,
    int? BrandId,
    bool IsUsed,
    string? Imei,
    int StockQuantity,
    string MainImageUrl,
    List<string> ImageUrls,
    List<ProductAttributeInputDto> Attributes 
);