using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using Latidos.Models;
using Latidos.Services;

namespace Latidos.ViewModels;

public class AdminTimesViewModel : BindableObject
{
    private readonly IEventService _eventService;
    private readonly IOrderService _orderService;
    private readonly IResultService _resultService;

    public ObservableCollection<RunningEvent> Events { get; } = new();
    public ObservableCollection<AdminTimeRow> Rows { get; } = new();

    private RunningEvent? _selectedEvent;
    public RunningEvent? SelectedEvent
    {
        get => _selectedEvent;
        set
        {
            if (_selectedEvent == value) return;
            _selectedEvent = value;
            OnPropertyChanged();
            _ = LoadRowsAsync();
        }
    }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage == value) return;
            _statusMessage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasStatusMessage));
        }
    }

    public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);

    public ICommand SaveTimesCommand { get; }

    public AdminTimesViewModel()
    {
        _eventService = IPlatformApplication.Current?.Services.GetService<IEventService>() ?? new EventService();
        _orderService = IPlatformApplication.Current?.Services.GetService<IOrderService>() ?? new OrderService();
        _resultService = IPlatformApplication.Current?.Services.GetService<IResultService>() ?? new ResultService();
        SaveTimesCommand = new Command(async () => await SaveTimesAsync());
        _ = LoadEventsAsync();
    }

    private async Task LoadEventsAsync()
    {
        var list = await _eventService.GetAllEventsAsync();
        Events.Clear();
        foreach (var evt in list.OrderBy(e => e.EventDate))
        {
            Events.Add(evt);
        }

        SelectedEvent = Events.FirstOrDefault();
    }

    private async Task LoadRowsAsync()
    {
        Rows.Clear();
        StatusMessage = string.Empty;
        if (SelectedEvent == null)
        {
            return;
        }

        var orders = await _orderService.GetAllOrdersAsync();
        var registered = orders
            .SelectMany(o => o.Items)
            .Where(i => i.EventId == SelectedEvent.Id && !string.IsNullOrWhiteSpace(i.CompetitorDocument))
            .GroupBy(i => i.CompetitorDocument)
            .Select(g => g.Last())
            .ToList();

        var saved = await _resultService.GetResultsByEventAsync(SelectedEvent.Id);
        var map = saved.ToDictionary(s => s.CompetitorDocument, s => s);

        foreach (var item in registered)
        {
            map.TryGetValue(item.CompetitorDocument, out var existing);
            Rows.Add(new AdminTimeRow
            {
                CompetitorDocument = item.CompetitorDocument,
                CompetitorName = item.CompetitorName,
                CompetitorNumber = item.CompetitorNumber,
                TimeText = existing?.OfficialTime?.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture) ?? string.Empty
            });
        }
    }

    private async Task SaveTimesAsync()
    {
        if (SelectedEvent == null)
        {
            StatusMessage = "Selecciona un evento.";
            return;
        }

        var output = new List<CompetitorResult>();
        foreach (var row in Rows)
        {
            if (!string.IsNullOrWhiteSpace(row.TimeText) &&
                TimeSpan.TryParseExact(row.TimeText.Trim(), @"hh\:mm\:ss", CultureInfo.InvariantCulture, out var time))
            {
                output.Add(new CompetitorResult
                {
                    EventId = SelectedEvent.Id,
                    CompetitorDocument = row.CompetitorDocument,
                    CompetitorName = row.CompetitorName,
                    CompetitorNumber = row.CompetitorNumber,
                    OfficialTime = time,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        await _resultService.SaveResultsAsync(SelectedEvent.Id, output);
        StatusMessage = "Tiempos guardados. Formato esperado: HH:MM:SS";
    }
}

public class AdminTimeRow : BindableObject
{
    public string CompetitorDocument { get; set; } = string.Empty;
    public string CompetitorName { get; set; } = string.Empty;
    public string CompetitorNumber { get; set; } = string.Empty;

    private string _timeText = string.Empty;
    public string TimeText
    {
        get => _timeText;
        set
        {
            if (_timeText == value) return;
            _timeText = value;
            OnPropertyChanged();
        }
    }
}
