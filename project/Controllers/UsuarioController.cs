

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
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

    [HttpPost("Usuario/actualizar")]
    public async Task<IActionResult> ActualizarUsuario(Usuario usuario)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        (string?, bool) result = await _usuarioService.UpdateUsuario(usuario);
        return Ok(result);
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
}