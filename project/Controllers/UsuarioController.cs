

using Microsoft.AspNetCore.Mvc;
using project.Models;

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
        if(!ModelState.IsValid) return BadRequest(ModelState);
        (string?, bool) result = await _usuarioService.UpdateUsuario(usuario);
        return Ok(result);
    }
}