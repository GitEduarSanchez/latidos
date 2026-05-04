using Latidos.Models;

namespace Latidos.Services;

public interface IEventService
{
    Task<List<RunningEvent>> GetAllEventsAsync();
    Task<RunningEvent?> GetEventByIdAsync(int eventId);
    Task<bool> AddEventAsync(RunningEvent runningEvent);
    Task<bool> UpdateEventAsync(RunningEvent runningEvent);
    Task<bool> DeleteEventAsync(int eventId);
}
