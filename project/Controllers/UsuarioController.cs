

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using project.Helpers;
using project.Models;
[Authorize]
public class UsuarioController : Controller
{
    private readonly IUsuarioService _usuarioService;
    private readonly IAuthService _authService;
    public UsuarioController(IUsuarioService usuarioService, IAuthService authService)
    {
        _usuarioService = usuarioService;
        _authService = authService;
    }
    [HttpPost("Usuario/setearAvatar")]
    public async Task<IActionResult> SetearAvatar(int idUsuario, string avatarUrl)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Actualizar en la base de datos
        (string? error, bool success) = await _usuarioService.SetearAvatar(idUsuario, avatarUrl);

        if (success)
        {
            // Actualizar el claim del avatar en la sesión actual
            await _authService.UpdateAvatarClaim(avatarUrl);
        }

        return Ok(new { success = success, message = error });
    }
    [HttpGet("Usuario/obtenerAvatar/{idUsuario}")]
    public async Task<IActionResult> ObtenerAvatar(int idUsuario)
    {
        var (error, avatarUrl) = await _usuarioService.GetAvatarUrl(idUsuario);
        if (error != null) return NotFound();
        return Ok(avatarUrl);
    }
    [HttpGet("/Perfil/Usuario/{id}")]
    public async Task<IActionResult> ObtenerPerfilUsuario(int id)
    {
        var (error, usuario) = await _usuarioService.GetUsuarioById(id);
        if (error != null) return NotFound();
        return View("~/Views/Profiles/UserProfiles.cshtml", usuario);
    }
    // [HttpPost("Usuario/actualizar")]
    // public async Task<IActionResult> ActualizarUsuario(Usuario usuario)
    // {
    //     if (!ModelState.IsValid) return BadRequest(ModelState);
    //     (string?, bool) result = await _usuarioService.UpdateUsuario(usuario);
    //     return Ok(result);
    // }
    [HttpGet("Usuario/cambiarRol")]
    public async Task<IActionResult> CambiarRol(int idUsuario, int idRolActual)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        int rolACambiar = idRolActual == 1 ? 2 : 1;
        if (User.FindFirst("idUsuario")!.Value == idUsuario.ToString()) return BadRequest("No puedes cambiar tu propio rol");
        (string?, bool) result = await _usuarioService.CambiarRol(idUsuario, rolACambiar);
        if (result.Item1 != null && !result.Item2) return BadRequest(result.Item1);
        return Ok(result);
    }
    [HttpGet("Usuario/resetearContrasena")]
    public async Task<IActionResult> ResetearContrasena(int idUsuario) // , int idRolActual Agregar esto si es necesario
    {
        if (!ModelState.IsValid) return BadRequest("No se pudo resetear la contraseña");
        (string?, bool) result = await _usuarioService.resetearContraseña(idUsuario);
        if (result.Item1 != null && !result.Item2) return BadRequest(result.Item1);
        return Ok(result);
    }
    [HttpPost("Usuario/cambiar/contrasena")]
    public async Task<IActionResult> cambiarContraseña(string passwordActual, string password, string email, int idUsuario)
    {
        if (!ModelState.IsValid) return this.RedirectToActionWithError(nameof(ObtenerPerfilUsuario), "Usuario", "Los datos ingresados no son validos.", new { id = idUsuario });
        (string? errorServiceForValidateCredential, bool success) = await _usuarioService.validarCredenciales(email, passwordActual);
        if (errorServiceForValidateCredential != null && !success) return this.RedirectToActionWithError(nameof(ObtenerPerfilUsuario), "Usuario", "No pudimos validar sus credenciales: " + errorServiceForValidateCredential, new { id = idUsuario }, "No se pudo cambiar la contraseña.");
        (string? errorServiceForChangePassword, bool valid) = await _usuarioService.cambiarContraseña(idUsuario, password);
        if (errorServiceForChangePassword != null && !valid) return this.RedirectToActionWithError(nameof(ObtenerPerfilUsuario), "Usuario", errorServiceForChangePassword, new { id = idUsuario });
        return this.RedirectToActionWithSuccess(nameof(ObtenerPerfilUsuario), "Usuario", "Sus credenciales han cambiado exitosamente, por favor no olvide iniciar sesión con las nuevas credenciales." ,new { id = idUsuario }, "Contraseña cambiada exitosamente.");
    }
}