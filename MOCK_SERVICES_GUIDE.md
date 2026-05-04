# 🎭 Servicios Mock - Guía de Desarrollo

## ¿Qué son los Servicios Mock?

Son **simuladores** de servicios reales que permiten prototipar sin dependencias externas:

- ✅ No requieren Stripe API
- ✅ Respuestas instantáneas/controladas
- ✅ Ideal para desarrollo local
- ✅ Testing sin costo
- ✅ Emula comportamientos reales

---

## 🚀 Usar Servicios Mock

### Activar Mock (Predeterminado)

En `MauiProgram.cs`:

```csharp
var config = new ServiceConfiguration(useMock: true);  // ✓ Mock activo
builder.Services.AddSingleton<IServiceConfiguration>(config);
```

### Cambiar a Servicios Reales

Cuando estés listo para Stripe:

```csharp
var config = new ServiceConfiguration(useMock: false);  // ✗ Usa real
```

---

## 📋 Servicios Mock Disponibles

### 1. MockEventService

Retorna 6 eventos de prueba con datos realistas:

```csharp
public class MockEventService : IEventService
{
    // Eventos incluidos:
    // 1. 5K City Morning Run - $25 - 245/500 participantes
    // 2. Half Marathon - $45 - 156/300 participantes
    // 3. Trail Running 10K - $35 - 87/200 participantes
    // 4. Women's 10K Charity - $30 - 320/400 participantes
    // 5. Sprint Series - $20 - 145/150 participantes
    // 6. Night Glow Run - $35 - 412/600 participantes
}
```

#### Métodos:

```csharp
// Obtener todos
Task<List<RunningEvent>> GetAllEventsAsync()

// Obtener por ID
Task<RunningEvent?> GetEventByIdAsync(int eventId)

// Agregar nuevo
Task<bool> AddEventAsync(RunningEvent @event)

// Actualizar
Task<bool> UpdateEventAsync(RunningEvent @event)
```

#### Ejemplo de Uso:

```csharp
var eventService = new MockEventService();
var eventos = await eventService.GetAllEventsAsync();

foreach (var evt in eventos)
{
    Console.WriteLine($"{evt.Name} - ${evt.Price}");
}
```

---

### 2. MockPaymentService

Simula Stripe con diferentes respuestas según tarjeta:

```csharp
public class MockPaymentService : IPaymentService
{
    // Tarjetas de prueba incluidas:
    // ✓ 4242424242424242 → EXITOSA
    // ✗ 4000000000000002 → DECLINADA
    // ✗ 4000000000000069 → EXPIRADA
    // ✗ 4000002500003155 → REQUIERE AUTENTICACIÓN
    // ✓ Cualquier otra válida → EXITOSA
}
```

#### Métodos:

```csharp
// Procesar pago
Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)

// Reembolsar
Task<bool> RefundAsync(string transactionId, decimal amount)
```

#### Ejemplo de Uso:

```csharp
var paymentService = new MockPaymentService();

var request = new PaymentRequest
{
    TokenId = "4242424242424242",
    Amount = 99.99m,
    CustomerName = "John Doe",
    CustomerEmail = "john@example.com"
};

var response = await paymentService.ProcessPaymentAsync(request);
if (response.Success)
{
    Console.WriteLine($"Pago exitoso: {response.TransactionId}");
}
```

---

## 🎯 Tarjetas de Prueba Mock

Usa estas tarjetas en la aplicación:

### ✅ Exitosa
```
Número:      4242 4242 4242 4242
Expiración:  12/25
CVV:         123
Resultado:   ✓ Pago aprobado
```

### ✗ Declinada
```
Número:      4000 0000 0000 0002
Expiración:  12/25
CVV:         123
Resultado:   ✗ Tarjeta rechazada
```

### ✗ Expirada
```
Número:      4000 0000 0000 0069
Expiración:  12/25
CVV:         123
Resultado:   ✗ Tarjeta expirada
```

### ✗ Requiere Autenticación
```
Número:      4000 0025 0000 3155
Expiración:  12/25
CVV:         123
Resultado:   ✗ Requiere 3D Secure
```

---

## 🔄 Flujo Completo con Mock

### 1. Abrir App
```
→ MauiProgram.cs con useMock: true
→ MockEventService registrado
→ MockPaymentService registrado
```

### 2. Ver Eventos
```
→ EventsPage carga
→ EventsViewModel.LoadEventsAsync()
→ MockEventService.GetAllEventsAsync()
→ 6 eventos retornados
→ UI renderiza eventos
```

### 3. Agregar al Carrito
```
→ Usuario hace clic "Add to Cart"
→ CartItem se agrega a CartService
→ Confirmación mostrada
```

### 4. Ir a Checkout
```
→ Usuario completa formulario
→ Selecciona tarjeta: 4242...
→ Hace clic "Process Payment"
```

### 5. Procesar Pago Mock
```
→ CheckoutViewModel.ProcessPaymentAsync()
→ MockPaymentService.ProcessPaymentAsync()
→ Simula latencia (500-2000ms)
→ Reconoce tarjeta 4242...
→ Retorna: Success = true
→ TransactionId generado
```

