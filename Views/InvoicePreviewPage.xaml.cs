namespace Latidos.Views;

public partial class InvoicePreviewPage : ContentPage
{
    public InvoicePreviewPage(string invoicePath)
    {
        InitializeComponent();

        if (Path.GetExtension(invoicePath).Equals(".html", StringComparison.OrdinalIgnoreCase) &&
            File.Exists(invoicePath))
        {
            InvoiceWebView.Source = new HtmlWebViewSource
            {
                Html = File.ReadAllText(invoicePath)
            };
            return;
        }

        InvoiceWebView.Source = new UrlWebViewSource
        {
            Url = invoicePath.StartsWith("file://", StringComparison.OrdinalIgnoreCase)
                ? invoicePath
                : $"file:///{invoicePath.Replace("\\", "/")}"
        };
    }

    private async void OnCloseClicked(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
