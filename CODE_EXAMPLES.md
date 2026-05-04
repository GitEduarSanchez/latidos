# Ejemplos de Código - Latidos App

## Ejemplos de Uso de los Servicios

### 1. Obtener y Mostrar Eventos

```csharp
// En EventsViewModel.cs
public async Task LoadEventsAsync()
{
    try
    {
        IsLoading = true;

        // Obtener todos los eventos
        Events = await _eventService.GetAllEventsAsync();

        // Filtrar solo eventos disponibles (con espacios)
        var availableEvents = Events
            .Where(e => e.CurrentParticipants < e.MaxParticipants)
            .ToList();

        Events = availableEvents;
    }
    catch (Exception ex)
    {
        StatusMessage = $"Error: {ex.Message}";
    }
    finally
    {
        IsLoading = false;
    }
}
```

### 2. Agregar Artículos al Carrito

```csharp
// En EventsViewModel.cs
public async Task AddToCartAsync(RunningEvent runningEvent)
{
    if (runningEvent.CurrentParticipants >= runningEvent.MaxParticipants)
    {
        await Application.Current!.MainPage!.DisplayAlert(
            "Sold Out", 
            "This event is full", 
            "OK"
        );
        return;
    }

    var cartItem = new CartItem
    {
        EventId = runningEvent.Id,
        Event = runningEvent,
        Quantity = 1,
        Price = runningEvent.Price
    };

    await _cartService.AddToCartAsync(cartItem);

    await Application.Current!.MainPage!.DisplayAlert(
        "Success", 
        $"✓ Added {runningEvent.Name} to cart", 
        "OK"
    );
}
```

### 3. Manejar el Carrito de Compras

```csharp
// En CartViewModel.cs
public async Task UpdateQuantityAsync(int cartItemId, int newQuantity)
{
    if (newQuantity <= 0)
    {
        await RemoveFromCartAsync(cartItemId);
        return;
    }

    try
    {
        await _cartService.UpdateQuantityAsync(cartItemId, newQuantity);
        await LoadCartAsync();
    }
    catch (Exception ex)
    {
        await Application.Current!.MainPage!.DisplayAlert(
            "Error", 
            $"Failed to update: {ex.Message}", 
            "OK"
        );
    }
}

// Vaciar carrito después de compra exitosa
public async Task ClearCartAndResetAsync()
{
    await _cartService.ClearCartAsync();
    CartItems = new List<CartItem>();
    CartTotal = 0;
    CanCheckout = false;
}
```

### 4. Procesar Pago con Stripe

```csharp
// En CheckoutViewModel.cs
public async Task ProcessPaymentAsync()
{
    // Validar información
    if (!ValidatePaymentInfo())
    {
        StatusMessage = "Please fill in all required fields";
        return;
    }

    try
    {
        StatusMessage = "🔄 Processing payment...";

        // Crear solicitud de pago
        var paymentRequest = new PaymentRequest
        {
            TokenId = CardNumber,  // En producción: usar token de Stripe.js
            Amount = Total,
            Currency = "USD",
            Description = $"Running Events x{ItemCount}",
            CustomerEmail = CustomerEmail,
            CustomerName = CustomerName
        };

        // Procesar pago
        var response = await _paymentService.ProcessPaymentAsync(paymentRequest);

        if (response.Success)
        {
            await HandleSuccessfulPaymentAsync(response);
        }
        else
        {
            await HandleFailedPaymentAsync(response);
        }
    }
    catch (Exception ex)
    {
        StatusMessage = $"❌ Error: {ex.Message}";
    }
}

private async Task HandleSuccessfulPaymentAsync(PaymentResponse response)
{
    try
    {
        // Crear orden en la base de datos
        var order = new Order
        {
            CustomerName = CustomerName,
            CustomerEmail = CustomerEmail,
            TotalAmount = Total,
            Status = "Completed",
            TransactionId = response.TransactionId,
            Items = _cartItems.Select(ci => new OrderItem
            {
                EventId = ci.EventId,
                EventName = ci.Event?.Name ?? "Unknown",
                Quantity = ci.Quantity,
                UnitPrice = ci.Price,
                TotalPrice = ci.TotalPrice
            }).ToList()
        };

        await _orderService.CreateOrderAsync(order);

        // Limpiar carrito
        await _cartService.ClearCartAsync();

        StatusMessage = $"✓ Payment successful! Order: {response.TransactionId}";

        await Application.Current!.MainPage!.DisplayAlert(
            "Success",
            $"Thank you for your purchase!\n\nOrder: {order.OrderNumber}",
            "OK"
        );

        // Volver a eventos
        await Shell.Current.GoToAsync("events");
    }
    catch (Exception ex)
    {
        StatusMessage = $"Order creation failed: {ex.Message}";
    }
}

private async Task HandleFailedPaymentAsync(PaymentResponse response)
{
    StatusMessage = $"❌ Payment declined: {response.Message}";

    await Application.Current!.MainPage!.DisplayAlert(
        "Payment Failed",
        response.Message,
        "Try Again"
    );
}

private bool ValidatePaymentInfo()
{
    return !string.IsNullOrWhiteSpace(CustomerName) &&
           !string.IsNullOrWhiteSpace(CustomerEmail) &&
           IsValidEmail(CustomerEmail) &&
           IsValidCardNumber(CardNumber) &&
           !string.IsNullOrWhiteSpace(CardExpiry) &&
           !string.IsNullOrWhiteSpace(CardCvv) &&
           AcceptedTerms;
}

private bool IsValidEmail(string email)
{
    try
    {
        var addr = new System.Net.Mail.MailAddress(email);
        return addr.Address == email;
    }
    catch
    {
        return false;
    }
}

private bool IsValidCardNumber(string cardNumber)
{
    return cardNumber.Length >= 13 && cardNumber.Length <= 19 
        && cardNumber.All(char.IsDigit);
}
```

