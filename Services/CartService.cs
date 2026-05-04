using Latidos.Models;

namespace Latidos.Services;

public class CartService : ICartService
{
    private readonly List<CartItem> _cartItems = new();

    public Task<List<CartItem>> GetCartItemsAsync()
    {
        return Task.FromResult(_cartItems.ToList());
    }

    public Task<bool> AddToCartAsync(CartItem item)
    {
        try
        {
            var existingItem = _cartItems.FirstOrDefault(ci => ci.EventId == item.EventId);
            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                item.Id = _cartItems.Count > 0 ? _cartItems.Max(ci => ci.Id) + 1 : 1;
                _cartItems.Add(item);
            }
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public Task<bool> RemoveFromCartAsync(int cartItemId)
    {
        try
        {
            var item = _cartItems.FirstOrDefault(ci => ci.Id == cartItemId);
            if (item != null)
            {
                _cartItems.Remove(item);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public Task<bool> UpdateQuantityAsync(int cartItemId, int quantity)
    {
        try
        {
            var item = _cartItems.FirstOrDefault(ci => ci.Id == cartItemId);
            if (item != null)
            {
                item.Quantity = quantity;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public Task ClearCartAsync()
    {
        _cartItems.Clear();
        return Task.CompletedTask;
    }

    public Task<decimal> GetCartTotalAsync()
    {
        var total = _cartItems.Sum(ci => ci.TotalPrice);
        return Task.FromResult(total);
    }
}
