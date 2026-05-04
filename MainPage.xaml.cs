namespace Latidos
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnCounterClicked(object? sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Tocado {count} vez";
            else
                CounterBtn.Text = $"Tocado {count} veces";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }
}
