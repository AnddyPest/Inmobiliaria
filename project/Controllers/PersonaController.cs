

using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using project.Models;
using project.Models.Interfaces;

public class PersonaController : Controller
{
    private readonly IPersonaService _personaService;
    public PersonaController(IPersonaService personaService)
    {
        _personaService = personaService;
    }

    [HttpGet("persona/dni")]
    public async Task<IActionResult> getPersonaByDni(int dni)
    {
        System.Console.WriteLine("[PersonaController] getPersonaByDni");
        Persona? persona = await _personaService.ObtenerPorDni(dni);
        if(persona == null)
            return Json(new {persona = persona, existe = false});
        return Json(new {persona = persona, existe = true}); ;
    }

}