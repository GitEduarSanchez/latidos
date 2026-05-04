namespace Latidos.Models;

public class CompetitorResult
{
    public int EventId { get; set; }
    public string CompetitorDocument { get; set; } = string.Empty;
    public string CompetitorName { get; set; } = string.Empty;
    public string CompetitorNumber { get; set; } = string.Empty;
    public TimeSpan? OfficialTime { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
