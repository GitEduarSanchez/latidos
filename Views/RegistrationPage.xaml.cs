using Latidos.ViewModels;

namespace Latidos.Views;

public partial class RegistrationPage : ContentPage, IQueryAttributable
{
    public RegistrationPage()
    {
        InitializeComponent();
        BindingContext = new RegistrationViewModel();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.TryGetValue("eventId", out var value) ||
            !int.TryParse(value?.ToString(), out var eventId) ||
            BindingContext is not RegistrationViewModel vm)
        {
            return;
        }

        _ = vm.LoadEventAsync(eventId);
    }
}
