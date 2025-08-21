using Microsoft.AspNetCore.Mvc;

using project.Models;
using project.Models.Interfaces;
using System.Threading.Tasks;

namespace project.Controllers
{
    public class PropietarioController : Controller
    {
        private IPropietarioService propietarioService;
        private IPersonaService personaService;
        public PropietarioController(IPropietarioService propietarioServ, IPersonaService personaService)
        {
            this.propietarioService = propietarioServ;
            this.personaService = personaService;
        }

        [HttpPost("Propietario/Add")]
        public async Task<IActionResult> AgregarPropietario([FromBody] Persona persona) //testar
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (persona.Dni <= 0) return BadRequest("Se requiere dni y debe ser mayor que 0");
            Persona? personaRegistrada = await personaService.ObtenerPorDni(persona.Dni);

            if (personaRegistrada == null)
            {
                int codeResult = await personaService.Alta(persona);
                if (codeResult == -1) return Problem("No se registro a la persona");
                persona.IdPersona = codeResult;
            }
            else
            {
                persona.IdPersona = personaRegistrada.IdPersona;
            }
            var propietario = await propietarioService.Alta(persona.IdPersona);
            if (propietario == -1)
            {
                return Problem("No se creo el propietario");
            }

            return Ok(persona);
        }
        [HttpPost("Propietario/Update")]
        public async Task<IActionResult> ActualizarPropietario([FromBody] Persona personaEnviadaDesdeElFront) //testear
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (personaEnviadaDesdeElFront.Dni <= 0) return BadRequest("Se requiere dni y debe ser mayor que 0");
            Persona? personaDesdeDB = await personaService.ObtenerPorDni(personaEnviadaDesdeElFront.Dni);

            if (personaDesdeDB == null) return NotFound("No se encuentra registrada la persona");

            personaEnviadaDesdeElFront.IdPersona = personaDesdeDB.IdPersona;
            int codeResult = await personaService.Editar(personaEnviadaDesdeElFront);
            if (codeResult == -1) return Problem("No se actualizo al propietario");

            (string?, Propietario?) propietario = await propietarioService.getPropietarioByIdPersona(personaEnviadaDesdeElFront.IdPersona);
            if (propietario.Item1 != null)
            {
                return Problem(propietario.Item1);
            }
            return Ok(propietario.Item2);
        }

        [HttpPost("Propietario/Baja")]
        public async Task<IActionResult> BajaPropietario([FromBody] int idPropietario) //testear
        {

            if (idPropietario <= 0) return BadRequest("Se requiere idPropietario y debe ser mayor a 0");
            int codeResult = await propietarioService.BajaLogica(idPropietario);
            if(codeResult == -1) return Problem("No se pudo dar de baja al propietario");
            
            return Ok("El propietario fue dado de baja correctamente");
        }
    }
}