### 5. Manejar Errores y Reintentosde Pago

```csharp
// Servicio con reintentos
public class RobustPaymentService
{
    private readonly IPaymentService _paymentService;
    private const int MaxRetries = 3;
    private const int DelayMilliseconds = 1000;

    public async Task<PaymentResponse> ProcessPaymentWithRetriesAsync(
        PaymentRequest request)
    {
        int attempts = 0;

        while (attempts < MaxRetries)
        {
            try
            {
                var response = await _paymentService.ProcessPaymentAsync(request);
                return response;
            }
            catch (HttpRequestException) when (attempts < MaxRetries - 1)
            {
                attempts++;
                await Task.Delay(DelayMilliseconds * attempts);
            }
            catch (Exception ex)
            {
                return new PaymentResponse
                {
                    Success = false,
                    Message = $"Payment error: {ex.Message}",
                    Amount = request.Amount
                };
            }
        }

        return new PaymentResponse
        {
            Success = false,
            Message = "Payment service unavailable. Please try again.",
            Amount = request.Amount
        };
    }
}
```

### 6. Obtener Historial de Órdenes del Usuario

```csharp
// En un futuro ViewModel de historial
public class OrderHistoryViewModel : BindableObject
{
    private readonly IOrderService _orderService;

    private List<Order> _orders = new();
    public List<Order> Orders
    {
        get => _orders;
        set
        {
            if (_orders != value)
            {
                _orders = value;
                OnPropertyChanged();
            }
        }
    }

    public async Task LoadUserOrdersAsync(string customerEmail)
    {
        try
        {
            Orders = await _orderService.GetUserOrdersAsync(customerEmail);
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Error", 
                $"Failed to load orders: {ex.Message}", 
                "OK"
            );
        }
    }

    // Calcular estadísticas
    public decimal GetTotalSpent()
    {
        return Orders.Sum(o => o.TotalAmount);
    }

    public int GetTotalEvents()
    {
        return Orders.Sum(o => o.Items.Sum(i => i.Quantity));
    }

    public List<Order> GetSuccessfulOrders()
    {
        return Orders.Where(o => o.Status == "Completed").ToList();
    }
}
```

### 7. Crear un Servicio de Descuentos

```csharp
public interface IDiscountService
{
    Task<decimal> CalculateDiscountAsync(List<CartItem> items);
}

public class DiscountService : IDiscountService
{
    public async Task<decimal> CalculateDiscountAsync(List<CartItem> items)
    {
        // Simular procesamiento asincrónico
        await Task.Delay(100);

        var subtotal = items.Sum(ci => ci.TotalPrice);
        decimal discount = 0;

        // Lógica de descuentos
        if (items.Count >= 3)
            discount = subtotal * 0.10m; // 10% por 3+ eventos
        else if (items.Count >= 2)
            discount = subtotal * 0.05m; // 5% por 2+ eventos
        else if (subtotal > 100)
            discount = subtotal * 0.05m; // 5% por compra > $100

        return discount;
    }
}

// Usar en CheckoutViewModel
private async Task CalculateDiscountedTotalAsync()
{
    var discount = await _discountService.CalculateDiscountAsync(_cartItems);

    Subtotal = _cartItems.Sum(ci => ci.TotalPrice);
    Discount = discount;
    Tax = (Subtotal - Discount) * 0.08m;
    Total = Subtotal - Discount + Tax;
}
```

