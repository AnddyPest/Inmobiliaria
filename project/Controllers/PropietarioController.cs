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
        public async Task<IActionResult> AgregarPropietario([FromBody] Persona persona) //testear
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (persona.Dni <= 0) return BadRequest("Se requiere dni y debe ser mayor que 0");
            Persona? personaRegistrada = await personaService.ObtenerPorDni(persona.Dni);

            if (personaRegistrada == null)
            {
                int codeResult = await personaService.Alta(persona);
                if (codeResult == -1) return BadRequest("No se registro a la persona");
                persona.IdPersona = codeResult;
            }
            else
            {
                persona.IdPersona = personaRegistrada.IdPersona;
            }
            (string?,Boolean) propietario = await propietarioService.Alta(persona.IdPersona);
            if (propietario.Item1 != null && !propietario.Item2 ) return BadRequest(propietario.Item1);

            return Ok(propietario.Item1 + persona.ToString());
        }
        [HttpPost("Propietario/Update")]
        public async Task<IActionResult> ActualizarPropietario([FromBody] Persona personaEnviadaDesdeElFront) //testear
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (personaEnviadaDesdeElFront.IdPersona <= 0) return BadRequest("Se requiere idPersona y debe ser mayor que 0");
            (string?, Persona?) personaDesdeDB = await personaService.GetPersonaById(personaEnviadaDesdeElFront.IdPersona,true);
            if (personaDesdeDB.Item1 != null) return BadRequest(personaDesdeDB.Item1);

            
            int codeResult = await personaService.Editar(personaEnviadaDesdeElFront);
            if (codeResult == -1) return Problem("No se actualizo al propietario");

            (string?, Propietario?) propietario = await propietarioService.getPropietarioByIdPersona(personaEnviadaDesdeElFront.IdPersona);
            if (propietario.Item1 != null) return Problem(propietario.Item1);
            
            return Ok(propietario.Item2);
        }

        [HttpPost("Propietario/Baja")]
        public async Task<IActionResult> BajaPropietario([FromBody] int idPropietario) //testear
        {

            if (idPropietario <= 0) return BadRequest("Se requiere idPropietario y debe ser mayor a 0");
            (string?,Boolean) codeResult = await propietarioService.BajaLogica(idPropietario);
            if(!codeResult.Item2) return Problem(codeResult.Item1);
            
            return Ok("El propietario fue dado de baja correctamente");
        }
        [HttpPost("Propietario/Alta")]
        public async Task<IActionResult> AltaPropietario([FromBody] int idPropietario) //testear
        {
            if (idPropietario <= 0) return BadRequest("Se requiere idPropietario y debe ser mayor a 0");
            (string?,Boolean) codeResult = await propietarioService.AltaLogica(idPropietario);
            if (!codeResult.Item2) return Problem(codeResult.Item1);
            return Ok("El propietario fue dado de alta correctamente");
        }
    }
}
