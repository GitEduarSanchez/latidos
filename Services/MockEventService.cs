using Latidos.Models;

namespace Latidos.Services;

/// <summary>
/// Mock Event Service para testing y desarrollo
/// Retorna datos de prueba sin conectar a base de datos
/// </summary>
public class MockEventService : IEventService
{
    private readonly List<RunningEvent> _events;

    public MockEventService()
    {
        _events = GenerateMockEvents();
    }

    private List<RunningEvent> GenerateMockEvents()
    {
        return new List<RunningEvent>
        {
            new RunningEvent
            {
                Id = 1,
                Name = "5K City Morning Run",
                Description = "A fun and casual 5K run through downtown. Perfect for beginners.",
                EventDate = DateTime.Now.AddDays(7),
                Price = 25.00m,
                Location = "Central Park",
                MaxParticipants = 500,
                CurrentParticipants = 245,
                ImageUrl = "event_5k_city.svg"
            },
            new RunningEvent
            {
                Id = 2,
                Name = "Half Marathon Championship",
                Description = "Competitive 21K race with professional timing and support.",
                EventDate = DateTime.Now.AddDays(14),
                Price = 45.00m,
                Location = "Downtown District",
                MaxParticipants = 300,
                CurrentParticipants = 156,
                ImageUrl = "event_half_marathon.svg"
            },
            new RunningEvent
            {
                Id = 3,
                Name = "Trail Running Adventure",
                Description = "10K scenic trail run through mountain paths. Intermediate level.",
                EventDate = DateTime.Now.AddDays(21),
                Price = 35.00m,
                Location = "Mountain Trail Park",
                MaxParticipants = 200,
                CurrentParticipants = 87,
                ImageUrl = "event_trail_running.svg"
            },
            new RunningEvent
            {
                Id = 4,
                Name = "Women's 10K Charity Run",
                Description = "10K fun run supporting local women's healthcare initiatives.",
                EventDate = DateTime.Now.AddDays(30),
                Price = 30.00m,
                Location = "Riverside Track",
                MaxParticipants = 400,
                CurrentParticipants = 320,
                ImageUrl = "event_charity_run.svg"
            },
            new RunningEvent
            {
                Id = 5,
                Name = "Sprint Series - Week 1",
                Description = "Fast-paced 3K sprint for speed enthusiasts. Advanced runners.",
                EventDate = DateTime.Now.AddDays(3),
                Price = 20.00m,
                Location = "Athletic Complex",
                MaxParticipants = 150,
                CurrentParticipants = 145,
                ImageUrl = "event_sprint_series.svg"
            },
            new RunningEvent
            {
                Id = 6,
                Name = "Night Glow Run",
                Description = "Evening 5K run with LED lights and DJ stations.",
                EventDate = DateTime.Now.AddDays(10),
                Price = 35.00m,
                Location = "Downtown Loop",
                MaxParticipants = 600,
                CurrentParticipants = 412,
                ImageUrl = "event_night_glow.svg"
            }
        };
    }

    public Task<List<RunningEvent>> GetAllEventsAsync()
    {
        // Simular latencia de base de datos
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
