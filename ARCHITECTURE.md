# Diagrama de Arquitectura - Latidos App

## 🏗️ Arquitectura General

```
┌─────────────────────────────────────────────────────────────┐
│                    LATIDOS APPLICATION                      │
│                      (.NET MAUI 10.0)                       │
└─────────────────────────────────────────────────────────────┘
                              │
        ┌─────────────────────┼─────────────────────┐
        │                     │                     │
        ▼                     ▼                     ▼
    ┌────────────┐       ┌────────────┐       ┌────────────┐
    │ Events Tab │       │ Cart Tab   │       │ Checkout   │
    │  (XAML)    │       │  (XAML)    │       │  (XAML)    │
    └──────┬─────┘       └──────┬─────┘       └──────┬─────┘
           │                    │                    │
           └────────┬───────────┴────────┬───────────┘
                    │                    │
                    ▼                    ▼
           ┌────────────────────────────────────┐
           │    VIEWMODELS (Data Binding)       │
           │  - EventsViewModel                 │
           │  - CartViewModel                   │
           │  - CheckoutViewModel               │
           └────────────────────────────────────┘
                    │
                    ▼
           ┌────────────────────────────────────┐
           │   SERVICES (Business Logic)        │
           │  ┌────────────────────────────┐    │
           │  │ IEventService              │    │
           │  │ EventService               │    │
           │  └────────────────────────────┘    │
           │  ┌────────────────────────────┐    │
           │  │ ICartService               │    │
           │  │ CartService                │    │
           │  └────────────────────────────┘    │
           │  ┌────────────────────────────┐    │
           │  │ IPaymentService            │    │
           │  │ StripePaymentService       │    │
           │  └────────────────────────────┘    │
           │  ┌────────────────────────────┐    │
           │  │ IOrderService              │    │
           │  │ OrderService               │    │
           │  └────────────────────────────┘    │
           └────────────────────────────────────┘
                    │
                    ▼
           ┌────────────────────────────────────┐
           │    MODELS (Data Structures)        │
           │  - RunningEvent                    │
           │  - CartItem                        │
           │  - PaymentRequest                  │
           │  - PaymentResponse                 │
           │  - Order                           │
           │  - OrderItem                       │
           └────────────────────────────────────┘
                    │
           ┌────────┴──────────┐
           ▼                   ▼
      ┌─────────────┐    ┌──────────────────┐
      │  In-Memory  │    │  Stripe API      │
      │   Storage   │    │  (Payment)       │
      └─────────────┘    └──────────────────┘
```

---

## 📊 Flujo de Datos

### 1. Flujo de Eventos

```
EventsPage (UI)
    │ Display Command
    ▼
EventsViewModel
    │ LoadEventsCommand
    ▼
IEventService.GetAllEventsAsync()
    │
    ▼
EventService (In-Memory)
    │ Retorna List<RunningEvent>
    ▼
EventsViewModel
    │ Events Property Updated
    ▼
EventsPage (Binding Actualizado)
    │ CollectionView Renderizado
    ▼
Usuario ve eventos
```

### 2. Flujo de Carrito

```
EventsPage (Button Click)
    │ AddToCartCommand
    ▼
EventsViewModel
    │ Crea CartItem
    ▼
ICartService.AddToCartAsync()
    │
    ▼
CartService (In-Memory)
    │ Agrega a _cartItems
    ▼
CartPage (Tab Click)
    │
    ▼
CartViewModel.LoadCartAsync()
    │
    ▼
CartService.GetCartItemsAsync()
    │ Retorna List<CartItem>
    ▼
CartViewModel
    │ CartItems Updated
    ▼
CartPage (Binding Actualizado)
    │
    ▼
Usuario ve carrito
```

### 3. Flujo de Pago

```
CheckoutPage (Formulario)
    │ ProcessPaymentCommand
    ▼
CheckoutViewModel.ProcessPaymentAsync()
    │ Valida datos
    ▼
PaymentRequest creado
    │
    ▼
IPaymentService.ProcessPaymentAsync()
    │
    ▼
StripePaymentService
    │ HttpClient.PostAsync() → Stripe API
    ▼
Stripe (Servidores)
    │ Procesa pago
    ▼
PaymentResponse retornada
    │
    ▼
CheckoutViewModel
    │ Si exitoso: IOrderService.CreateOrderAsync()
    ▼
OrderService (In-Memory)
    │ Crea Order
    ▼
ICartService.ClearCartAsync()
    │ Vacía carrito
    ▼
CheckoutViewModel
    │ Navigate to Events
    ▼
EventsPage
    │
    ▼
Usuario ve confirmación
```

