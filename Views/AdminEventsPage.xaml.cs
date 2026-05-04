using Latidos.ViewModels;

namespace Latidos.Views;

public partial class AdminEventsPage : ContentPage
{
    public AdminEventsPage()
    {
        InitializeComponent();
        BindingContext = new AdminEventsViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is AdminEventsViewModel vm)
        {
            await vm.LoadEventsAsync();
        }
    }
}
