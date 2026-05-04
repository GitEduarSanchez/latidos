using Latidos.Models;

namespace Latidos.Services;

public class OrderService : IOrderService
{
    private readonly List<Order> _orders = new();

    public Task<bool> CreateOrderAsync(Order order)
    {
        try
        {
            order.Id = _orders.Count > 0 ? _orders.Max(o => o.Id) + 1 : 1;
            order.OrderNumber = $"ORD-{DateTime.Now:yyyyMMddHHmmss}";
            order.OrderDate = DateTime.UtcNow;
            _orders.Add(order);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public Task<Order?> GetOrderByIdAsync(int orderId)
    {
        var order = _orders.FirstOrDefault(o => o.Id == orderId);
        return Task.FromResult(order);
    }

    public Task<List<Order>> GetAllOrdersAsync()
    {
        return Task.FromResult(_orders.ToList());
    }

    public Task<List<Order>> GetUserOrdersAsync(string customerEmail)
    {
        var userOrders = _orders.Where(o => o.CustomerEmail == customerEmail).ToList();
        return Task.FromResult(userOrders);
    }

    public Task<bool> UpdateOrderStatusAsync(int orderId, string status)
    {
        try
        {
            var order = _orders.FirstOrDefault(o => o.Id == orderId);
            if (order != null)
            {
                order.Status = status;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }
}
