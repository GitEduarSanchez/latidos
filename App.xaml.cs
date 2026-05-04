using Microsoft.Extensions.DependencyInjection;

namespace Latidos
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Force light theme for Latidos brand
            UserAppTheme = AppTheme.Light;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}
