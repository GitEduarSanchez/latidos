using Latidos.Models;

namespace Latidos.Services;

public interface ICompetitorService
{
    Task<List<CompetitorProfile>> SearchCompetitorsAsync(string query);
    Task SaveCompetitorAsync(CompetitorProfile competitor);
}
