namespace Latidos.Views;

public partial class SplashPage : ContentPage
{
    public SplashPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RunIntroAsync();
        NavigateToLogin();
    }

    private async Task RunIntroAsync()
    {
        var ringsTask = AnimateRipplesAsync();

        await Task.WhenAll(
            Glow.FadeTo(1, 500, Easing.CubicOut),
            LogoFrame.FadeTo(1, 500, Easing.CubicOut),
            LogoFrame.ScaleTo(1.0, 600, Easing.SpringOut));

        await HeartbeatAsync();
        await Task.Delay(80);
        await HeartbeatAsync();

        await Task.WhenAll(
            Title.FadeTo(1, 350),
            Title.TranslateTo(0, 0, 350, Easing.CubicOut));

        await Task.WhenAll(
            Subtitle.FadeTo(1, 300),
            Subtitle.TranslateTo(0, 0, 300, Easing.CubicOut));

        await Task.Delay(700);

        await Task.WhenAll(
            LogoFrame.FadeTo(0, 350),
            Glow.FadeTo(0, 350),
            Title.FadeTo(0, 350),
            Subtitle.FadeTo(0, 350));

        await ringsTask;
    }

    private async Task HeartbeatAsync()
    {
        await LogoFrame.ScaleTo(1.08, 160, Easing.CubicOut);
        await LogoFrame.ScaleTo(1.0, 180, Easing.CubicIn);
    }

    private async Task AnimateRipplesAsync()
    {
        var first = RippleAsync(Ring1, 0);
        var second = RippleAsync(Ring2, 600);
        var third = RippleAsync(Ring1, 1200);
        await Task.WhenAll(first, second, third);
    }

    private static async Task RippleAsync(Border ring, int delayMs)
    {
        await Task.Delay(delayMs);
        ring.Scale = 1;
        ring.Opacity = 0.55;
        await Task.WhenAll(
            ring.ScaleTo(2.4, 1400, Easing.SinOut),
            ring.FadeTo(0, 1400, Easing.CubicIn));
    }

    private void NavigateToLogin()
    {
        if (Window != null)
        {
            Window.Page = new NavigationPage(new LoginPage());
        }
    }
}
