
using Microsoft.AspNetCore.Mvc;
using project.Models.Interfaces;

public class EmpleadoController : Controller
{
    private readonly IEmpleadoService empleadoService;
    private readonly IPersonaService personaService;
    public EmpleadoController(IPersonaService personaService, IEmpleadoService empleadoService)
    {
        this.empleadoService = empleadoService;
        this.personaService = personaService;
    }
    [HttpGet]
    public IActionResult VistaEmpleados()
    {
        return View("~/Views/Empleados/GestionEmpleados.cshtml");
    }
    [HttpGet]
    public IActionResult VistaEditEmpleados()
    {
        return View("~/Views/Empleados/EditEmpleados.cshtml");
    }
    [HttpGet]
    public IActionResult VistaNewEmpleado()
    {
        return View("~/Views/Empleados/NewEmpleado.cshtml");
    }

}
