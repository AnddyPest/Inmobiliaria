
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using project.Helpers;
using project.Models;
using project.Models.Interfaces;
using project.Models.ViewModels;

public class EmpleadoController : Controller
{
    private readonly IEmpleadoService empleadoService;
    private readonly IPersonaService personaService;
    public EmpleadoController(IPersonaService personaService, IEmpleadoService empleadoService)
    {
        this.empleadoService = empleadoService;
        this.personaService = personaService;
    }
    [HttpGet("/Empleado")]
    public async Task<IActionResult> VistaEmpleados()
    {
        (string?, List<Empleado>?) empleados = await empleadoService.ObtenerTodos();
        EmpleadoViewModel empleadoViewModel = new EmpleadoViewModel();
        empleadoViewModel.Empleados = empleados.Item2 ?? new List<Empleado>();
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
        if (error != null && !confirmacion )
        {
            HelperFor.imprimirMensajeDeError(error, nameof(EmpleadoController), nameof(RegistrarEmpleado));
            return this.RedirectToActionWithError(nameof(VistaNewEmpleado), error);
        }
        return this.RedirectToActionWithSuccess(nameof(VistaEmpleados), "El empleado se ha registrado correctamente", "Empleado Registrado!!!");

    }
    [HttpGet("Empleado/validar")]
    public async Task<IActionResult> ValidarDniRegistro(int dni)
    {
        (string? error, Empleado? empleado)  = await empleadoService.getEmpleadoById(dni);
        if(error!= null)
            HelperFor.imprimirMensajeDeError(error, nameof(EmpleadoController), nameof(ValidarDniRegistro));
        if (empleado != null) return Json(new { empleado = empleado, existe = true });
        return Json(new { empleado = empleado, existe = false });
    }

}
