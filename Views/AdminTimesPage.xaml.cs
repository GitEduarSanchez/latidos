using Latidos.ViewModels;

namespace Latidos.Views;

public partial class AdminTimesPage : ContentPage
{
    public AdminTimesPage()
    {
        InitializeComponent();
        BindingContext = new AdminTimesViewModel();
    }
}
