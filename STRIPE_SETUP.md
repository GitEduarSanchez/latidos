# Configuración Stripe - Guía Detallada

## 🔐 Obtener Claves de Stripe

### Paso 1: Crear Cuenta
1. Ve a https://stripe.com/
2. Haz clic en "Sign up"
3. Completa el formulario con tu información
4. Verifica tu email

### Paso 2: Acceder al Dashboard
1. Inicia sesión en https://dashboard.stripe.com
2. Completa la verificación de identidad (KYC)
3. Verifica tu número de teléfono

### Paso 3: Obtener Claves
1. En el menú izquierdo, ve a **Developers** > **API Keys**
2. Asegúrate que estés en modo **Test** (Switch arriba a la derecha)
3. Copia:
   - **Secret Key** (comienza con `sk_test_`)
   - **Publishable Key** (comienza con `pk_test_`)

```
Ejemplo:
Secret Key:     sk_test_51234567890abcdef...
Publishable Key: pk_test_51234567890abcdef...
```

---

## 🔧 Configurar en la Aplicación

### Opción 1: Variables de Entorno (Recomendado para Desarrollo)

#### En Windows (PowerShell):
```powershell
# Crear variables de entorno temporales (solo para sesión actual)
$env:STRIPE_SECRET_KEY = "sk_test_YOUR_KEY_HERE"
$env:STRIPE_PUBLISHABLE_KEY = "pk_test_YOUR_KEY_HERE"

# Para permanente, usa:
[Environment]::SetEnvironmentVariable("STRIPE_SECRET_KEY", "sk_test_YOUR_KEY_HERE", "User")
[Environment]::SetEnvironmentVariable("STRIPE_PUBLISHABLE_KEY", "pk_test_YOUR_KEY_HERE", "User")

# Luego reinicia Visual Studio
```

#### En macOS/Linux:
```bash
# Agregar a ~/.bash_profile o ~/.zshrc
export STRIPE_SECRET_KEY="sk_test_YOUR_KEY_HERE"
export STRIPE_PUBLISHABLE_KEY="pk_test_YOUR_KEY_HERE"

# Aplicar cambios
source ~/.bash_profile  # o source ~/.zshrc
```

### Opción 2: Archivo de Configuración

Edita `appsettings.json`:
```json
{
  "Stripe": {
    "SecretKey": "sk_test_YOUR_KEY_HERE",
    "PublishableKey": "pk_test_YOUR_KEY_HERE"
  },
  "PaymentGateway": {
    "Provider": "Stripe",
    "Environment": "Sandbox"
  }
}
```

⚠️ **IMPORTANTE**: Nunca hagas commit de este archivo con datos reales en Git

```bash
# Agregar a .gitignore
echo "appsettings.json" >> .gitignore
```

### Opción 3: Secrets de Usuario (Desarrollo Seguro)

```bash
# En la carpeta del proyecto
dotnet user-secrets init

# Agregar claves
dotnet user-secrets set "Stripe:SecretKey" "sk_test_YOUR_KEY_HERE"
dotnet user-secrets set "Stripe:PublishableKey" "pk_test_YOUR_KEY_HERE"

# Ver secretos (enmascarados)
dotnet user-secrets list

# Borrar todos
dotnet user-secrets clear
```

---

## 🧪 Tarjetas de Prueba de Stripe

### Pagos Exitosos
```
Número:      4242 4242 4242 4242
Expiración:  Cualquier fecha futura (ej: 12/25)
CVV:         Cualquier número (ej: 123)
Resultado:   ✓ Pago aprobado
```

### Pagos Rechazados
```
Número:      4000 0000 0000 0002
Expiración:  Cualquier fecha futura
CVV:         Cualquier número
Resultado:   ✗ Tarjeta declinada
```

### Errores Específicos
```
4000 0000 0000 0069     → Tarjeta expirada
4000 0025 0000 3155     → Requiere 3D Secure
4000 0000 0000 0127     → CVC inválido
5555 5555 5555 4444     → Mastercard (aprobada)
3782 822463 10005       → American Express (aprobada)
```

---

## 🔄 Testear la Integración

### Test Básico en la Aplicación

1. **Ejecutar la app**
   ```bash
   dotnet run
   ```

2. **Agregar eventos al carrito**
   - Haz clic en "Add to Cart" varios eventos

3. **Ir a Checkout**
   - Haz clic en "Proceed to Checkout"

4. **Completar formulario**
   - Nombre: Cualquier nombre
   - Email: test@example.com
   - Tarjeta: 4242 4242 4242 4242
   - Expiración: 12/25
   - CVV: 123
   - ✓ Acepta términos

5. **Procesar pago**
   - Haz clic en "Process Payment"

### Resultado Esperado
```
✓ Payment successful! Order #: ch_1234567890abcdef...
Deberías ver confirmación y volver a Eventos
```

---

## 📊 Monitorear Transacciones

### Ver Pagos en Dashboard

1. Ve a https://dashboard.stripe.com
2. En la barra lateral izquierda: **Payments**
3. Verás todos tus pagos de prueba listados
4. Haz clic en uno para ver detalles

