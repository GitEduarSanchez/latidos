using Latidos.Models;

namespace Latidos.Services;

public class AuthService : IAuthService
{
    public AppUser? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser != null;

    public Task<(bool Success, string Message)> LoginAsync(string fullName, string email, string password, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return Task.FromResult((false, "Escribe tu nombre."));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return Task.FromResult((false, "Escribe tu correo."));
        }

        if (role == UserRole.Admin && password != "admin123")
        {
            return Task.FromResult((false, "Contrasena de administrador invalida."));
        }

        if (role == UserRole.Competitor && string.IsNullOrWhiteSpace(password))
        {
            return Task.FromResult((false, "Escribe la contrasena."));
        }

        CurrentUser = new AppUser
        {
            FullName = fullName.Trim(),
            Email = email.Trim(),
            Role = role
        };

        return Task.FromResult((true, "Inicio de sesion exitoso."));
    }

    public Task LogoutAsync()
    {
        CurrentUser = null;
        return Task.CompletedTask;
    }
}
