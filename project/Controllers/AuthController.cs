

using Microsoft.AspNetCore.Mvc;

public class AuthController : Controller
{
    [HttpGet("auth/login")]
    public IActionResult Login()
    {
        return View("~/Views/Auth/VistaRegistroUsuario.cshtml");
    }
}