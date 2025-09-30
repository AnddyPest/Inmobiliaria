
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using project.Helpers;
using project.Models;
using project.Models.Interfaces;
using project.Models.ViewModels;
[Authorize(Roles = "Administrador")]
public class EmpleadoController : Controller
{
    private readonly IEmpleadoService empleadoService;
    private readonly IPersonaService personaService;
    private readonly IUsuarioService usuarioService;
    private readonly IAuthService authService;
    public EmpleadoController(
        IPersonaService personaService,
        IEmpleadoService empleadoService,
        IUsuarioService usuarioService,
        IAuthService authService
        )
    {
        this.empleadoService = empleadoService;
        this.personaService = personaService;
        this.usuarioService = usuarioService;
        this.authService = authService;
    }
    [HttpGet("/Empleado")]
    public async Task<IActionResult> VistaEmpleados(int nroPagina = 1, int? estado = null, int? dni = null)
    {
        int cantidadRegistrosPorPagina = 5;
        (string?, List<Empleado>?, int?) empleados = await empleadoService.ObtenerTodos(nroPagina, cantidadRegistrosPorPagina, estado, dni);
        EmpleadoViewModel empleadoViewModel = new EmpleadoViewModel();
        empleadoViewModel.Empleados = empleados.Item2 ?? new List<Empleado>();
        ViewBag.nroPagina = nroPagina;
        ViewBag.cantidadTotalDePaginas = empleados.Item3 % cantidadRegistrosPorPagina == 0 ? empleados.Item3 / cantidadRegistrosPorPagina : empleados.Item3 / cantidadRegistrosPorPagina + 1;
        return View("~/Views/Empleados/GestionEmpleados.cshtml", empleadoViewModel);
    }

