

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.ValueContentAnalysis;
using project.Helpers;
using project.Models;

public class AuthService : IAuthService
{
    private readonly IUsuarioService _usuarioService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public AuthService(IUsuarioService usuarioService, IHttpContextAccessor httpContextAccessor)
    {
        _usuarioService = usuarioService;
        _httpContextAccessor = httpContextAccessor;
    }
    public async Task<(string?, bool)> Login(string email, string password)
    {
        (string? errorService, Usuario? usuario) = await _usuarioService.GetUsuarioByEmail(email);
        if (errorService != null)
        {
            HelperFor.imprimirMensajeDeError(errorService, nameof(AuthService), nameof(Login));
            return (errorService, false);
        }
        (string?, bool) validarCredenciales = await _usuarioService.validarCredenciales(email, password);
        if (validarCredenciales.Item1 != null && !validarCredenciales.Item2)
        {
            HelperFor.imprimirMensajeDeError(validarCredenciales.Item1, nameof(AuthService), nameof(Login));
            return (validarCredenciales.Item1, false);
        }
        try
        {
            System.Console.WriteLine(usuario.Empleado);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario!.Empleado.Nombre),
                new Claim("idUsuario", usuario!.idUsuario.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, usuario!.Rol.Nombre)
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties{IsPersistent = true, ExpiresUtc = DateTime.UtcNow.AddHours(8)};
            await _httpContextAccessor.HttpContext!.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );
            return (null, true);
        }
        catch (Exception ex)
        {
            HelperFor.imprimirMensajeDeError(ex.Message, nameof(AuthService), nameof(Login)); 
            return ("Error al iniciar sesión: Internal Server Error", false);
        }
        

    }
    
    public async Task<(string?, bool)> Logout()
    {
        await _httpContextAccessor.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return (null, true);
    }

    
}