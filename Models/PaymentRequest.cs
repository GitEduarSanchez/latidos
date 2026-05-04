namespace Latidos.Models;

public class PaymentRequest
{
    public string TokenId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "COP";
    public string Description { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
}