### Información Disponible
- Monto
- Fecha/Hora
- Estado (Succeeded, Failed)
- ID de transacción
- Información del cliente
- Detalles de la tarjeta

---

## 🚀 Próximas Etapas (Antes de Producción)

### 1. Generar Token en el Cliente

En producción, NUNCA envíes el número de tarjeta directamente. Usa Stripe.js:

```javascript
// Cliente (JavaScript)
const stripe = Stripe('pk_test_YOUR_KEY');

// Crear elemento de tarjeta
const cardElement = elements.create('card');
cardElement.mount('#card-element');

// Generar token
const {token} = await stripe.createToken(cardElement);

// Enviar token al backend
// Nunca el número de tarjeta
```

En .NET MAUI, puedes usar una WebView para Stripe.js o una librería específica.

### 2. Usar Claves de Producción

Cuando estés listo para producción:

1. Ve a Stripe Dashboard
2. Desactiva el modo Test (Switch)
3. Copia tus claves de **Producción**
4. Actualiza variables de entorno
5. Actualiza URLs a producción

```csharp
// En StripePaymentService
private readonly string _stripeApiUrl = 
    Environment.GetEnvironmentVariable("ENVIRONMENT") == "Production" 
        ? "https://api.stripe.com/v1"  // Producción
        : "https://api.stripe.com/v1";   // Test
```

### 3. Implementar Webhooks

Los webhooks notifican tu app de eventos de Stripe:

```csharp
// Escuchar eventos de Stripe
// Ejemplo: charge.succeeded, charge.failed

[HttpPost("webhook")]
public async Task<IActionResult> StripeWebhook()
{
    var json = await new StreamReader(HttpContext.Request.Body)
        .ReadToEndAsync();

    var stripeEvent = EventUtility.ConstructEvent(json, 
        Request.Headers["Stripe-Signature"], 
        _webhookSecret);

    switch (stripeEvent.Type)
    {
        case "charge.succeeded":
            // Procesar pago exitoso
            break;
        case "charge.failed":
            // Procesar pago fallido
            break;
    }

    return Ok();
}
```

### 4. Documentar para Seguridad

```
STRIPE SECURITY CHECKLIST
- [ ] Claves solo en variables de entorno
- [ ] Nunca logear números de tarjeta completos
- [ ] HTTPS en todas las comunicaciones
- [ ] Validación server-side de montos
- [ ] Rate limiting en API de pagos
- [ ] Auditoría de transacciones
- [ ] Políticas de privacidad actualizadas
- [ ] Términos de servicio con info de pagos
- [ ] Soporte GDPR si aplica
- [ ] Pruebas de seguridad penetración
```

---

## 🆘 Problemas Comunes

### ❌ "Invalid API Key"
**Causa**: Clave incorrecta o no configurada
**Solución**:
- Copia exactamente desde Stripe Dashboard
- Verifica que sea modo Test (`sk_test_`)
- Reinicia la aplicación después de cambiar variables

### ❌ "Your card was declined"
**Causa**: Tarjeta de prueba incorrecta o formato inválido
**Solución**:
- Usa tarjeta `4242 4242 4242 4242`
- Verifica no haya espacios/guiones extras
- Expiración debe ser futura
- CVV debe ser 3 dígitos

### ❌ "Unauthorized"
**Causa**: Secret Key no configurada o incorrecta
**Solución**:
- Verifica `STRIPE_SECRET_KEY` está definida
- Usa la Secret Key, no Publishable Key
- Reinicia la aplicación

### ❌ "Network Error"
**Causa**: Sin conexión a internet o Stripe API caída
**Solución**:
- Verifica conexión internet
- Revisa estado de Stripe: https://status.stripe.com
- Implementa reintentos automáticos

---

## 📈 Pasos para Escalar

### Fase 1: MVP (Actual)
- ✓ Pagos básicos con Stripe
- ✓ Base de datos en memoria
- ✓ Sin autenticación

### Fase 2: Beta
- Base de datos SQLite local
- Autenticación básica
- Notificaciones por email

### Fase 3: Producción
- Backend REST API
- Base de datos en cloud
- Autenticación OAuth
- Admin panel
- Múltiples métodos de pago

---

## 📚 Recursos Oficial de Stripe

- **API Docs**: https://stripe.com/docs/api
- **Testing**: https://stripe.com/docs/testing
- **Client Libraries**: https://stripe.com/docs/libraries
- **Dashboard**: https://dashboard.stripe.com
- **Status Page**: https://status.stripe.com
- **Support**: https://support.stripe.com

---

## ✅ Verificación Final

Antes de decir que está listo:

```
[ ] Claves Stripe configuradas
[ ] Tarjeta de prueba funcionando
[ ] Pago exitoso procesa orden
[ ] Pago rechazado muestra error
[ ] Variables de entorno securizadas
[ ] Código compilable sin warnings
[ ] Logs funcionan correctamente
[ ] Manejo de errores implementado
[ ] Documentación actualizada
```

¡Listo para usar! 🎉

---

**Última actualización**: Enero 2025
