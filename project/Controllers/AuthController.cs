

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using project.Helpers;
using project.Models;
[AllowAnonymous]
public class AuthController : Controller
{
    private readonly IEmpleadoService _empleadoService;
    private readonly IUsuarioService _usuarioService;
    private readonly IAuthService _authService;
    public AuthController(IEmpleadoService empleadoService, IUsuarioService usuarioService, IAuthService authService)
    {
        _empleadoService = empleadoService;
        _usuarioService = usuarioService;
        _authService = authService;
    }
    [HttpGet()]
    public IActionResult Login()
    {
        
        return View("~/Views/Auth/VistaIniciarSesion.cshtml");
    }
    [HttpGet("auth/registro")]
    public IActionResult Registro()
    {
        return View("~/Views/Auth/VistaRegistroUsuario.cshtml");
    }
    [HttpGet("auth/unauthorized")]
    public IActionResult UnauthorizedView()
    {
        return View("~/Views/Auth/VistaNoAutorizado.cshtml");
    }
    [HttpPost("auth/login")]
    public async Task<IActionResult> Login(string email, string contrasena)
    {
        if (!ModelState.IsValid)
            return this.RedirectToActionWithError(nameof(Login), ModelState.GetErrorMessages());
        (string?, bool) login = await _authService.Login(email, contrasena);
        if (login.Item1 != null && !login.Item2) return this.RedirectToActionWithError(nameof(Login), login.Item1);
        this.SweetAlertSuccess("Bienvenido!!", "Bienvenido!!");
        return RedirectToAction("Index", "Home");
    }
    [HttpPost("auth/registro")]
    public async Task<IActionResult> Registro(string email, string contrasena, int dni)
    {
        if (!ModelState.IsValid)
        {
            HelperFor.imprimirMensajeDeError("Error en el modelo", nameof(AuthController), nameof(Registro));
            return this.RedirectToActionWithError(nameof(Registro), ModelState.GetErrorMessages());
        }
        (string?, Empleado?) empleado = await _empleadoService.getEmpleadoByDni(dni);
        if (empleado.Item1 != null || empleado.Item2 == null)
        {
            HelperFor.imprimirMensajeDeError("El empleado no se encuentra registrado", nameof(AuthController), nameof(Registro));
            return this.RedirectToActionWithError(nameof(Registro), "El empleado no se encuentra registrado");
        }
        if (empleado.Item2.Estado == false)
        {
            HelperFor.imprimirMensajeDeError("El empleado se encuentra dado de baja", nameof(AuthController), nameof(Registro));
            return this.RedirectToActionWithError(nameof(Registro), "El empleado se encuentra dado de baja");
        }
        Usuario usuario = new Usuario();
        usuario.email = email;
        usuario.contrasena = AuthHelper.HashContraseña(contrasena);
        usuario.IdRol = 2;
        usuario.estado = true;
        (string?, Empleado? empleado) empleadoFromService = await _empleadoService.getEmpleadoByDni(dni);
        if (empleadoFromService.Item1 != null || empleadoFromService.empleado == null)
        {
            HelperFor.imprimirMensajeDeError("El empleado no se encuentra registrado", nameof(AuthController), nameof(Registro));
            return this.RedirectToActionWithError(nameof(Registro), "El empleado no se encuentra registrado");
        }

        (string? errorServicioCreateUsuario, bool, int idUsuarioFromService) registro = await _usuarioService.CreateUsuario(usuario);
        if (registro.Item1 != null && !registro.Item2) return this.RedirectToActionWithError(nameof(Registro), registro.errorServicioCreateUsuario!);

        (string?, bool) asignacionDeUsuario = await _empleadoService.asignarUsuario(registro.idUsuarioFromService, empleadoFromService.empleado.IdEmpleado);

        (string?, bool) login = await _authService.Login(usuario.email, contrasena);
        if (login.Item1 != null && !login.Item2) return this.RedirectToActionWithError(nameof(Registro), login.Item1);

        this.SweetAlertSuccess("Bienvenido!!", "Bienvenido!!");
        return RedirectToAction("Index", "Home");

    }
    [HttpGet("auth/logout")]
    public async Task<IActionResult> Logout()
    {
       
        var logout = await _authService.Logout();
        return this.RedirectToActionWithSuccess(nameof(Login), "Hasta pronto!!", "Hasta pronto!!");
    }
    
}