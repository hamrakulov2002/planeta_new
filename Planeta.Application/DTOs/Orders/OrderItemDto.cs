namespace Planeta.Application.DTOs.Orders;

public record OrderItemDto(
    int Id,
    int ProductId,
    string ProductName,
    int Quantity,
    decimal PriceAtPurchase
);