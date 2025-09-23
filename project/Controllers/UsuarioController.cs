

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
        if (User.FindFirst("idUsuario")!.Value == idUsuario.ToString()) return BadRequest("No puedes cambiar tu propio rol");
        (string?, bool) result = await _usuarioService.CambiarRol(idUsuario, rolACambiar);
        if (result.Item1 != null && !result.Item2) return BadRequest(result.Item1);
        return Ok(result);
    }
    [HttpGet("Usuario/resetearContrasena")]
    public async Task<IActionResult> ResetearContrasena(int idUsuario) // , int idRolActual Agregar esto si es necesario
    {
        System.Console.WriteLine("Linea33");
        if (!ModelState.IsValid) return BadRequest("No se pudo resetear la contraseña");
        System.Console.WriteLine("Linea38");
        //if(User.FindFirst("idUsuario")!.Value != idUsuario.ToString() && idRolActual == 1) return BadRequest("No puedes resetear la contraseña de otro administrador"); Pensar si es necesario
        (string?, bool) result = await _usuarioService.resetearContraseña(idUsuario);
        if (result.Item1 != null && !result.Item2) return BadRequest(result.Item1);
        System.Console.WriteLine("Linea43");
        return Ok(result);
    }
}