### 8. Logging y Monitoreo

```csharp
public class PaymentLogger
{
    private readonly string _logFilePath = 
        Path.Combine(FileSystem.AppDataDirectory, "payment_logs.txt");

    public async Task LogPaymentAsync(PaymentRequest request, PaymentResponse response)
    {
        var logEntry = $@"
[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]
Amount: ${request.Amount}
Customer: {request.CustomerName} ({request.CustomerEmail})
Status: {(response.Success ? "SUCCESS" : "FAILED")}
Transaction ID: {response.TransactionId}
Message: {response.Message}
---
";

        try
        {
            await File.AppendAllTextAsync(_logFilePath, logEntry);
        }
        catch
        {
            // Logging fallback
            Debug.WriteLine(logEntry);
        }
    }

    public async Task<string> GetLogsAsync()
    {
        try
        {
            return await File.ReadAllTextAsync(_logFilePath);
        }
        catch
        {
            return "No logs available";
        }
    }
}
```

### 9. Validaciones Avanzadas

```csharp
public static class PaymentValidator
{
    // Validar número de tarjeta (Luhn Algorithm)
    public static bool IsValidCardNumber(string cardNumber)
    {
        cardNumber = cardNumber.Replace(" ", "").Replace("-", "");

        if (!cardNumber.All(char.IsDigit))
            return false;

        int sum = 0;
        bool isSecondDigit = false;

        for (int i = cardNumber.Length - 1; i >= 0; i--)
        {
            int digit = cardNumber[i] - '0';

            if (isSecondDigit)
            {
                digit *= 2;
                if (digit > 9)
                    digit -= 9;
            }

            sum += digit;
            isSecondDigit = !isSecondDigit;
        }

        return sum % 10 == 0;
    }

    // Validar expiración de tarjeta
    public static bool IsCardExpired(string expiry)
    {
        if (!expiry.Contains("/"))
            return true;

        var parts = expiry.Split('/');
        if (parts.Length != 2)
            return true;

        if (!int.TryParse(parts[0], out int month) ||
            !int.TryParse(parts[1], out int year))
            return true;

        var now = DateTime.Now;
        var expireDate = new DateTime(2000 + year, month, 1).AddMonths(1).AddDays(-1);

        return now > expireDate;
    }

    // Validar CVV
    public static bool IsValidCVV(string cvv)
    {
        return cvv.Length >= 3 && cvv.Length <= 4 && cvv.All(char.IsDigit);
    }
}

// Uso en CheckoutViewModel
private bool ValidateAllPaymentInfo()
{
    return PaymentValidator.IsValidCardNumber(CardNumber) &&
           !PaymentValidator.IsCardExpired(CardExpiry) &&
           PaymentValidator.IsValidCVV(CardCvv) &&
           IsValidEmail(CustomerEmail);
}
```

### 10. Manejo de Conexión a Internet

```csharp
public class ConnectivityHelper
{
    public static bool IsConnected()
    {
        var current = Connectivity.Current;
        return current.NetworkAccess == NetworkAccess.Internet;
    }

    public static async Task<bool> CheckConnectivityAsync()
    {
        if (!IsConnected())
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "No Connection",
                "Please check your internet connection",
                "OK"
            );
            return false;
        }
        return true;
    }
}

// Usar antes de procesar pago
public async Task ProcessPaymentAsync()
{
    if (!await ConnectivityHelper.CheckConnectivityAsync())
        return;

    // Procesar pago...
}
```

---

## Patrones de Diseño Usados

### MVVM (Model-View-ViewModel)
- Separación de responsabilidades
- Binding de datos automático
- Fácil de testear

### Dependency Injection
- Inyección en MauiProgram.cs
- Servicios registrados con interfaces
- Facilita testing y cambios futuros

### Repository Pattern
- Services actúan como repositorios
- Abstracción del acceso a datos
- Fácil migración a BD real

---

## Tips de Performance

1. **Async/Await**: Siempre usa operaciones asincrónicas
2. **Caching**: Guarda eventos frecuentes en memoria
3. **Lazy Loading**: Carga datos bajo demanda
4. **Paginación**: No cargues todos los eventos de una vez
5. **Threading**: Usa Task para no bloquear UI

---

¡Espero que estos ejemplos te ayuden a entender y extender la aplicación! 🚀
