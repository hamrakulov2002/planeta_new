using Planeta.Application.DTOs.Orders;

namespace Planeta.Application.Interfaces;

public interface IOrderService
{
    Task<int> CreateOrderAsync(string? userId, CreateOrderRequest request);
    
    Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(string userId);
    
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
}