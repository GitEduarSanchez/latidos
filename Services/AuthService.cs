using Latidos.Models;

namespace Latidos.Services;

public class AuthService : IAuthService
{
    private static readonly Dictionary<string, (string password, UserRole role, string fullName, string email)> _credentials = new()
    {
        { "admin", ("admin123", UserRole.Admin, "Administrador", "admin@latidos.com") },
        { "corredor", ("correr123", UserRole.Competitor, "Corredor Demo", "corredor@latidos.com") },
    };

    public AppUser? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser != null;

    public Task<(bool Success, string Message)> LoginAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return Task.FromResult((false, "Escribe tu usuario."));
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return Task.FromResult((false, "Escribe tu clave."));
        }

        if (!_credentials.TryGetValue(username.Trim().ToLowerInvariant(), out var stored))
        {
            return Task.FromResult((false, "Usuario no encontrado."));
        }

        if (stored.password != password)
        {
            return Task.FromResult((false, "Clave incorrecta."));
        }

        CurrentUser = new AppUser
        {
            FullName = stored.fullName,
            Email = stored.email,
            Role = stored.role
        };

        return Task.FromResult((true, "Inicio de sesion exitoso."));
    }

    public Task LogoutAsync()
    {
        CurrentUser = null;
        return Task.CompletedTask;
    }
}
