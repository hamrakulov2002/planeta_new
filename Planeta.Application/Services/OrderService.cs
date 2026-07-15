using Planeta.Application.DTOs.Orders;
using Planeta.Application.Interfaces;
using Planeta.Domain.Entities;
using Planeta.Domain.Interfaces;

namespace Planeta.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;

    public OrderService(IOrderRepository orderRepository, IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    public async Task<int> CreateOrderAsync(string? userId, CreateOrderRequest request)
    {
        if (request.Items == null || !request.Items.Any())
            throw new ArgumentException("Нельзя оформить заказ с пустой корзиной.");

        var order = new Order
        {
            UserName = request.CustomerName,
            PhoneNumber = request.PhoneNumber,
            Comment = request.Comment,
            OrderDate = DateTime.UtcNow,
            IsProcessed = false,
            TotalPrice = 0
        };

        // Если в будущем добавишь поле UserId в класс Order, раскомментируй строку ниже:
        // order.UserId = userId;

        foreach (var cartItem in request.Items)
        {
            var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
            if (product == null)
                throw new KeyNotFoundException($"Товар с ID {cartItem.ProductId} не найден в каталоге.");

            if (product.Quantity < cartItem.Quantity)
                throw new InvalidOperationException($"Недостаточно товара '{product.Name}' на складе. Доступно: {product.Quantity} шт.");

            product.Quantity -= cartItem.Quantity;

            var orderItem = new OrderItem
            {
                ProductId = product.Id,
                Quantity = cartItem.Quantity,
                PriceAtPurchase = product.Price
            };

            order.Items.Add(orderItem);
            order.TotalPrice += orderItem.PriceAtPurchase * orderItem.Quantity;
        }

        await _orderRepository.AddAsync(order);
        await _orderRepository.SaveChangesAsync();

        return order.Id;
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(string userId)
    {
        var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);
        return MapToDtoList(orders);
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        var orders = await _orderRepository.GetAllOrdersWithItemsAsync();
        return MapToDtoList(orders);
    }

    private IEnumerable<OrderDto> MapToDtoList(IEnumerable<Order> orders)
    {
        return orders.Select(o => new OrderDto(
            o.Id,
            o.OrderDate,
            o.UserName,
            o.PhoneNumber,
            o.Comment,
            o.TotalPrice,
            o.IsProcessed,
            o.Items.Select(i => new OrderItemDto(
                i.Id,
                i.ProductId,
                i.Product?.Name ?? "Товар удален из каталога",
                i.Quantity,
                i.PriceAtPurchase
            )).ToList()
        ));
    }
}