using Latidos.ViewModels;

namespace Latidos.Views;

public partial class CartPage : ContentPage
{
    public CartPage()
    {
        InitializeComponent();
        BindingContext = new CartViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is CartViewModel vm)
        {
            await vm.LoadCartAsync();
        }
    }
}
