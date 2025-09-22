

using project.Models;

public interface IAuthService
{
    Task<(string?, bool)> Login(string email, string password);
    Task<(string?, bool)> Logout();
}