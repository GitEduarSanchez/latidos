using System.Globalization;
using System.Windows.Input;
using Latidos.Models;
using Latidos.Services;

namespace Latidos.ViewModels;

public class AdminEventsViewModel : BindableObject
{
    private readonly IEventService _eventService;
    private int? _editingEventId;

    private List<RunningEvent> _events = new();
    public List<RunningEvent> Events
    {
        get => _events;
        set
        {
            if (_events != value)
            {
                _events = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EventsCountText));
            }
        }
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (_isLoading != value)
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }
    }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged();
            }
        }
    }

    private string _description = string.Empty;
    public string Description
    {
        get => _description;
        set
        {
            if (_description != value)
            {
                _description = value;
                OnPropertyChanged();
            }
        }
    }

    private string _city = string.Empty;
    public string City
    {
        get => _city;
        set
        {
            if (_city != value)
            {
                _city = value;
                OnPropertyChanged();
            }
        }
    }

    private string _location = string.Empty;
    public string Location
    {
        get => _location;
        set
        {
            if (_location != value)
            {
                _location = value;
                OnPropertyChanged();
            }
        }
    }

    private DateTime _eventDate = DateTime.Today.AddDays(7);
    public DateTime EventDate
    {
        get => _eventDate;
        set
        {
            if (_eventDate != value)
            {
                _eventDate = value;
                OnPropertyChanged();
            }
        }
    }

    private string _priceText = "0.00";
    public string PriceText
    {
        get => _priceText;
        set
        {
            if (_priceText != value)
            {
                _priceText = value;
                OnPropertyChanged();
            }
        }
    }

    private string _maxParticipantsText = "100";
    public string MaxParticipantsText
    {
        get => _maxParticipantsText;
        set
        {
            if (_maxParticipantsText != value)
            {
                _maxParticipantsText = value;
                OnPropertyChanged();
            }
        }
    }

    private string _currentParticipantsText = "0";
    public string CurrentParticipantsText
    {
        get => _currentParticipantsText;
        set
        {
            if (_currentParticipantsText != value)
            {
                _currentParticipantsText = value;
                OnPropertyChanged();
            }
        }
    }

    private string _selectedStatusText = "Activo";
    public string SelectedStatusText
    {
        get => _selectedStatusText;
        set
        {
            if (_selectedStatusText != value)
            {
                _selectedStatusText = value;
                OnPropertyChanged();
            }
        }
    }

    private string _selectedImageUrl = "event_5k_city.svg";
    public string SelectedImageUrl
    {
        get => _selectedImageUrl;
        set
        {
            if (_selectedImageUrl != value)
            {
                _selectedImageUrl = value;
                OnPropertyChanged();
            }
        }
    }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage != value)
            {
                _statusMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasStatusMessage));
            }
        }
    }

    public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);

    public bool IsEditing => _editingEventId.HasValue;

    public string FormTitle => IsEditing ? "Editar evento" : "Alta de evento";

    public string PrimaryActionText => IsEditing ? "Actualizar evento" : "Crear evento";

    public string EventsCountText => Events.Count == 1 ? "1 evento registrado" : $"{Events.Count} eventos registrados";

    public List<string> StatusOptions { get; } = new() { "Activo", "Cancelado" };

    public List<string> ImageOptions { get; } = new()
    {
        "event_5k_city.svg",
        "event_half_marathon.svg",
        "event_trail_running.svg",
        "event_charity_run.svg",
        "event_sprint_series.svg",
        "event_night_glow.svg"
    };

    public ICommand LoadEventsCommand { get; }
    public ICommand SaveEventCommand { get; }
    public ICommand NewEventCommand { get; }
    public ICommand EditEventCommand { get; }
    public ICommand DeleteEventCommand { get; }

    public AdminEventsViewModel()
    {
        _eventService = IPlatformApplication.Current?.Services.GetService<IEventService>() ?? new EventService();

        LoadEventsCommand = new Command(async () => await LoadEventsAsync());
        SaveEventCommand = new Command(async () => await SaveEventAsync());
        NewEventCommand = new Command(ClearForm);
        EditEventCommand = new Command<RunningEvent>(LoadEventForEditing);
        DeleteEventCommand = new Command<RunningEvent>(async runningEvent => await DeleteEventAsync(runningEvent));
    }

    public async Task LoadEventsAsync()
    {
        try
        {
            IsLoading = true;
            Events = (await _eventService.GetAllEventsAsync())
                .OrderBy(runningEvent => runningEvent.StartOrderGroup)
                .ThenBy(runningEvent => runningEvent.EventDate)
                .ToList();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task SaveEventAsync()
    {
        StatusMessage = string.Empty;

        if (!TryBuildEvent(out var runningEvent, out var validationMessage))
        {
            StatusMessage = validationMessage;
            return;
        }

        var saved = IsEditing
            ? await _eventService.UpdateEventAsync(runningEvent)
            : await _eventService.AddEventAsync(runningEvent);

        StatusMessage = saved
            ? (IsEditing ? "Evento actualizado correctamente." : "Evento creado correctamente.")
            : "No se pudo guardar el evento.";

        if (saved)
        {
            ClearForm();
            await LoadEventsAsync();
        }
    }

    private bool TryBuildEvent(out RunningEvent runningEvent, out string validationMessage)
    {
        runningEvent = new RunningEvent();
        validationMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Name))
        {
            validationMessage = "Escribe el nombre del evento.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(City))
        {
            validationMessage = "Escribe la ciudad del evento.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Location))
        {
            validationMessage = "Escribe el lugar del evento.";
            return false;
        }

        if (!TryParseDecimal(PriceText, out var price) || price < 0)
        {
            validationMessage = "Escribe un precio valido.";
            return false;
        }

        if (!int.TryParse(MaxParticipantsText, out var maxParticipants) || maxParticipants <= 0)
        {
            validationMessage = "Escribe una capacidad valida.";
            return false;
        }

        if (!int.TryParse(CurrentParticipantsText, out var currentParticipants) || currentParticipants < 0)
        {
            validationMessage = "Escribe una cantidad de inscritos valida.";
            return false;
        }

        if (currentParticipants > maxParticipants)
        {
            validationMessage = "Los inscritos no pueden superar la capacidad.";
            return false;
        }

        runningEvent = new RunningEvent
        {
            Id = _editingEventId ?? 0,
            Name = Name.Trim(),
            Description = Description.Trim(),
            City = City.Trim(),
            Location = Location.Trim(),
            EventDate = EventDate.Date,
            Price = price,
            MaxParticipants = maxParticipants,
            CurrentParticipants = currentParticipants,
            ImageUrl = string.IsNullOrWhiteSpace(SelectedImageUrl) ? "event_5k_city.svg" : SelectedImageUrl,
            Status = SelectedStatusText == "Cancelado" ? EventStatus.Cancelled : EventStatus.Active
        };

        return true;
    }

    private static bool TryParseDecimal(string value, out decimal result)
    {
        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out result) ||
               decimal.TryParse(value.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out result);
    }

    private void LoadEventForEditing(RunningEvent? runningEvent)
    {
        if (runningEvent == null)
        {
            return;
        }

        _editingEventId = runningEvent.Id;
        Name = runningEvent.Name;
        Description = runningEvent.Description;
        City = runningEvent.City;
        Location = runningEvent.Location;
        EventDate = runningEvent.EventDate;
        PriceText = runningEvent.Price.ToString("0", CultureInfo.InvariantCulture);
        MaxParticipantsText = runningEvent.MaxParticipants.ToString(CultureInfo.InvariantCulture);
        CurrentParticipantsText = runningEvent.CurrentParticipants.ToString(CultureInfo.InvariantCulture);
        SelectedStatusText = runningEvent.IsCancelled ? "Cancelado" : "Activo";
        SelectedImageUrl = runningEvent.ImageUrl;
        StatusMessage = "Editando evento seleccionado.";
        NotifyFormState();
    }

    private async Task DeleteEventAsync(RunningEvent? runningEvent)
    {
        if (runningEvent == null)
        {
            return;
        }

        var confirmed = await Shell.Current.DisplayAlert("Eliminar evento", $"Quieres eliminar \"{runningEvent.Name}\"?", "Eliminar", "Cancelar");
        if (!confirmed)
        {
            return;
        }

        var deleted = await _eventService.DeleteEventAsync(runningEvent.Id);
        StatusMessage = deleted ? "Evento eliminado correctamente." : "No se pudo eliminar el evento.";

        if (deleted)
        {
            if (_editingEventId == runningEvent.Id)
            {
                ClearForm();
            }

            await LoadEventsAsync();
        }
    }

    private void ClearForm()
    {
        _editingEventId = null;
        Name = string.Empty;
        Description = string.Empty;
        City = string.Empty;
        Location = string.Empty;
        EventDate = DateTime.Today.AddDays(7);
        PriceText = "0";
        MaxParticipantsText = "100";
        CurrentParticipantsText = "0";
        SelectedStatusText = "Activo";
        SelectedImageUrl = "event_5k_city.svg";
        StatusMessage = string.Empty;
        NotifyFormState();
    }

    private void NotifyFormState()
    {
        OnPropertyChanged(nameof(IsEditing));
        OnPropertyChanged(nameof(FormTitle));
        OnPropertyChanged(nameof(PrimaryActionText));
    }
}
