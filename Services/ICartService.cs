using Latidos.Models;

namespace Latidos.Services;

public interface ICartService
{
    Task<List<CartItem>> GetCartItemsAsync();
    Task<bool> AddToCartAsync(CartItem item);
    Task<bool> RemoveFromCartAsync(int cartItemId);
    Task<bool> UpdateQuantityAsync(int cartItemId, int quantity);
    Task<bool> UpdateCompetitorAsync(int cartItemId, CompetitorProfile competitor);
    Task ClearCartAsync();
    Task<decimal> GetCartTotalAsync();
}
