using Latidos.ViewModels;

namespace Latidos.Views;

public partial class CheckoutPage : ContentPage
{
    public CheckoutPage()
    {
        InitializeComponent();
        BindingContext = new CheckoutViewModel();
    }
}
