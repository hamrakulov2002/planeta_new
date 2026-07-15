namespace Planeta.Application.DTOs.Orders;

public record OrderDto(
    int Id,
    DateTime OrderDate,
    string UserName,
    string PhoneNumber,
    string? Comment,
    decimal TotalPrice,
    bool IsProcessed,
    List<OrderItemDto> Items
);