---

## 🔄 Ciclo de Vida de la Aplicación

```
Inicio
  ↓
App.xaml.cs
  ↓
MauiProgram.cs
  ├─ Registrar Servicios (DI)
  │  ├─ IEventService
  │  ├─ ICartService
  │  ├─ IPaymentService
  │  └─ IOrderService
  │
  ├─ Registrar Views
  │  ├─ EventsPage
  │  ├─ CartPage
  │  └─ CheckoutPage
  │
  └─ Build()
       ↓
AppShell.xaml
  ├─ TabBar
  │  ├─ Events Route
  │  └─ Cart Route
  │
  └─ Checkout Route
       ↓
EventsPage (Default)
  │
  └─ Usuario interactúa
       ↓
Navegación entre páginas
       ↓
Fin (Salida)
```

---

## 🏛️ Estructura de Directorios

```
Latidos/
│
├── Models/                         (Capas de Datos)
│   ├── RunningEvent.cs
│   ├── CartItem.cs
│   ├── PaymentRequest.cs
│   ├── PaymentResponse.cs
│   └── Order.cs
│
├── Services/                       (Capas de Negocio)
│   ├── Interfaces (I*)
│   │   ├── IEventService.cs
│   │   ├── ICartService.cs
│   │   ├── IPaymentService.cs
│   │   └── IOrderService.cs
│   │
│   └── Implementaciones
│       ├── EventService.cs
│       ├── CartService.cs
│       ├── StripePaymentService.cs
│       └── OrderService.cs
│
├── Views/                          (Presentación XAML)
│   ├── EventsPage.xaml
│   ├── EventsPage.xaml.cs
│   ├── CartPage.xaml
│   ├── CartPage.xaml.cs
│   ├── CheckoutPage.xaml
│   └── CheckoutPage.xaml.cs
│
├── ViewModels/                     (Lógica de Presentación)
│   ├── EventsViewModel.cs
│   ├── CartViewModel.cs
│   └── CheckoutViewModel.cs
│
├── Recursos/
│   └── AppIcon/
│   └── Fonts/
│   └── Images/
│   └── Raw/
│
├── App.xaml                        (Config Global)
├── App.xaml.cs
├── AppShell.xaml                   (Navegación)
├── AppShell.xaml.cs
├── MauiProgram.cs                  (Inyección Dependencias)
├── appsettings.json                (Configuración)
│
├── Latidos.csproj                  (Definición Proyecto)
│
└── Documentación/
    ├── README.md
    ├── IMPLEMENTATION_GUIDE.md
    ├── STRIPE_SETUP.md
    ├── CODE_EXAMPLES.md
    ├── GETTING_STARTED.md
    └── ARCHITECTURE.md (este archivo)
```

---

## 🔌 Inyección de Dependencias (DI)

```
MauiProgram.CreateMauiApp()
  │
  └─ builder.Services
      │
      ├─ AddSingleton<IEventService, EventService>()
      │  └─ Mismo instancia durante toda la app
      │
      ├─ AddSingleton<ICartService, CartService>()
      │  └─ Una sola instancia compartida
      │
      ├─ AddSingleton<IOrderService, OrderService>()
      │  └─ Una sola instancia compartida
      │
      └─ AddSingleton<IPaymentService, StripePaymentService>()
         └─ Una sola instancia compartida
```

### Ventajas
- Desacoplamiento entre capas
- Facilita testing
- Código más mantenible
- Cambios de implementación sin afectar clientes

---

## 💾 Almacenamiento de Datos (Actual)

```
┌──────────────────┐
│  Application     │
│  (Running)       │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│  In-Memory Lists │
│  (RAM)           │
│                  │
│  - Events List   │
│  - Cart Items    │
│  - Orders List   │
└──────────────────┘
         │
         ▼
┌──────────────────┐
│  Se pierde al    │
│  cerrar app      │
└──────────────────┘
```

### Futuro (Persistencia)

```
┌──────────────────┐
│  Application     │
│  (Running)       │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│  SQLite Database │
│                  │
│  - Events        │
│  - Cart          │
│  - Orders        │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│  Almacenamiento  │
│  Persistente     │
│  Local del Tel   │
└──────────────────┘
```

---

## 🔗 Patrones de Diseño Usados

### 1. **MVVM (Model-View-ViewModel)**
```
View (XAML)
   │ Data Binding (Two-way)
   ▼
ViewModel (Properties + Commands)
   │ Uses
   ▼
Services (Business Logic)
   │ Works with
   ▼
Models (Data)
```

