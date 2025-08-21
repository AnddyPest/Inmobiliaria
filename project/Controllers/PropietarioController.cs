using Microsoft.AspNetCore.Mvc;
using project.Models;
using project.Models.Interfaces;
using System.Threading.Tasks;

namespace project.Controllers
{
    public class PropietarioController : Controller
    {
        IPropietarioService propietarioService;
        IPersonaService personaService;
        public PropietarioController(IPropietarioService propietarioServ, IPersonaService personaService)
        {
            this.propietarioService = propietarioServ;
            this.personaService = personaService;
        }

        [HttpGet("Propietario/Add")]
        public async Task<IActionResult> AgregarPropietario(Persona persona)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (persona.Dni <= 0) return BadRequest("Se requiere dni y debe ser mayor que 0");
            Persona? personaRegistrada = await personaService.ObtenerPorDni(persona.Dni);
            if(personaRegistrada == null)
            {
                int codeResult = await personaService.Alta(persona);
                if (codeResult == -1) return Problem("No se registro a la persona");
                persona.IdPersona = codeResult;
            }
            
        }
    }
}
