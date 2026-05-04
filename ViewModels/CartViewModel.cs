using Latidos.Models;
using Latidos.Services;
using System.Windows.Input;

namespace Latidos.ViewModels;

public class CartViewModel : BindableObject
{
    private readonly ICartService _cartService;

    private List<CartItem> _cartItems = new();
    public List<CartItem> CartItems
    {
        get => _cartItems;
        set
        {
            if (_cartItems != value)
            {
                _cartItems = value;
                OnPropertyChanged();
            }
        }
    }

    private decimal _cartTotal;
    public decimal CartTotal
    {
        get => _cartTotal;
        set
        {
            if (_cartTotal != value)
            {
                _cartTotal = value;
                OnPropertyChanged();
            }
        }
    }

    private bool _canCheckout;
    public bool CanCheckout
    {
        get => _canCheckout;
        set
        {
            if (_canCheckout != value)
            {
                _canCheckout = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand LoadCartCommand { get; }
    public ICommand RemoveFromCartCommand { get; }
    public ICommand ProceedToCheckoutCommand { get; }
    public ICommand ContinueShoppingCommand { get; }

    public CartViewModel()
    {
        _cartService = IPlatformApplication.Current?.Services.GetService<ICartService>() ?? new CartService();

        LoadCartCommand = new Command(async () => await LoadCartAsync());
        RemoveFromCartCommand = new Command<CartItem>(async (item) => await RemoveFromCartAsync(item));
        ProceedToCheckoutCommand = new Command(async () => await ProceedToCheckoutAsync());
        ContinueShoppingCommand = new Command(async () => await ContinueShoppingAsync());
    }

    public async Task LoadCartAsync()
    {
        try
        {
            CartItems = await _cartService.GetCartItemsAsync();
            CartTotal = await _cartService.GetCartTotalAsync();
            CanCheckout = CartItems.Count > 0;
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert("Error", $"Failed to load cart: {ex.Message}", "OK");
        }
    }

    public async Task RemoveFromCartAsync(CartItem item)
    {
        try
        {
            await _cartService.RemoveFromCartAsync(item.Id);
            await LoadCartAsync();
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert("Error", $"Failed to remove item: {ex.Message}", "OK");
        }
    }

    public async Task ProceedToCheckoutAsync()
    {
        if (CartItems.Count == 0)
        {
            await Application.Current!.MainPage!.DisplayAlert("Warning", "Your cart is empty", "OK");
            return;
        }

        await Shell.Current.GoToAsync("checkout");
    }

    public async Task ContinueShoppingAsync()
    {
        await Shell.Current.GoToAsync("events");
    }
}
