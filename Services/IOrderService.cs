using Latidos.Models;

namespace Latidos.Services;

public interface IOrderService
{
    Task<bool> CreateOrderAsync(Order order);
    Task<Order?> GetOrderByIdAsync(int orderId);
    Task<List<Order>> GetAllOrdersAsync();
    Task<List<Order>> GetUserOrdersAsync(string customerEmail);
    Task<bool> UpdateOrderStatusAsync(int orderId, string status);
}
