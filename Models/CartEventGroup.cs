namespace Latidos.Models;

public class CartEventGroup
{
    public int EventId { get; set; }
    public RunningEvent? Event { get; set; }
    public List<CartItem> Items { get; set; } = new();

    public string EventName => Event?.Name ?? "Evento sin nombre";
    public DateTime EventDate => Event?.EventDate ?? DateTime.MinValue;
    public string EventImageUrl => Event?.ImageUrl ?? string.Empty;

    public int CompetitorCount => Items.Sum(i => Math.Max(i.Quantity, 1));
    public string CompetitorCountText => CompetitorCount == 1 ? "1 competidor" : $"{CompetitorCount} competidores";
    public decimal GroupTotal => Items.Sum(i => i.TotalPrice);
    public string GroupTotalText => CurrencyFormatter.FormatCop(GroupTotal);
}
