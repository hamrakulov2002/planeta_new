using Planeta.Domain.Entities;

namespace Planeta.Domain.Interfaces;

public interface IOrderRepository
{
    Task AddAsync(Order order);
    Task<Order?> GetByIdAsync(int id);
    Task<IEnumerable<Order>> GetAllAsync();
    Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userName);
    Task<IEnumerable<Order>> GetAllOrdersWithItemsAsync();
    Task SaveChangesAsync();
}