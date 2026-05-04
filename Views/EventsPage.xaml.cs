using Latidos.ViewModels;

namespace Latidos.Views;

public partial class EventsPage : ContentPage
{
    public EventsPage()
    {
        InitializeComponent();
        BindingContext = new EventsViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is EventsViewModel vm)
        {
            await vm.LoadEventsAsync();
        }
    }
}
