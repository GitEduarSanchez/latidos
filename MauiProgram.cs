using Microsoft.Extensions.Logging;
using Latidos.Services;
using Latidos.Views;

namespace Latidos
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Configuration
            var config = new ServiceConfiguration(useMock: true); // true = Mock, false = Real
            builder.Services.AddSingleton<IServiceConfiguration>(config);

            // Register Services
            if (config.UseMockServices)
            {
                // MOCK SERVICES - Para desarrollo y prototipado
                builder.Services.AddSingleton<IEventService, MockEventService>();
                builder.Services.AddSingleton<IPaymentService, MockPaymentService>();
            }
            else
            {
                // REAL SERVICES - Para producción
                builder.Services.AddSingleton<IEventService, EventService>();
                builder.Services.AddSingleton<HttpClient>();
                builder.Services.AddSingleton<IPaymentService, StripePaymentService>();
            }

            // Shared Services
            builder.Services.AddSingleton<ICartService, CartService>();
            builder.Services.AddSingleton<IOrderService, OrderService>();

            // Register Views
            builder.Services.AddSingleton<EventsPage>();
            builder.Services.AddSingleton<CartPage>();
            builder.Services.AddSingleton<CheckoutPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