### 2. **Repository Pattern**
```
Interface (Contract)
   │ Implemented by
   ▼
Service Class
   │ Uses
   ▼
Data Storage (Memoria/BD)
```

### 3. **Dependency Injection**
```
MauiProgram.cs
   │ Registers
   ▼
Service Interfaces + Implementations
   │ Injected into
   ▼
ViewModels (Constructor)
```

### 4. **Observer Pattern**
```
BindableObject
   │ Implements INotifyPropertyChanged
   ▼
ViewModel (Observable)
   │ Notifica cambios
   ▼
View (XAML Binding)
   │ Actualiza UI automáticamente
```

---

## 🚦 Flujo de Control (High Level)

```
┌─ INICIO ─────────────────────────────────────────────┐
│                                                       │
│  1. App.xaml.cs → MainPage                           │
│  2. AppShell.xaml → Navegación                       │
│  3. MauiProgram.cs → Servicios registrados           │
│                                                       │
└─────────────────┬─────────────────────────────────────┘
                  │
              ┌───┴───┐
              ▼       ▼
         ┌────────┐ ┌───────┐
         │ Events │ │ Cart  │
         │ (Tab1) │ │(Tab2) │
         └───┬────┘ └───┬───┘
             │         │
             └────┬────┘
                  ▼
            ┌─────────────┐
            │ Checkout    │
            │ (Shell Route)
            └──────┬──────┘
                   │
                   ▼
            ┌─────────────┐
            │ Stripe API  │
            │ (Payment)   │
            └──────┬──────┘
                   │
            ┌──────┴──────┐
            ▼             ▼
        SUCCESS        FAILURE
            │             │
            └──────┬──────┘
                   ▼
            ┌─────────────┐
            │ Confirmation│
            │ or Retry    │
            └─────────────┘

└─ FIN ─────────────────────────────────────────────────┘
```

---

## 📈 Escabilidad Futura

### Fase 1 (Actual - MVP)
```
Latidos (MAUI Client)
   │
   └─ In-Memory Services
      └─ Stripe API
```

### Fase 2 (Backend Simple)
```
Latidos (MAUI Client)
   │
   └─ REST API (ASP.NET Core)
      ├─ SQL Server / PostgreSQL
      └─ Stripe API
```

### Fase 3 (Escalado)
```
Latidos (MAUI Client)
   │
   └─ API Gateway
      ├─ Events Microservice
      ├─ Cart Microservice
      ├─ Payment Microservice
      │  └─ Stripe API
      ├─ Orders Microservice
      │
      └─ Databases
         ├─ PostgreSQL (Main)
         └─ Redis (Cache)
```

---

## 🔐 Seguridad - Capas

```
┌─────────────────────────────────────────┐
│  Nivel 4: API & Autenticación           │
│  - OAuth / JWT Tokens                   │
│  - Rate Limiting                        │
│  - CORS                                 │
└─────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────┐
│  Nivel 3: Validación                    │
│  - Input Validation (Client & Server)   │
│  - Business Rules                       │
│  - Authorization                        │
└─────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────┐
│  Nivel 2: Encriptación                  │
│  - HTTPS TLS                            │
│  - Datos sensibles cifrados             │
│  - Secure storage                       │
└─────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────┐
│  Nivel 1: Infraestructura                │
│  - Firewalls                            │
│  - DDoS Protection                      │
│  - Backups                              │
└─────────────────────────────────────────┘
```

---

## 📊 Tabla Resumen de Componentes

| Componente | Tipo | Responsabilidad | Dependencias |
|-----------|------|-----------------|--------------|
| EventsPage | View | Mostrar eventos | EventsViewModel |
| CartPage | View | Mostrar carrito | CartViewModel |
| CheckoutPage | View | Formulario pago | CheckoutViewModel |
| EventsViewModel | ViewModel | Lógica eventos | IEventService |
| CartViewModel | ViewModel | Lógica carrito | ICartService |
| CheckoutViewModel | ViewModel | Lógica pago | IPaymentService, IOrderService |
| EventService | Service | Gestionar eventos | Models.RunningEvent |
| CartService | Service | Gestionar carrito | Models.CartItem |
| StripePaymentService | Service | Procesar pagos | Stripe API |
| OrderService | Service | Gestionar órdenes | Models.Order |
| RunningEvent | Model | Datos evento | - |
| CartItem | Model | Datos carrito | - |
| Order | Model | Datos orden | - |

---

¡Listo! Tu arquitectura es escalable, mantenible y sigue buenas prácticas. 🎉
