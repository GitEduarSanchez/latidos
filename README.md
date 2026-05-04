# Latidos - Running Events Registration App

Una aplicación .NET MAUI para la compra y gestión de inscripciones en eventos de running con integración de pasarela de pago Stripe.

## Características

✅ **Catálogo de Eventos** - Visualiza todos los eventos de running disponibles
✅ **Carrito de Compras** - Agrega y gestiona inscripciones
✅ **Checkout** - Proceso seguro de compra
✅ **Integración Stripe** - Procesamiento de pagos profesional
✅ **Gestión de Órdenes** - Historial de compras
✅ **MVVM Architecture** - Código limpio y mantenible

## Estructura del Proyecto

```
Latidos/
├── Models/                    # Modelos de datos
│   ├── RunningEvent.cs
│   ├── CartItem.cs
│   ├── PaymentRequest.cs
│   ├── PaymentResponse.cs
│   └── Order.cs
├── Services/                  # Servicios de negocio
│   ├── IEventService.cs
│   ├── EventService.cs
│   ├── ICartService.cs
│   ├── CartService.cs
│   ├── IPaymentService.cs
│   ├── StripePaymentService.cs
│   ├── IOrderService.cs
│   └── OrderService.cs
├── Views/                     # Páginas XAML
│   ├── EventsPage.xaml        # Catálogo de eventos
│   ├── CartPage.xaml          # Carrito de compras
│   └── CheckoutPage.xaml      # Página de pago
├── ViewModels/                # ViewModels MVVM
│   ├── EventsViewModel.cs
│   ├── CartViewModel.cs
│   └── CheckoutViewModel.cs
├── MauiProgram.cs             # Configuración de la app
├── AppShell.xaml              # Navegación
└── appsettings.json           # Variables de configuración
```

## Configuración Inicial

### 1. Requisitos
- Visual Studio 2024 o superior
- .NET 10 SDK
- Cuenta de Stripe (https://stripe.com)

### 2. Instalación de Paquetes NuGet

```bash
dotnet add package CommunityToolkit.Mvvm
dotnet add package Stripe.net
```

### 3. Configuración de Stripe

1. Obtén tus claves de Stripe:
   - Ir a https://dashboard.stripe.com/apikeys
   - Copiar tu **Secret Key** y **Publishable Key**

2. Actualiza `appsettings.json`:
   ```json
   {
     "Stripe": {
       "SecretKey": "sk_test_YOUR_KEY_HERE",
       "PublishableKey": "pk_test_YOUR_KEY_HERE"
     }
   }
   ```

### 4. Leer la configuración en la App

Actualiza `MauiProgram.cs` para cargar la configuración:

```csharp
var configBuilder = new ConfigurationBuilder();
var assembly = typeof(MauiProgram).Assembly;
using (var stream = assembly.GetManifestResourceStream("Latidos.appsettings.json"))
{
    if (stream != null)
    {
        configBuilder.AddJsonStream(stream);
    }
}

var configuration = configBuilder.Build();
builder.Services.AddSingleton(configuration);
```

## Flujo de Aplicación

### 1. **Página de Eventos**
- Muestra lista de eventos disponibles
- Permite agregar inscripciones al carrito
- Botón para ir al carrito

### 2. **Carrito**
- Visualiza artículos agregados
- Permite modificar cantidades
- Muestra total
- Botón para proceder al checkout

### 3. **Checkout**
- Formulario de información del cliente
- Datos de tarjeta de crédito
- Aceptación de términos y condiciones
- Procesamiento del pago con Stripe

### 4. **Confirmación**
- Número de transacción
- Confirmación por email
- Redirección a eventos

## Integración de Stripe

### Flujo de Pago Actual (Sandbox)

En producción, debes usar **Stripe.js** para generar tokens seguros:

```javascript
// Cliente (JavaScript)
const stripe = Stripe('pk_test_YOUR_KEY');
const result = await stripe.createToken(cardElement);
const token = result.token.id; // Enviar este token al backend
```

Luego usar el token en `StripePaymentService.cs`.

### Configuración Recomendada para Producción

1. **Habilitar HTTPS**
2. **Usar Stripe.js para tokenización**
3. **Validación de datos en cliente y servidor**
4. **Manejo de errores mejorado**
5. **Webhooks de Stripe para notificaciones**

## Modelos de Datos

### RunningEvent
```csharp
public class RunningEvent
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime EventDate { get; set; }
    public decimal Price { get; set; }
    public string Location { get; set; }
    public int MaxParticipants { get; set; }
    public int CurrentParticipants { get; set; }
}
```

### Order
```csharp
public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } // Pending, Processing, Completed, Failed
    public string CustomerName { get; set; }
    public string CustomerEmail { get; set; }
    public string TransactionId { get; set; }
    public List<OrderItem> Items { get; set; }
}
```

## Servicios Disponibles

### IEventService
- `GetAllEventsAsync()` - Obtiene todos los eventos
- `GetEventByIdAsync(int eventId)` - Obtiene evento por ID
- `AddEventAsync(RunningEvent)` - Agrega nuevo evento
- `UpdateEventAsync(RunningEvent)` - Actualiza evento

### ICartService
- `GetCartItemsAsync()` - Obtiene items del carrito
- `AddToCartAsync(CartItem)` - Agrega item al carrito
- `RemoveFromCartAsync(int itemId)` - Elimina item del carrito
- `UpdateQuantityAsync(int itemId, int quantity)` - Actualiza cantidad
- `ClearCartAsync()` - Vacía el carrito
- `GetCartTotalAsync()` - Obtiene total del carrito

### IPaymentService
- `ProcessPaymentAsync(PaymentRequest)` - Procesa pago
- `RefundAsync(string transactionId, decimal amount)` - Reembolsa pago

### IOrderService
- `CreateOrderAsync(Order)` - Crea nueva orden
- `GetOrderByIdAsync(int orderId)` - Obtiene orden por ID
- `GetUserOrdersAsync(string email)` - Obtiene órdenes del usuario
- `UpdateOrderStatusAsync(int orderId, string status)` - Actualiza estado

## Próximas Mejoras

- [ ] Base de datos SQLite local
- [ ] Autenticación de usuarios
- [ ] Historial de órdenes persistente
- [ ] Notificaciones por email
- [ ] Webhook de Stripe para confirmaciones en tiempo real
- [ ] Soporte para múltiples métodos de pago
- [ ] Validación avanzada de tarjetas
- [ ] Interfaz administrativo para gestionar eventos

## Pruebas con Stripe (Sandbox)

Tarjetas de prueba:
- Exitosa: `4242 4242 4242 4242`
- Declinada: `4000 0000 0000 0002`
- Expiración: Cualquier fecha futura
- CVV: Cualquier número

## Licencia

Este proyecto está disponible bajo licencia MIT.

## Soporte

Para reportar problemas o sugerencias, contacta al equipo de desarrollo.
