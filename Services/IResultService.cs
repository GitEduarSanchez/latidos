using Latidos.Models;

namespace Latidos.Services;

public interface IResultService
{
    Task<List<CompetitorResult>> GetResultsByEventAsync(int eventId);
    Task SaveResultsAsync(int eventId, IEnumerable<CompetitorResult> results);
}
