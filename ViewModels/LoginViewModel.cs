using System.Windows.Input;
using Latidos.Models;
using Latidos.Services;

namespace Latidos.ViewModels;

public class LoginViewModel : BindableObject
{
    private readonly IAuthService _authService;

    private string _username = "admin";
    public string Username
    {
        get => _username;
        set
        {
            if (_username == value) return;
            _username = value;
            OnPropertyChanged();
        }
    }

    private string _password = "admin123";
    public string Password
    {
        get => _password;
        set
        {
            if (_password == value) return;
            _password = value;
            OnPropertyChanged();
        }
    }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage == value) return;
            _statusMessage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasStatusMessage));
        }
    }

    public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);

    public ICommand LoginCommand { get; }

    public LoginViewModel()
    {
        _authService = IPlatformApplication.Current?.Services.GetService<IAuthService>() ?? new AuthService();
        LoginCommand = new Command(async () => await LoginAsync());
    }

    private async Task LoginAsync()
    {
        StatusMessage = string.Empty;
        var result = await _authService.LoginAsync(Username, Password);
        if (!result.Success)
        {
            StatusMessage = result.Message;
            return;
        }

        var shell = new AppShell();
        Application.Current!.Windows[0].Page = shell;
        await shell.GoToAsync("///events");
    }
}
