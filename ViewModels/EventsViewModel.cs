using Latidos.Models;
using Latidos.Services;
using System.Windows.Input;

namespace Latidos.ViewModels;

public class EventsViewModel : BindableObject
{
    private readonly IEventService _eventService;
    private readonly ICartService _cartService;

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

    public ICommand LoadEventsCommand { get; }
    public ICommand AddToCartCommand { get; }
    public ICommand GoToCartCommand { get; }

    public EventsViewModel()
    {
        _eventService = IPlatformApplication.Current?.Services.GetService<IEventService>() ?? new EventService();
        _cartService = IPlatformApplication.Current?.Services.GetService<ICartService>() ?? new CartService();

        LoadEventsCommand = new Command(async () => await LoadEventsAsync());
        AddToCartCommand = new Command<RunningEvent>(async (evt) => await AddToCartAsync(evt));
        GoToCartCommand = new Command(async () => await GoToCartAsync());
    }

    public async Task LoadEventsAsync()
    {
        try
        {
            IsLoading = true;
            Events = await _eventService.GetAllEventsAsync();
            await RefreshCartBadgeAsync();
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert("Error", $"Failed to load events: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task AddToCartAsync(RunningEvent runningEvent)
    {
        try
        {
            var cartItem = new CartItem
            {
                EventId = runningEvent.Id,
                Event = runningEvent,
                Quantity = 1,
                Price = runningEvent.Price
            };

            var added = await _cartService.AddToCartAsync(cartItem);
            if (!added)
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", "Failed to add item to cart", "OK");
                return;
            }

            await RefreshCartBadgeAsync();
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert("Error", $"Failed to add to cart: {ex.Message}", "OK");
        }
    }

    public async Task GoToCartAsync()
    {
        await Shell.Current.GoToAsync("cart");
    }

    private async Task RefreshCartBadgeAsync()
    {
        var cartItems = await _cartService.GetCartItemsAsync();
        CartItemCount = cartItems.Sum(item => item.Quantity);
    }
}
