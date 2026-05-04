using System.Globalization;

namespace Latidos
{
    public partial class App : Application
    {
        public App()
        {
            var spanishCulture = new CultureInfo("es-ES");
            CultureInfo.DefaultThreadCurrentCulture = spanishCulture;
            CultureInfo.DefaultThreadCurrentUICulture = spanishCulture;

            InitializeComponent();

            // Force light theme for Latidos brand.
            UserAppTheme = AppTheme.Light;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}
