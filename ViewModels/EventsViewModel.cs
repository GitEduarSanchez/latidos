using Latidos.Models;
using Latidos.Services;
using System.Windows.Input;

namespace Latidos.ViewModels;

public class EventsViewModel : BindableObject
{
    private readonly IEventService _eventService;
    private readonly ICartService _cartService;
    private List<RunningEvent> _allEvents = new();

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
                OnPropertyChanged(nameof(HasEvents));
                OnPropertyChanged(nameof(NoEventsFound));
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

    private int _cartItemCount;
    public int CartItemCount
    {
        get => _cartItemCount;
        set
        {
            if (_cartItemCount != value)
            {
                _cartItemCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsCartBadgeVisible));
            }
        }
    }

    public bool IsCartBadgeVisible => CartItemCount > 0;

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }
    }

    private string _citySearchText = string.Empty;
    public string CitySearchText
    {
        get => _citySearchText;
        set
        {
            if (_citySearchText != value)
            {
                _citySearchText = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }
    }

    private DateTime _filterDate = DateTime.Today;
    public DateTime FilterDate
    {
        get => _filterDate;
        set
        {
            if (_filterDate != value)
            {
                _filterDate = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }
    }

    private bool _isDateFilterEnabled;
    public bool IsDateFilterEnabled
    {
        get => _isDateFilterEnabled;
        set
        {
            if (_isDateFilterEnabled != value)
            {
                _isDateFilterEnabled = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }
    }

    public bool HasEvents => Events.Count > 0;

    public bool NoEventsFound => !IsLoading && Events.Count == 0;

    public string EventsCountText => Events.Count == 1 ? "1 evento encontrado" : $"{Events.Count} eventos encontrados";

    public ICommand LoadEventsCommand { get; }
    public ICommand AddToCartCommand { get; }
    public ICommand GoToCartCommand { get; }
    public ICommand ClearFiltersCommand { get; }

    public EventsViewModel()
    {
        _eventService = IPlatformApplication.Current?.Services.GetService<IEventService>() ?? new EventService();
        _cartService = IPlatformApplication.Current?.Services.GetService<ICartService>() ?? new CartService();

        LoadEventsCommand = new Command(async () => await LoadEventsAsync());
        AddToCartCommand = new Command<RunningEvent>(async (evt) => await AddToCartAsync(evt));
        GoToCartCommand = new Command(async () => await GoToCartAsync());
        ClearFiltersCommand = new Command(ClearFilters);
    }

    public async Task LoadEventsAsync()
    {
        try
        {
            IsLoading = true;
            _allEvents = await _eventService.GetAllEventsAsync();
            ApplyFilters();
            await RefreshCartBadgeAsync();
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert("Error", $"No se pudieron cargar los eventos: {ex.Message}", "Aceptar");
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(NoEventsFound));
        }
    }

    public async Task AddToCartAsync(RunningEvent runningEvent)
    {
        try
        {
            if (!runningEvent.CanRegister)
            {
                await Application.Current!.MainPage!.DisplayAlert("Evento no disponible", "Este evento no acepta inscripciones.", "Aceptar");
                return;
            }

            await Shell.Current.GoToAsync($"registration?eventId={runningEvent.Id}");
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert("Error", $"No se pudo agregar al carrito: {ex.Message}", "Aceptar");
        }
    }

    public async Task GoToCartAsync()
    {
        await Shell.Current.GoToAsync("///cart");
    }

    private async Task RefreshCartBadgeAsync()
    {
        var cartItems = await _cartService.GetCartItemsAsync();
        CartItemCount = cartItems.Sum(item => item.Quantity);
    }

    private void ApplyFilters()
    {
        var filteredEvents = _allEvents.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filteredEvents = filteredEvents.Where(runningEvent =>
                runningEvent.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(CitySearchText))
        {
            filteredEvents = filteredEvents.Where(runningEvent =>
                runningEvent.City.Contains(CitySearchText, StringComparison.OrdinalIgnoreCase) ||
                runningEvent.Location.Contains(CitySearchText, StringComparison.OrdinalIgnoreCase));
        }

        if (IsDateFilterEnabled)
        {
            filteredEvents = filteredEvents.Where(runningEvent => runningEvent.EventDate.Date == FilterDate.Date);
        }

        Events = filteredEvents
            .OrderBy(runningEvent => runningEvent.StartOrderGroup)
            .ThenBy(runningEvent => runningEvent.EventDate)
            .ToList();
    }

    private void ClearFilters()
    {
        SearchText = string.Empty;
        CitySearchText = string.Empty;
        IsDateFilterEnabled = false;
        FilterDate = DateTime.Today;
        ApplyFilters();
    }
}
