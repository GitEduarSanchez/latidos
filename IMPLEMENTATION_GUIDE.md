# Guía de Implementación - App Latidos

## 🎯 Descripción General

Latidos es una aplicación .NET MAUI completa para gestionar inscripciones a eventos de running con procesamiento de pagos integrado mediante Stripe.

## 📋 Tabla de Contenidos

1. [Requisitos Previos](#requisitos-previos)
2. [Estructura del Proyecto](#estructura-del-proyecto)
3. [Configuración Inicial](#configuración-inicial)
4. [Flujo de la Aplicación](#flujo-de-la-aplicación)
5. [Integración con Stripe](#integración-con-stripe)
6. [Próximos Pasos](#próximos-pasos)

---

## 🔧 Requisitos Previos

- **.NET 10 SDK** instalado
- **Visual Studio 2024** o superior con soporte para MAUI
- **Cuenta de Stripe** (https://stripe.com) - Sandbox para pruebas
- **Git** (opcional, para control de versiones)

---

## 📁 Estructura del Proyecto

```
Latidos/
├── Models/                          # Modelos de datos
│   ├── RunningEvent.cs             # Evento de running
│   ├── CartItem.cs                 # Artículo en carrito
│   ├── PaymentRequest.cs           # Solicitud de pago
│   ├── PaymentResponse.cs          # Respuesta de pago
│   └── Order.cs                    # Orden con detalles
│
├── Services/                        # Capa de servicios
│   ├── IEventService.cs            # Interfaz de eventos
│   ├── EventService.cs             # Implementación de eventos
│   ├── ICartService.cs             # Interfaz de carrito
│   ├── CartService.cs              # Implementación de carrito
│   ├── IPaymentService.cs          # Interfaz de pagos
│   ├── StripePaymentService.cs     # Integración Stripe
│   ├── IOrderService.cs            # Interfaz de órdenes
│   └── OrderService.cs             # Implementación de órdenes
│
├── Views/                           # Páginas XAML
│   ├── EventsPage.xaml             # Catálogo de eventos
│   ├── EventsPage.xaml.cs
│   ├── CartPage.xaml               # Carrito de compras
│   ├── CartPage.xaml.cs
│   ├── CheckoutPage.xaml           # Página de pago
│   └── CheckoutPage.xaml.cs
│
├── ViewModels/                      # ViewModels MVVM
│   ├── EventsViewModel.cs
│   ├── CartViewModel.cs
│   └── CheckoutViewModel.cs
│
├── App.xaml                         # Recursos globales
├── App.xaml.cs
├── AppShell.xaml                   # Navegación
├── AppShell.xaml.cs
├── MauiProgram.cs                  # Configuración DI
├── appsettings.json                # Configuración
├── Latidos.csproj                  # Configuración del proyecto
└── README.md

```

---

## ⚙️ Configuración Inicial

### 1. Clonar o Descargar el Proyecto

```bash
# Clonar (si está en un repositorio)
git clone <url-del-repo>
cd Latidos

# O descargar el ZIP desde GitHub
```

### 2. Restaurar Dependencias

```bash
dotnet restore
```

### 3. Construir el Proyecto

```bash
dotnet build
```

### 4. Configurar Variables de Entorno (Opcional pero Recomendado)

En Windows (PowerShell):
```powershell
$env:STRIPE_SECRET_KEY = "sk_test_YOUR_KEY_HERE"
$env:STRIPE_PUBLISHABLE_KEY = "pk_test_YOUR_KEY_HERE"
```

En macOS/Linux (Bash):
```bash
export STRIPE_SECRET_KEY="sk_test_YOUR_KEY_HERE"
export STRIPE_PUBLISHABLE_KEY="pk_test_YOUR_KEY_HERE"
```

---

## 🔄 Flujo de la Aplicación

### Vista General del Flujo

```
┌─────────────────┐
│  Events Page    │  ← Catálogo de eventos
│  (EventsPage)   │
└────────┬────────┘
         │ Agregar al carrito
         ▼
┌─────────────────┐
│  Cart Page      │  ← Revisa carrito
│  (CartPage)     │
└────────┬────────┘
         │ Proceder a checkout
         ▼
┌─────────────────┐
│ Checkout Page   │  ← Informa personal & pago
│(CheckoutPage)   │
└────────┬────────┘
         │ Procesar pago
         ▼
┌─────────────────┐
│  Stripe         │  ← Procesa el pago
│  Payment API    │
└────────┬────────┘
         │ Crear orden
         ▼
┌─────────────────┐
│  Order Created  │  ← Confirmación
│  (Back to Events)
└─────────────────┘
```

### Página 1: Events (Catálogo)
- Muestra todos los eventos de running disponibles
- Información: Nombre, Descripción, Fecha, Localización, Precio, Participantes
- Botones: "Add to Cart" y enlace al "Cart"

### Página 2: Cart (Carrito)
- Visualiza todos los artículos agregados
- Permite eliminar artículos
- Muestra el total
- Botones: "Proceed to Checkout" y "Continue Shopping"

### Página 3: Checkout (Pago)
- Formulario de información del cliente (nombre, email)
- Campos de tarjeta (número, expiración, CVV)
- Checkbox de términos y condiciones
- Botón "Process Payment"

---

## 💳 Integración con Stripe

### Conceptos Clave

La integración con Stripe permite procesar pagos de manera segura:

1. **Token de Pago**: En producción, Stripe.js genera un token seguro
2. **API Secret**: Se usa en el backend (nunca en el cliente)
3. **Sandbox**: Ambiente de prueba sin dinero real

### Configurar Stripe

#### Paso 1: Obtener Claves

1. Ir a https://dashboard.stripe.com
2. Iniciar sesión o crear cuenta
3. Ir a "Developers" → "API Keys"
4. Copiar:
   - **Secret Key**: `sk_test_...` (confidencial)
   - **Publishable Key**: `pk_test_...` (público)

#### Paso 2: Agregar Claves a la App

**Opción A: Variables de Entorno (Recomendado)**

En tu `.csproj` o en el sistema operativo, define:
```
STRIPE_SECRET_KEY=sk_test_YOUR_KEY
STRIPE_PUBLISHABLE_KEY=pk_test_YOUR_KEY
```

**Opción B: Archivo de Configuración**

En `appsettings.json`:
```json
{
  "Stripe": {
    "SecretKey": "sk_test_YOUR_KEY",
    "PublishableKey": "pk_test_YOUR_KEY"
  }
}
```

### Tarjetas de Prueba

Usa estas tarjetas en Sandbox (expira cualquier fecha futura, CVV: cualquier número):

| Escenario | Número | Resultado |
|-----------|--------|-----------|
| Exitoso | 4242 4242 4242 4242 | Pago aprobado |
| Rechazado | 4000 0000 0000 0002 | Pago declinado |
| Expirada | 4000 0000 0000 0069 | Tarjeta expirada |
| Autenticación | 4000 0025 0000 3155 | Requiere 3D Secure |

---

## 🚀 Próximos Pasos

### Corto Plazo (MVP)
- [x] Modelo de datos
- [x] Servicios base
- [x] Interfaz de usuario básica
- [x] Integración Stripe
- [ ] Pruebas unitarias
- [ ] Manejo de errores mejorado

### Mediano Plazo
- [ ] **Base de datos SQLite** para persistencia local
- [ ] **Autenticación de usuarios** (login/registro)
- [ ] **Historial de órdenes** persistente
- [ ] **Notificaciones por email** mediante SendGrid
- [ ] **Webhooks de Stripe** para confirmaciones en tiempo real

### Largo Plazo
- [ ] **Backend API REST** (ASP.NET Core)
- [ ] **Base de datos en cloud** (SQL Server, PostgreSQL)
- [ ] **Admin panel** para gestionar eventos
- [ ] **Reportes de ventas**
- [ ] **Notificaciones push**
- [ ] **Localización multiidioma**
- [ ] **Múltiples métodos de pago** (PayPal, Apple Pay, Google Pay)

---

## 📝 Funciones Principales Implementadas

### ✅ EventService
```csharp
// Obtener todos los eventos
List<RunningEvent> events = await eventService.GetAllEventsAsync();

// Obtener evento específico
RunningEvent? evt = await eventService.GetEventByIdAsync(1);

// Crear nuevo evento
await eventService.AddEventAsync(new RunningEvent { ... });
```

### ✅ CartService
```csharp
// Agregar al carrito
await cartService.AddToCartAsync(cartItem);

// Obtener carrito
List<CartItem> items = await cartService.GetCartItemsAsync();

// Calcular total
decimal total = await cartService.GetCartTotalAsync();

// Vaciar carrito
await cartService.ClearCartAsync();
```

### ✅ StripePaymentService
```csharp
// Procesar pago
PaymentResponse response = await paymentService.ProcessPaymentAsync(
    new PaymentRequest { ... }
);

// Reembolsar
bool refunded = await paymentService.RefundAsync(transactionId, amount);
```

### ✅ OrderService
```csharp
// Crear orden
await orderService.CreateOrderAsync(order);

// Obtener orden
Order? order = await orderService.GetOrderByIdAsync(1);

// Órdenes del usuario
List<Order> userOrders = await orderService.GetUserOrdersAsync(email);
```

---

## 🧪 Testing

### Ejecutar Pruebas de Compilación

```bash
# Compilar en modo Debug
dotnet build -c Debug

# Compilar en modo Release
dotnet build -c Release
```

### Probar la Aplicación

**Windows:**
```bash
dotnet run --framework net10.0-windows10.0.19041.0
```

**Android:**
```bash
dotnet run --framework net10.0-android
```

---

## 🔐 Seguridad y Buenas Prácticas

### ⚠️ IMPORTANTE: Antes de Producción

1. **NUNCA** hagas commit de claves Stripe en Git
2. **SIEMPRE** usa variables de entorno
3. **VALIDA** todos los inputs en cliente y servidor
4. **USA** HTTPS en todas las comunicaciones
5. **IMPLEMENTA** autenticación de usuarios
6. **AUDITA** las transacciones regularmente

### Checklist de Seguridad

- [ ] Secretos en variables de entorno
- [ ] Validación de email del cliente
- [ ] Manejo de excepciones completo
- [ ] Logging de errores de pago
- [ ] CORS configurado correctamente
- [ ] Rate limiting implementado
- [ ] Datos sensibles no logueados
- [ ] HTTPS en producción

---

## 📚 Recursos Útiles

- **Documentación Stripe**: https://stripe.com/docs
- **MAUI Docs**: https://learn.microsoft.com/maui
- **.NET 10**: https://learn.microsoft.com/dotnet
- **Stripe Test Cards**: https://stripe.com/docs/testing

---

## 🆘 Solución de Problemas

### Problema: "Stripe API Key not configured"
**Solución**: 
- Verifica que `STRIPE_SECRET_KEY` esté definida como variable de entorno
- O actualiza `appsettings.json` con tu clave real

### Problema: "Payment failed"
**Solución**: 
- Verifica que uses una tarjeta válida de prueba
- Confirma que la clave Stripe sea de Sandbox (`sk_test_`)
- Revisa los logs de error en la consola

### Problema: "No defining declaration found for implementing declaration"
**Solución**: 
- Este proyecto NO usa MVVM Toolkit avanzado
- Usa `BindableObject` nativo de MAUI

---

## 👨‍💻 Contribuciones

Para reportar bugs o sugerir features:
1. Abre un issue en GitHub
2. Describe el problema o sugerencia
3. Proporciona pasos para reproducir (si es bug)

---

## 📄 Licencia

MIT License - Libre para usar, modificar y distribuir

---

## 📧 Contacto

Para preguntas o soporte: [tu-email@example.com]

**Última actualización**: Enero 2025
