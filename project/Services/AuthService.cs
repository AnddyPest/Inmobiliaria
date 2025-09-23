

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
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario!.Empleado.Nombre),
                new Claim("idUsuario", usuario!.idUsuario.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, usuario!.Rol.Nombre),
                new Claim("AvatarUrl", usuario!.AvatarUrl ?? string.Empty)
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
    // Cuando hacemos el actualizar avatar, para que se vea el avatarcito arriba en el navbar
    // necesitamos actualizar el claim del avatar en la cookie para que se vea el cambio
    // sin necesidad de que el usuario cierre sesion y vuelva a entrar
    // bueno, este metodo hace eso
    public async Task<(string?, bool)> UpdateAvatarClaim(string newAvatarUrl)
    {
        try
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                return ("Usuario no autenticado", false);
            }

            // Obtiene los claims de la sesion
            var currentClaims = user.Claims.ToList();

            // remueve el claim de avatar actual si existe
            var avatarClaim = currentClaims.FirstOrDefault(c => c.Type == "AvatarUrl");
            if (avatarClaim != null)
            {
                currentClaims.Remove(avatarClaim);
            }

            // agrega el nuevo url del avatar seleccionado
            currentClaims.Add(new Claim("AvatarUrl", newAvatarUrl ?? string.Empty));

            // actualiza la cookie con los nuevos claims
            var claimsIdentity = new ClaimsIdentity(currentClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTime.UtcNow.AddHours(8) };

            // reautentica con el nuevo conjunto de claimses y a la bosta
            await _httpContextAccessor.HttpContext!.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );

            return (null, true);
        }
        catch (Exception ex)
        {
            HelperFor.imprimirMensajeDeError(ex.Message, nameof(AuthService), nameof(UpdateAvatarClaim));
            return ("Error al actualizar el claim del avatar", false);
        }
    }

    
}