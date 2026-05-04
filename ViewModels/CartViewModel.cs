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
                OnPropertyChanged(nameof(CartTotalText));
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

    private int _itemCount;
    public int ItemCount
    {
        get => _itemCount;
        set
        {
            if (_itemCount != value)
            {
                _itemCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasItems));
                OnPropertyChanged(nameof(IsEmpty));
                OnPropertyChanged(nameof(ItemCountText));
            }
        }
    }

    public bool HasItems => ItemCount > 0;
    public bool IsEmpty => ItemCount == 0;
    public string ItemCountText => ItemCount == 1 ? "1 inscripcion" : $"{ItemCount} inscripciones";
    public string CartTotalText => CurrencyFormatter.FormatCop(CartTotal);

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
            ItemCount = CartItems.Sum(item => item.Quantity);
            CanCheckout = HasItems;
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert("Error", $"No se pudo cargar el carrito: {ex.Message}", "Aceptar");
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
            await Application.Current!.MainPage!.DisplayAlert("Error", $"No se pudo eliminar el producto: {ex.Message}", "Aceptar");
        }
    }

    public async Task ProceedToCheckoutAsync()
    {
        if (CartItems.Count == 0)
        {
            await Application.Current!.MainPage!.DisplayAlert("Carrito vacio", "Agrega un evento antes de continuar.", "Aceptar");
            return;
        }

        await Shell.Current.GoToAsync("///checkout");
    }

    public async Task ContinueShoppingAsync()
    {
        await Shell.Current.GoToAsync("///events");
    }
}
