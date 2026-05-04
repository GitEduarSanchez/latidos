using System.Windows.Input;
using Latidos.Models;
using Latidos.Services;

namespace Latidos.ViewModels;

public class LoginViewModel : BindableObject
{
    private readonly IAuthService _authService;

    private string _fullName = string.Empty;
    public string FullName
    {
        get => _fullName;
        set
        {
            if (_fullName == value) return;
            _fullName = value;
            OnPropertyChanged();
        }
    }

    private string _email = string.Empty;
    public string Email
    {
        get => _email;
        set
        {
            if (_email == value) return;
            _email = value;
            OnPropertyChanged();
        }
    }

    private string _password = string.Empty;
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

    private string _selectedRole = "Competidor";
    public string SelectedRole
    {
        get => _selectedRole;
        set
        {
            if (_selectedRole == value) return;
            _selectedRole = value;
            OnPropertyChanged();
        }
    }

    public List<string> Roles { get; } = new() { "Competidor", "Admin" };

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
        var role = SelectedRole == "Admin" ? UserRole.Admin : UserRole.Competitor;
        var result = await _authService.LoginAsync(FullName, Email, Password, role);
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
