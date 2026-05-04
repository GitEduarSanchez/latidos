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
                Name = "Carrera urbana 5K",
                Description = "Una carrera casual de 5K por el centro. Ideal para principiantes.",
                EventDate = DateTime.Now.AddDays(7),
                Price = 25.00m,
                City = "Santo Domingo",
                Location = "Parque Central",
                MaxParticipants = 500,
                CurrentParticipants = 245,
                ImageUrl = "event_5k_city.svg"
            },
            new RunningEvent
            {
                Id = 2,
                Name = "Campeonato de media maraton",
                Description = "Carrera competitiva de 21K con cronometraje profesional y apoyo en ruta.",
                EventDate = DateTime.Now.AddDays(14),
                Price = 45.00m,
                City = "Santo Domingo",
                Location = "Distrito Centro",
                MaxParticipants = 300,
                CurrentParticipants = 156,
                ImageUrl = "event_half_marathon.svg"
            },
            new RunningEvent
            {
                Id = 3,
                Name = "Aventura trail running",
                Description = "Recorrido escenico de 10K por senderos de montana. Nivel intermedio.",
                EventDate = DateTime.Now.AddDays(21),
                Price = 35.00m,
                City = "Jarabacoa",
                Location = "Parque Sendero Montana",
                MaxParticipants = 200,
                CurrentParticipants = 87,
                ImageUrl = "event_trail_running.svg"
            },
            new RunningEvent
            {
                Id = 4,
                Name = "Carrera solidaria 10K",
                Description = "Carrera recreativa de 10K en apoyo a iniciativas locales de salud.",
                EventDate = DateTime.Now.AddDays(30),
                Price = 30.00m,
                City = "Santiago",
                Location = "Pista Ribera",
                MaxParticipants = 400,
                CurrentParticipants = 320,
                ImageUrl = "event_charity_run.svg"
            },
            new RunningEvent
            {
                Id = 5,
                Name = "Serie sprint - Semana 1",
                Description = "Sprint rapido de 3K para corredores que buscan velocidad. Nivel avanzado.",
                EventDate = DateTime.Now.AddDays(-5),
                Price = 20.00m,
                City = "Santo Domingo",
                Location = "Complejo Atletico",
                MaxParticipants = 150,
                CurrentParticipants = 145,
                ImageUrl = "event_sprint_series.svg"
            },
            new RunningEvent
            {
                Id = 6,
                Name = "Carrera nocturna glow",
                Description = "Carrera nocturna de 5K con luces LED y estaciones de musica.",
                EventDate = DateTime.Now.AddDays(10),
                Price = 35.00m,
                City = "Punta Cana",
                Location = "Circuito Centro",
                MaxParticipants = 600,
                CurrentParticipants = 412,
                ImageUrl = "event_night_glow.svg",
                Status = EventStatus.Cancelled
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
            runningEvent.Id = _events.Count == 0 ? 1 : _events.Max(e => e.Id) + 1;
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
                _events.Sort((left, right) => left.EventDate.CompareTo(right.EventDate));
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public Task<bool> DeleteEventAsync(int eventId)
    {
        try
        {
            var existingEvent = _events.FirstOrDefault(e => e.Id == eventId);
            if (existingEvent == null)
            {
                return Task.FromResult(false);
            }

            _events.Remove(existingEvent);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }
}
