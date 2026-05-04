using Latidos.Models;
using Latidos.Services;

namespace Latidos;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("registration", typeof(Views.RegistrationPage));
        ApplyRoleVisibility();
    }

    private void ApplyRoleVisibility()
    {
        var auth = IPlatformApplication.Current?.Services.GetService<IAuthService>();
        var role = auth?.CurrentUser?.Role ?? UserRole.Competitor;
        var isAdmin = role == UserRole.Admin;

        AdminTab.IsVisible = isAdmin;
        TimesTab.IsVisible = isAdmin;
    }
}
