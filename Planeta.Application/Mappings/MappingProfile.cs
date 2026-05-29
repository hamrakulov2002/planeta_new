using AutoMapper;
using Planeta.Application.DTOs.Brand;
using Planeta.Application.DTOs.Catalog;
using Planeta.Domain.Entities;

namespace Planeta.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // 1. Маппинг из сущности БД (Product) в DTO для фронтенда (ProductDto)
        CreateMap<Product, ProductDto>()
            // Извлекаем имя категории, если она подгружена
            .ForMember(dest => dest.CategoryName,
                opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
            
            // Извлекаем имя бренда
            .ForMember(dest => dest.BrandName, 
                opt => opt.MapFrom(src => src.Brand != null ? src.Brand.Name : null))
            
            // Логика выбора главной картинки
            .ForMember(dest => dest.MainImageUrl, opt => opt.MapFrom(src =>
                src.Images.FirstOrDefault(i => i.IsMain) != null 
                    ? src.Images.FirstOrDefault(i => i.IsMain)!.ImagePath
                    : src.Images.FirstOrDefault() != null 
                        ? src.Images.FirstOrDefault()!.ImagePath 
                        : null))
            
            // Собираем пути всех картинок в массив строк
            .ForMember(dest => dest.ImageUrls, 
                opt => opt.MapFrom(src => src.Images.Select(i => i.ImagePath).ToList()))
            
            // ИСПРАВЛЕНО: Безопасная трансформация связей EAV в плоский список с защитой от AsNoTracking
            .ForMember(dest => dest.Attributes, opt => opt.MapFrom(src =>
                src.AttributeValues != null 
                    ? src.AttributeValues.Select(av => new ProductAttributeResponseDto
                      {
                          // Если имя самого атрибута не успело подгрузиться из таблицы Attributes, 
                          // выводим заглушку, чтобы не потерять значение и не вернуть пустой массив
                          AttributeName = av.Attribute != null ? av.Attribute.Name : "Характеристика",
                          Value = av.Value ?? string.Empty
                      }).ToList()
                    : new List<ProductAttributeResponseDto>()));

        // 2. Маппинг из класса запроса формы (с IFormFile) в сущность БД
        CreateMap<CreateProductRequest, Product>()
            // Синхронизируем StockQuantity из формы с полем Quantity в базе данных
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.StockQuantity))
            // Игнорируем картинки и атрибуты, так как мы их обрабатываем по очереди вручную в сервисе
            .ForMember(dest => dest.Images, opt => opt.Ignore())
            .ForMember(dest => dest.AttributeValues, opt => opt.Ignore());
        
        // 3. Маппинг категорий
        CreateMap<CategoryDto, Category>().ReverseMap();
        CreateMap<CreateCategoryDto, Category>().ReverseMap();
        
        // 4. Маппинг брендов
        CreateMap<BrandDto, Brand>().ReverseMap();
        CreateMap<CreateBrandDto, Brand>().ReverseMap();

        // 5. Маппинг для обновления продукта
        CreateMap<UpdateProductDto, Product>()
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.StockQuantity))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Images, opt => opt.Ignore())
            .ForMember(dest => dest.AttributeValues, opt => opt.Ignore());
    }
}