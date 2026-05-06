using Latidos.Models;

namespace Latidos.Services;

public interface IAuthService
{
    AppUser? CurrentUser { get; }
    bool IsAuthenticated { get; }
    Task<(bool Success, string Message)> LoginAsync(string username, string password);
    Task LogoutAsync();
}
