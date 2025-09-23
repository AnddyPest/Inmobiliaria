

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using project.Models;
[Authorize]
public class UsuarioController : Controller
{
    private readonly IUsuarioService _usuarioService;

    public UsuarioController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpPost("Usuario/actualizar")]
    public async Task<IActionResult> ActualizarUsuario(Usuario usuario)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        (string?, bool) result = await _usuarioService.UpdateUsuario(usuario);
        return Ok(result);
    }
    [HttpGet("Usuario/cambiarRol")]
    public async Task<IActionResult> CambiarRol(int idUsuario, int idRolActual)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        int rolACambiar = idRolActual == 1 ? 2 : 1;
        if(User.FindFirst("idUsuario")!.Value == idUsuario.ToString()) return BadRequest("No puedes cambiar tu propio rol");
        (string?, bool) result = await _usuarioService.CambiarRol(idUsuario, rolACambiar);
        if(result.Item1 != null && !result.Item2) return BadRequest(result.Item1);
        return Ok(result);
    }
}