    [HttpGet("Empleado/new")]
    public IActionResult VistaNewEmpleado()
    {
        return View("~/Views/Empleados/NewEmpleado.cshtml");
    }
    [HttpPost("Empleado/registrar")]
    public async Task<IActionResult> RegistrarEmpleado(Persona persona)
    {
        if (!ModelState.IsValid)
        {
            return this.RedirectToActionWithError(nameof(VistaNewEmpleado), ModelState.GetErrorMessages());
        }
        if (await personaService.ObtenerPorDni(persona.Dni) is Persona personaSearched)
        {
            persona.IdPersona = personaSearched.IdPersona;
        }
        else
        {
            int codeResult = await personaService.Alta(persona);
            if (codeResult == -1)
                return this.RedirectToActionWithError(nameof(VistaNewEmpleado), "No se registro a la persona.");
            if (await personaService.ObtenerPorDni(persona.Dni) is Persona personaSearched2)
            {
                persona.IdPersona = personaSearched2.IdPersona;
            }
            else
            {
                return this.RedirectToActionWithError(nameof(VistaNewEmpleado), "No se pudo obtener la persona creada.");
            }
        }
        (string? error, bool confirmacion) = await empleadoService.CreateEmpleado(persona.IdPersona, null);
        if (error != null && !confirmacion)
        {
            HelperFor.imprimirMensajeDeError(error, nameof(EmpleadoController), nameof(RegistrarEmpleado));
            return this.RedirectToActionWithError(nameof(VistaNewEmpleado), error);
        }
        return this.RedirectToActionWithSuccess(nameof(VistaEmpleados), "El empleado se ha registrado correctamente", "Empleado Registrado!!!");

    }
    [HttpPost("Empleado/actualizar")]
    public async Task<IActionResult> ActualizarEmpleado(Empleado empleado)
    {
        System.Console.WriteLine("Empleado: " + empleado.IdUsuario); 
        bool changeEmailCredential = false;
        if (!ModelState.IsValid)
            return this.RedirectToActionWithError("ObtenerPerfilUsuario","Usuario", "Los datos ingresados no son validos.", new { id = empleado.IdUsuario });

        (string? errorService, Persona? empleadoFromService)  = await personaService.GetPersonaById(empleado.IdPersona, true);
        if (errorService != null && empleadoFromService == null)
        {
            HelperFor.imprimirMensajeDeError(errorService, nameof(EmpleadoController), nameof(ActualizarEmpleado));
            return this.RedirectToActionWithError("ObtenerPerfilUsuario","Usuario", errorService, new { id = empleado.IdUsuario });
        }
        if (empleadoFromService!.Email != empleado.Email)
        {
            changeEmailCredential = true;
            (string? error, bool emailValidoPersona) = await personaService.validarQueElGmailNoEsteDuplicado(empleado.Email, empleado.IdPersona);
            if (error != null && !emailValidoPersona)
            {
                HelperFor.imprimirMensajeDeError(error, nameof(EmpleadoController), nameof(ActualizarEmpleado));
                return this.RedirectToActionWithError("ObtenerPerfilUsuario","Usuario", error, new { id = empleado.IdUsuario });

            }
            (string? errorValidateEmail, bool emailValidoUser) = await usuarioService.ValidarEmailDisponible(new Usuario() { idUsuario = empleado.IdUsuario, email = empleado.Email });
            if (errorValidateEmail != null && !emailValidoUser)
            {
                HelperFor.imprimirMensajeDeError(errorValidateEmail, nameof(EmpleadoController), nameof(ActualizarEmpleado));
                return this.RedirectToActionWithError("ObtenerPerfilUsuario","Usuario", errorValidateEmail, new { id = empleado.IdUsuario });
            }
        }
        empleadoFromService!.Nombre = empleado.Nombre;
        empleadoFromService.Apellido = empleado.Apellido;
        empleadoFromService.Dni = empleado.Dni;
        empleadoFromService.Email = empleado.Email;
        empleadoFromService.Telefono = empleado.Telefono;
        empleadoFromService.Direccion = empleado.Direccion;
        int result = await personaService.Editar(empleadoFromService);
        if (result == -1)
        {
            HelperFor.imprimirMensajeDeError("No se pudo actualizar el empleado", nameof(EmpleadoController), nameof(ActualizarEmpleado));
            return this.RedirectToActionWithError("ObtenerPerfilUsuario","Usuario", "No se pudo actualizar el empleado", new { id = empleado.IdUsuario });
        }
        if (changeEmailCredential)
        {
            (string? error, bool confirmacion) = await usuarioService.CambiarGmail(new Usuario() { idUsuario = empleado.IdUsuario, email = empleado.Email });
            if (error != null && !confirmacion)
            {
                HelperFor.imprimirMensajeDeError(error, nameof(EmpleadoController), nameof(ActualizarEmpleado));
                return this.RedirectToActionWithError("ObtenerPerfilUsuario","Usuario", error, new { id = empleado.IdUsuario });
            }
        }
        
        (string? errorServicesClaim, bool confirmacionServicesClaim) = await authService.ActualizarClaim(new Empleado(empleadoFromService.Nombre, empleadoFromService.Apellido, empleadoFromService.Dni, empleadoFromService.Telefono,empleadoFromService.Direccion,empleadoFromService.Email,empleadoFromService.Estado));
        if (errorServicesClaim != null && !confirmacionServicesClaim)
        {
            HelperFor.imprimirMensajeDeError(errorServicesClaim, nameof(EmpleadoController), nameof(ActualizarEmpleado));
            return this.RedirectToActionWithError("ObtenerPerfilUsuario","Usuario", errorServicesClaim, new { id = empleado.IdUsuario });
        }
        return this.RedirectToActionWithSuccess("ObtenerPerfilUsuario", "Usuario", "El empleado se ha actualizado correctamente", new { id = empleado.IdUsuario }, "Empleado Actualizado!!!");
    }
    [HttpGet("Empleado/Baja")]
    public async Task<IActionResult> BajaEmpleado(int idEmpleado)
    {
        (string? error, bool confirmacion) = await empleadoService.BajaLogica(idEmpleado);
        if (error != null && !confirmacion)
        {
            HelperFor.imprimirMensajeDeError(error, nameof(EmpleadoController), nameof(BajaEmpleado));
            return this.RedirectToActionWithError(nameof(VistaEmpleados), error);
        }
        (string? errorBaja, bool confirmacionBaja) = await usuarioService.BajaLogicaByIdEmpleado(idEmpleado);
        if (errorBaja != null && !confirmacionBaja)
        {
            HelperFor.imprimirMensajeDeError(errorBaja, nameof(EmpleadoController), nameof(BajaEmpleado));
            return this.RedirectToActionWithError(nameof(VistaEmpleados), errorBaja);
        }
        return Ok();
    }
    [HttpGet("Empleado/Alta")]
    public async Task<IActionResult> AltaEmpleado(int idEmpleado)
    {
        (string? error, bool confirmacion) = await empleadoService.AltaLogica(idEmpleado);
        if (error != null && !confirmacion)
        {
            HelperFor.imprimirMensajeDeError(error, nameof(EmpleadoController), nameof(AltaEmpleado));
            return this.RedirectToActionWithError(nameof(VistaEmpleados), error);
        }
        (string? errorAlta, bool confirmacionAlta) = await usuarioService.AltaLogicaByIdEmpleado(idEmpleado);
        if (errorAlta != null && !confirmacionAlta)
        {
            HelperFor.imprimirMensajeDeError(errorAlta, nameof(EmpleadoController), nameof(AltaEmpleado));
            return this.RedirectToActionWithError(nameof(VistaEmpleados), errorAlta);
        }
        return this.RedirectToActionWithSuccess(nameof(VistaEmpleados), "El empleado se ha dado de alta correctamente", "Empleado dado de alta!!!");
    }
    [AllowAnonymous]
    [HttpGet("Empleado/Validar")]
    public async Task<IActionResult> ValidarDni(int dni)
    {
        if (dni < 8)
            return this.RedirectToActionWithError("Login", "Auth", "El dni debe tener al menos 8 digitos.", "Bad request");
        (string?, Empleado?) empleado = await empleadoService.getEmpleadoByDni(dni);
        if (empleado.Item1 != null || empleado.Item2 == null)
            return Json(new{ existe = false});
        System.Console.WriteLine(empleado.Item2);
        return Json(new { empleado = empleado.Item2, existe = true });
    }
}
