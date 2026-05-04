using Latidos.Models;

namespace Latidos.Services;

public interface ICompetitorService
{
    Task<List<CompetitorProfile>> SearchCompetitorsAsync(string query);
    Task<List<CompetitorProfile>> GetAllCompetitorsAsync();
    Task SaveCompetitorAsync(CompetitorProfile competitor);
}
