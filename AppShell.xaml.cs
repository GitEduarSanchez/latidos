namespace Latidos
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("registration", typeof(Views.RegistrationPage));
        }
    }
}
