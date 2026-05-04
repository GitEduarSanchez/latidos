using Latidos.Models;
using Latidos.Services;
using System.Windows.Input;

namespace Latidos.ViewModels;

public class CartViewModel : BindableObject
{
    private readonly ICartService _cartService;
    private readonly ICompetitorService _competitorService;

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

    private List<CartEventGroup> _groupedCartItems = new();
    public List<CartEventGroup> GroupedCartItems
    {
        get => _groupedCartItems;
        set
        {
            if (_groupedCartItems != value)
            {
                _groupedCartItems = value;
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
    public ICommand ChangeCompetitorCommand { get; }
    public ICommand ProceedToCheckoutCommand { get; }
    public ICommand ContinueShoppingCommand { get; }

    public CartViewModel()
    {
        _cartService = IPlatformApplication.Current?.Services.GetService<ICartService>() ?? new CartService();
        _competitorService = IPlatformApplication.Current?.Services.GetService<ICompetitorService>() ?? new CompetitorService();

        LoadCartCommand = new Command(async () => await LoadCartAsync());
        RemoveFromCartCommand = new Command<CartItem>(async (item) => await RemoveFromCartAsync(item));
        ChangeCompetitorCommand = new Command<CartItem>(async (item) => await ChangeCompetitorAsync(item));
        ProceedToCheckoutCommand = new Command(async () => await ProceedToCheckoutAsync());
        ContinueShoppingCommand = new Command(async () => await ContinueShoppingAsync());
    }

    public async Task LoadCartAsync()
    {
        try
        {
            CartItems = await _cartService.GetCartItemsAsync();
            GroupedCartItems = CartItems
                .GroupBy(ci => ci.EventId)
                .Select(g => new CartEventGroup
                {
                    EventId = g.Key,
                    Event = g.First().Event,
                    Items = g.ToList()
                })
                .OrderBy(g => g.EventDate)
                .ToList();
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

    public async Task ChangeCompetitorAsync(CartItem item)
    {
        try
        {
            var competitors = await _competitorService.GetAllCompetitorsAsync();
            if (competitors.Count == 0)
            {
                await Application.Current!.MainPage!.DisplayAlert("Sin competidores", "No hay competidores registrados para seleccionar.", "Aceptar");
                return;
            }

            var options = competitors
                .Select(c => $"{c.FullName} - {c.DocumentType}: {c.DocumentNumber}")
                .ToArray();

            var selected = await Application.Current!.MainPage!.DisplayActionSheet(
                "Cambiar participante",
                "Cancelar",
                null,
                options);

            if (string.IsNullOrWhiteSpace(selected) || selected == "Cancelar")
            {
                return;
            }

            var index = Array.IndexOf(options, selected);
            if (index < 0) return;

            var chosen = competitors[index];
            var updated = new CompetitorProfile
            {
                CompetitorNumber = chosen.CompetitorNumber,
                FullName = chosen.FullName,
                DocumentType = chosen.DocumentType,
                DocumentNumber = chosen.DocumentNumber,
                BirthDate = chosen.BirthDate,
                PhotoPath = chosen.PhotoPath
            };

            await _cartService.UpdateCompetitorAsync(item.Id, updated);
            await LoadCartAsync();
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert("Error", $"No se pudo cambiar el participante: {ex.Message}", "Aceptar");
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
