using Latidos.Models;
using Latidos.Services;
using System.Windows.Input;

namespace Latidos.ViewModels;

public class CheckoutViewModel : BindableObject
{
    private readonly IPaymentService _paymentService;
    private readonly ICartService _cartService;
    private readonly IOrderService _orderService;
    private List<CartItem> _cartItems = new();

    private string _customerName = string.Empty;
    public string CustomerName
    {
        get => _customerName;
        set
        {
            if (_customerName != value)
            {
                _customerName = value;
                OnPropertyChanged();
                UpdateCanProcessPayment();
            }
        }
    }

    private string _customerEmail = string.Empty;
    public string CustomerEmail
    {
        get => _customerEmail;
        set
        {
            if (_customerEmail != value)
            {
                _customerEmail = value;
                OnPropertyChanged();
                UpdateCanProcessPayment();
            }
        }
    }

    private string _cardNumber = string.Empty;
    public string CardNumber
    {
        get => _cardNumber;
        set
        {
            if (_cardNumber != value)
            {
                _cardNumber = value;
                OnPropertyChanged();
                UpdateCanProcessPayment();
            }
        }
    }

    private string _cardExpiry = string.Empty;
    public string CardExpiry
    {
        get => _cardExpiry;
        set
        {
            if (_cardExpiry != value)
            {
                _cardExpiry = value;
                OnPropertyChanged();
                UpdateCanProcessPayment();
            }
        }
    }

    private string _cardCvv = string.Empty;
    public string CardCvv
    {
        get => _cardCvv;
        set
        {
            if (_cardCvv != value)
            {
                _cardCvv = value;
                OnPropertyChanged();
                UpdateCanProcessPayment();
            }
        }
    }

    private bool _acceptedTerms;
    public bool AcceptedTerms
    {
        get => _acceptedTerms;
        set
        {
            if (_acceptedTerms != value)
            {
                _acceptedTerms = value;
                OnPropertyChanged();
                UpdateCanProcessPayment();
            }
        }
    }

    private bool _canProcessPayment;
    public bool CanProcessPayment
    {
        get => _canProcessPayment;
        set
        {
            if (_canProcessPayment != value)
            {
                _canProcessPayment = value;
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
            }
        }
    }

    private decimal _subtotal;
    public decimal Subtotal
    {
        get => _subtotal;
        set
        {
            if (_subtotal != value)
            {
                _subtotal = value;
                OnPropertyChanged();
            }
        }
    }

    private decimal _tax;
    public decimal Tax
    {
        get => _tax;
        set
        {
            if (_tax != value)
            {
                _tax = value;
                OnPropertyChanged();
            }
        }
    }

    private decimal _total;
    public decimal Total
    {
        get => _total;
        set
        {
            if (_total != value)
            {
                _total = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand ProcessPaymentCommand { get; }
    public ICommand CancelCommand { get; }

    public CheckoutViewModel()
    {
        _paymentService = IPlatformApplication.Current?.Services.GetService<IPaymentService>() ?? throw new InvalidOperationException("Payment service not registered");
        _cartService = IPlatformApplication.Current?.Services.GetService<ICartService>() ?? new CartService();
        _orderService = IPlatformApplication.Current?.Services.GetService<IOrderService>() ?? new OrderService();

        ProcessPaymentCommand = new Command(async () => await ProcessPaymentAsync());
        CancelCommand = new Command(async () => await CancelAsync());

        LoadCheckoutDataAsync();
    }

    private async void LoadCheckoutDataAsync()
    {
        try
        {
            _cartItems = await _cartService.GetCartItemsAsync();
            ItemCount = _cartItems.Sum(ci => ci.Quantity);
            Subtotal = _cartItems.Sum(ci => ci.TotalPrice);
            Tax = Subtotal * 0.08m;
            Total = Subtotal + Tax;
            UpdateCanProcessPayment();
        }
        catch (Exception ex)
        {
            StatusMessage = $"No se pudo cargar el carrito: {ex.Message}";
        }
    }

    private void UpdateCanProcessPayment()
    {
        CanProcessPayment = !string.IsNullOrWhiteSpace(CustomerName) &&
                           !string.IsNullOrWhiteSpace(CustomerEmail) &&
                           !string.IsNullOrWhiteSpace(CardNumber) &&
                           !string.IsNullOrWhiteSpace(CardExpiry) &&
                           !string.IsNullOrWhiteSpace(CardCvv) &&
                           AcceptedTerms &&
                           Total > 0;
    }

    public async Task ProcessPaymentAsync()
    {
        try
        {
            StatusMessage = "Procesando pago...";

            var paymentRequest = new PaymentRequest
            {
                TokenId = CardNumber,
                Amount = Total,
                Currency = "USD",
                Description = $"Inscripciones a eventos - {ItemCount} competidores",
                CustomerEmail = CustomerEmail,
                CustomerName = CustomerName
            };

            var paymentResponse = await _paymentService.ProcessPaymentAsync(paymentRequest);

            if (paymentResponse.Success)
            {
                var order = new Order
                {
                    CustomerName = CustomerName,
                    CustomerEmail = CustomerEmail,
                    TotalAmount = Total,
                    Status = "Completado",
                    TransactionId = paymentResponse.TransactionId,
                    Items = _cartItems.Select(ci => new OrderItem
                    {
                        EventId = ci.EventId,
                        EventName = ci.Event?.Name ?? "Evento sin nombre",
                        CompetitorNumber = ci.Competitor?.CompetitorNumber ?? string.Empty,
                        CompetitorName = ci.Competitor?.FullName ?? string.Empty,
                        CompetitorDocument = ci.Competitor == null ? string.Empty : $"{ci.Competitor.DocumentType}: {ci.Competitor.DocumentNumber}",
                        CompetitorAgeCategory = ci.Competitor?.AgeCategory ?? string.Empty,
                        Quantity = ci.Quantity,
                        UnitPrice = ci.Price,
                        TotalPrice = ci.TotalPrice
                    }).ToList()
                };

                await _orderService.CreateOrderAsync(order);
                await _cartService.ClearCartAsync();

                StatusMessage = $"Pago exitoso. Orden: {paymentResponse.TransactionId}";
                await Application.Current!.MainPage!.DisplayAlert("Pago exitoso", "El pago fue procesado correctamente.", "Aceptar");
                await Shell.Current.GoToAsync("///events");
            }
            else
            {
                StatusMessage = $"Pago rechazado: {paymentResponse.Message}";
                await Application.Current!.MainPage!.DisplayAlert("Error", $"Pago rechazado: {paymentResponse.Message}", "Aceptar");
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al procesar el pago: {ex.Message}";
            await Application.Current!.MainPage!.DisplayAlert("Error", $"Error: {ex.Message}", "Aceptar");
        }
    }

    public async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("///cart");
    }
}
