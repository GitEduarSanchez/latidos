# 🚀 Quick Start - Prototipado con Mock

## ✅ Estado Actual

Tu aplicación está **100% lista para prototipar** con servicios Mock:

```
✓ MockPaymentService activo
✓ MockEventService activo  
✓ 6 eventos de prueba incluidos
✓ Tarjetas de prueba configuradas
✓ Compilación exitosa
✓ Listo para ejecutar
```

---

## 🎬 Ejecutar Ahora

### En PowerShell:

```powershell
cd C:\Users\EDUAR\source\repos\Latidos\
dotnet run
```

### O en Visual Studio:
```
Presiona F5 para ejecutar
```

---

## 🎮 Probar Flujo Completo (5 min)

### 1️⃣ Ver Eventos
```
→ La app abre en "Events" tab
→ Ves 6 eventos de running
→ Cada uno con precio y participantes
```

**Eventos disponibles:**
- 5K City Morning Run - $25
- Half Marathon - $45
- Trail Running 10K - $35
- Women's 10K Charity - $30
- Sprint Series - $20
- Night Glow Run - $35

### 2️⃣ Agregar al Carrito
```
→ Haz clic "Add to Cart" en cualquier evento
→ Aparece "Success - Added X to cart"
→ Repite con 2-3 eventos más
```

### 3️⃣ Ver Carrito
```
→ Haz clic en "Cart" tab
→ Ves los eventos agregados
→ Total calculado automáticamente
```

### 4️⃣ Ir a Checkout
```
→ Haz clic "Proceed to Checkout"
→ Se abre formulario de pago
```

### 5️⃣ Completar Pago
```
Formulario:
- Nombre: Juan Pérez
- Email: juan@example.com
- Tarjeta: 4242 4242 4242 4242
- Expiración: 12/25
- CVV: 123
- ✓ Aceptar términos

→ Haz clic "Process Payment"
```

### 6️⃣ Confirmación
```
→ Ves mensaje de éxito
→ Vuelves a "Events"
→ Carrito está vacío
→ ¡Listo para nueva compra!
```

---

## 🧪 Probar Diferentes Escenarios

### ✅ Compra Exitosa
```
Tarjeta: 4242 4242 4242 4242
Resultado: ✓ Pago exitoso - Order #: ch_mock_1000_...
```

### ✗ Tarjeta Rechazada
```
Tarjeta: 4000 0000 0000 0002
Resultado: ✗ Your card was declined
```

### ✗ Tarjeta Expirada
```
Tarjeta: 4000 0000 0000 0069
Resultado: ✗ Your card has expired
```

### ✗ Requiere Autenticación
```
Tarjeta: 4000 0025 0000 3155
Resultado: ✗ This card requires authentication
```

---

## 🔍 Qué Está Pasando (Behind the Scenes)

```
MockEventService
├─ 6 eventos en memoria
├─ Sin base de datos
├─ Datos siempre disponibles
└─ Simula latencia de BD

MockPaymentService
├─ Reconoce 4 tipos de tarjetas
├─ Genera IDs únicos
├─ Simula latencia de red (500-2000ms)
└─ Retorna respuestas realistas

CartService
├─ Guarda items en memoria
├─ Calcula totales
├─ Se limpia después de pago
└─ Persiste durante sesión

OrderService
├─ Crea órdenes después de pago
├─ Almacena en memoria
├─ Genera números únicos
└─ Ideal para testing
```

---

## 🎛️ Cambiar Configuración

### Activar Real Stripe (Cuando Esté Listo)

En `MauiProgram.cs`, línea ~15:

```csharp
// Cambiar de:
var config = new ServiceConfiguration(useMock: true);

// A:
var config = new ServiceConfiguration(useMock: false);
```

Luego necesitarás:
1. Claves Stripe válidas
2. Variables de entorno configuradas
3. Conexión a internet

---

## 📊 Datos Mock Disponibles

### Eventos en MockEventService

```
ID | Evento                    | Precio | Lugares | Locación
---|---------------------------|--------|---------|------------------
1  | 5K City Morning Run       | $25    | 245/500 | Central Park
2  | Half Marathon Championship| $45    | 156/300 | Downtown
3  | Trail Running Adventure   | $35    | 87/200  | Mountain Trail
4  | Women's 10K Charity Run   | $30    | 320/400 | Riverside
5  | Sprint Series - Week 1    | $20    | 145/150 | Athletic Complex
6  | Night Glow Run            | $35    | 412/600 | Downtown Loop
```

---

## 🐛 Troubleshooting Rápido

### ❌ "App no abre"
```
→ Ejecuta: dotnet clean
→ Luego: dotnet build
→ Finalmente: dotnet run
```

### ❌ "Eventos no aparecen"
```
→ Verifica que useMock: true en MauiProgram.cs
→ Compila: dotnet build
→ Ejecuta de nuevo
```

### ❌ "Pago no procesa"
```
→ Usa tarjeta: 4242 4242 4242 4242
→ Rellena campos correctamente
→ Acepta términos (checkbox)
→ Verifica que CanProcessPayment sea true
```

### ❌ "Carrito vacío"
```
→ Haz clic "Add to Cart" en un evento
→ Espera confirmación
→ Ve al tab "Cart"
→ Deberías ver el item
```

---

## 🎯 Próximas Mejoras Sugeridas

### Corto Plazo (Esta Semana)
- [ ] Agregar más eventos al mock
- [ ] Probar todos los flujos
- [ ] Validar UI en diferentes tamaños
- [ ] Capturar feedback

### Mediano Plazo (Este Mes)
- [ ] Agregar SQLite para persistencia local
- [ ] Implementar búsqueda de eventos
- [ ] Agregar filtros (precio, fecha, ubicación)
- [ ] Historial de órdenes local

### Largo Plazo (Futuro)
- [ ] Cambiar a Stripe real
- [ ] Backend API REST
- [ ] Base de datos en cloud
- [ ] Admin panel
- [ ] Notificaciones email

---

## 📝 Notas Importantes

### Mock vs Real

| Aspecto | Mock | Real |
|---------|------|------|
| Configuración | ✅ Inmediata | Requiere Stripe |
| Costo | ✅ Gratis | Cargo por trans. |
| Velocidad | ✅ Rápida | Más lenta |
| Datos | ✅ Hardcoded | Real del servidor |
| Uso | ✅ Desarrollo | ❌ NO para Prod |

### Cuando Cambiar a Real
- [ ] Funcionalidad completamente probada
- [ ] UI/UX validada
- [ ] Cuenta Stripe creada
- [ ] Variables de entorno listos
- [ ] Ready para usuarios reales

---

## 💡 Tips

1. **Repite el flujo varias veces** para asegurar estabilidad
2. **Prueba diferentes tarjetas** para ver comportamientos
3. **Agrega muchos eventos al carrito** para testear totales
4. **Limpia caché** si tienes problemas (Ctrl+Shift+Supr)
5. **Revisa consola** de Visual Studio para errores

---

## ✨ ¡Listo para Prototipar!

Tu aplicación Mock está **100% funcional**. Ahora puedes:

✅ Prototipar flujos
✅ Testear UI/UX
✅ Validar lógica de negocio
✅ Capturar requisitos reales
✅ Iterar rápidamente

**¡A por ello! 🚀**

---

**Si necesitas ayuda, consulta:**
- `MOCK_SERVICES_GUIDE.md` - Detalles técnicos
- `CODE_EXAMPLES.md` - Ejemplos de código
- `GETTING_STARTED.md` - Pasos detallados

Última actualización: Enero 2025
