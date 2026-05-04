namespace Latidos.Models;

public class CartItem
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public RunningEvent? Event { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }

    public decimal TotalPrice => Price * Quantity;
}
