namespace Latidos.Models;

public class CartItem
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public RunningEvent? Event { get; set; }
    public CompetitorProfile? Competitor { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }

    public decimal TotalPrice => Price * Quantity;
    public string TotalPriceText => CurrencyFormatter.FormatCop(TotalPrice);
    public string QuantityText => Quantity == 1 ? "1 inscripcion" : $"{Quantity} inscripciones";
    public string CompetitorNameText => Competitor?.FullName ?? "Competidor sin nombre";
    public string CompetitorDocumentText => Competitor == null ? "Documento pendiente" : $"{Competitor.DocumentType}: {Competitor.DocumentNumber}";
    public string CompetitorNumberText => string.IsNullOrWhiteSpace(Competitor?.CompetitorNumber) ? "Sin numero" : $"No. {Competitor.CompetitorNumber}";
    public string CompetitorAgeText => Competitor == null ? "Categoria pendiente" : $"{Competitor.Age} anos - {Competitor.AgeCategory}";
    public string CompetitorPhotoPath => Competitor?.PhotoPath ?? string.Empty;
    public bool HasCompetitorPhoto => Competitor?.HasPhoto == true;
}
