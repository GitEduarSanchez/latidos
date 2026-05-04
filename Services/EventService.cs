using Latidos.Models;

namespace Latidos.Services;

public class EventService : IEventService
{
    private readonly List<RunningEvent> _events = new();

    public EventService()
    {
        InitializeEvents();
    }

    private void InitializeEvents()
    {
        _events.AddRange(new[]
        {
            new RunningEvent
            {
                Id = 1,
                Name = "Carrera urbana 5K",
                Description = "Una carrera divertida de 5K por el centro de la ciudad",
                EventDate = DateTime.Now.AddDays(30),
                Price = 25.00m,
                Location = "Parque del Centro",
                MaxParticipants = 500,
                CurrentParticipants = 245,
                ImageUrl = "event_5k_city.svg"
            },
            new RunningEvent
            {
                Id = 2,
                Name = "Reto media maraton",
                Description = "Media maraton de 21K con cronometraje profesional",
                EventDate = DateTime.Now.AddDays(60),
                Price = 45.00m,
                Location = "Distrito Centro",
                MaxParticipants = 300,
                CurrentParticipants = 156,
                ImageUrl = "event_half_marathon.svg"
            },
            new RunningEvent
            {
                Id = 3,
                Name = "Aventura trail running",
                Description = "Carrera trail de 10K por senderos de montana",
                EventDate = DateTime.Now.AddDays(45),
                Price = 35.00m,
                Location = "Sendero Montana",
                MaxParticipants = 200,
                CurrentParticipants = 87,
                ImageUrl = "event_trail_running.svg"
            },
        });
    }

    public Task<List<RunningEvent>> GetAllEventsAsync()
    {
        return Task.FromResult(_events.ToList());
    }

    public Task<RunningEvent?> GetEventByIdAsync(int eventId)
    {
        var runningEvent = _events.FirstOrDefault(e => e.Id == eventId);
        return Task.FromResult(runningEvent);
    }

    public Task<bool> AddEventAsync(RunningEvent runningEvent)
    {
        try
        {
            runningEvent.Id = _events.Max(e => e.Id) + 1;
            _events.Add(runningEvent);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public Task<bool> UpdateEventAsync(RunningEvent runningEvent)
    {
        try
        {
            var existingEvent = _events.FirstOrDefault(e => e.Id == runningEvent.Id);
            if (existingEvent != null)
            {
                _events.Remove(existingEvent);
                _events.Add(runningEvent);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }
}