### 6. Crear Orden
```
→ OrderService.CreateOrderAsync()
→ Orden guardada en memoria
→ Carrito vaciado
→ Confirmación mostrada
```

### 7. Volver a Eventos
```
→ Usuario navega de vuelta
→ Flujo listo para repetir
```

---

## 📊 Datos Mock Incluidos

### Eventos
```
ID  | Nombre                    | Precio | Participantes | Ubicación
----|---------------------------|--------|---------------|------------------
1   | 5K City Morning Run       | $25    | 245/500       | Central Park
2   | Half Marathon Championship| $45    | 156/300       | Downtown
3   | Trail Running 10K         | $35    | 87/200        | Mountain Trail
4   | Women's 10K Charity       | $30    | 320/400       | Riverside
5   | Sprint Series - Week 1    | $20    | 145/150       | Athletic Complex
6   | Night Glow Run            | $35    | 412/600       | Downtown Loop
```

### Transacciones
```
Tarjeta                 | Resultado   | Latencia
------------------------|-------------|----------
4242 4242 4242 4242     | ✓ Exitosa   | 500-2000ms
4000 0000 0000 0002     | ✗ Declinada | 500-2000ms
4000 0000 0000 0069     | ✗ Expirada  | 500-2000ms
4000 0025 0000 3155     | ✗ Auth req. | 500-2000ms
Otra válida             | ✓ Exitosa   | 500-2000ms
```

---

## 🧪 Testing con Mock

### Test 1: Compra Exitosa

```csharp
[Test]
public async Task TestSuccessfulPurchase()
{
    var paymentService = new MockPaymentService();
    var request = new PaymentRequest
    {
        TokenId = "4242424242424242",
        Amount = 99.99m,
        CustomerName = "Test User",
        CustomerEmail = "test@example.com"
    };

    var response = await paymentService.ProcessPaymentAsync(request);

    Assert.IsTrue(response.Success);
    Assert.IsNotEmpty(response.TransactionId);
}
```

### Test 2: Compra Rechazada

```csharp
[Test]
public async Task TestDeclinedCard()
{
    var paymentService = new MockPaymentService();
    var request = new PaymentRequest
    {
        TokenId = "4000000000000002",
        Amount = 50m,
        CustomerName = "Test User",
        CustomerEmail = "test@example.com"
    };

    var response = await paymentService.ProcessPaymentAsync(request);

    Assert.IsFalse(response.Success);
    Assert.IsTrue(response.Message.Contains("declined"));
}
```

### Test 3: Cargar Eventos

```csharp
[Test]
public async Task TestLoadEvents()
{
    var eventService = new MockEventService();
    var events = await eventService.GetAllEventsAsync();

    Assert.AreEqual(6, events.Count);
    Assert.IsTrue(events.All(e => e.Price > 0));
}
```

---

## 🔧 Características del Mock

### Latencia Simulada
```csharp
// MockPaymentService simula 500-2000ms
await Task.Delay(_random.Next(500, 2000));
```

### IDs de Transacción Únicos
```csharp
// Generados como: ch_mock_1000_ABC123DE
response.TransactionId = $"ch_mock_{_transactionCounter++}_{Guid.NewGuid().ToString().Substring(0, 8)}";
```

### Validación de Tarjeta
```csharp
// Reconoce patrones de Stripe
if (cardNumber == "4242424242424242")  // Exitosa
if (cardNumber == "4000000000000002")  // Declinada
// etc...
```

---

## 🔄 Cambiar de Mock a Real

### Paso 1: Obtener Claves Stripe
```
https://stripe.com → Developers → API Keys
```

### Paso 2: Actualizar MauiProgram
```csharp
// Cambiar de:
var config = new ServiceConfiguration(useMock: true);

// A:
var config = new ServiceConfiguration(useMock: false);
```

### Paso 3: Establecer Variables de Entorno
```powershell
$env:STRIPE_SECRET_KEY = "sk_test_..."
```

### Paso 4: Listo
```
La app ahora usa StripePaymentService real
```

---

## 📈 Ventajas del Mock

| Aspecto | Mock | Real |
|---------|------|------|
| Configuración | ✅ Inmediata | ❌ Requiere Stripe |
| Costo | ✅ Gratis | ❌ Cargo por transacción |
| Velocidad | ✅ Rápido | ⚠️ Más lento |
| Testing | ✅ Fácil | ❌ Difícil sin credenciales |
| Desarrollo | ✅ Perfecto | ❌ Overkill |
| Producción | ❌ No usar | ✅ Requerido |

---

## 📝 Resumen

### Para Desarrollo
```
✓ Usa MockEventService + MockPaymentService
✓ Permite prototipado rápido
✓ Sin dependencias externas
✓ Testing local fácil
```

### Para Producción
```
✓ Cambiar a useMock: false
✓ Configurar Stripe
✓ Usar StripePaymentService
✓ Validaciones server-side
```

---

## 🎯 Próximos Pasos

1. **Ejecutar App** con Mock (actual)
2. **Prototipar** y probar flujos
3. **Agregar SQLite** si es necesario
4. **Configurar Stripe** cuando esté listo
5. **Cambiar a Real** (useMock: false)

---

¡Ahora puedes prototipar completamente sin Stripe! 🚀
