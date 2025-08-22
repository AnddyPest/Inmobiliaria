using Microsoft.AspNetCore.Mvc;
using project.Helpers;
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

        [HttpGet("Propietario/listar")]
        public async Task<IActionResult> ObtenerTodos() //Testeado y funcional
        {

            (string?, List<Propietario>?) propietarios = await propietarioService.ObtenerTodos();
            if (propietarios.Item1 != null)
            {
                HelperFor.imprimirMensajeDeError(propietarios.Item1, nameof(PropietarioController), nameof(ObtenerTodos));
                return BadRequest(propietarios.Item1);
            }
            Console.WriteLine(propietarios.Item2);
            return View("~/Views/Propietarios/GestionPropietarios.cshtml", propietarios.Item2);
        }

        [HttpPost("Propietario/Create")]
        public async Task<IActionResult> AgregarPropietario(Persona persona) //testear
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
            (string?, Boolean) propietario = await propietarioService.Alta(persona.IdPersona);
            if (propietario.Item1 != null && !propietario.Item2) return BadRequest(propietario.Item1);

            return RedirectToAction("ObtenerTodos");
        }
        [HttpPost("Propietario/Update")]
        public async Task<IActionResult> ActualizarPropietario(Persona model) //testear
        {
             if (!ModelState.IsValid) return BadRequest(ModelState);
            
            if (model.IdPersona <= 0) return BadRequest("Se requiere el idPersona de la persona");

            var existingPersona = await personaService.GetPersonaById(model.IdPersona, true);
            if (existingPersona.Item1 != null) return BadRequest(existingPersona.Item1);

            (string?, Propietario?) propietarioResult = await propietarioService.getPropietarioByIdPersona(existingPersona.Item2!.IdPersona);
            if (propietarioResult.Item1 != null)
            {
                HelperFor.imprimirMensajeDeError(propietarioResult.Item1, nameof(PropietarioController), nameof(ActualizarPropietario));
                return BadRequest(propietarioResult.Item1);
            }
            model.IdPersona = existingPersona.Item2.IdPersona!;
            int result = await personaService.Editar(model);
            if (result == -1)
            {
                HelperFor.imprimirMensajeDeError("No se actualizo la persona", nameof(PropietarioController), nameof(ActualizarPropietario));
                return BadRequest("No se actualizo la persona");
            }
            (string?, Propietario?) propietarioUpdate = await propietarioService.getPropietarioByIdPersona(existingPersona.Item2.IdPersona!);
            if (propietarioUpdate.Item1 != null)
            {
                HelperFor.imprimirMensajeDeError(propietarioUpdate.Item1, nameof(PropietarioController), nameof(ActualizarPropietario));
                return BadRequest(propietarioUpdate.Item1);
            }

            return RedirectToAction("ObtenerTodos");
        }

        [HttpPost("Propietario/Baja")]
        public async Task<IActionResult> BajaPropietario([FromBody] int idPropietario) //testear
        {

            if (idPropietario <= 0) return BadRequest("Se requiere idPropietario y debe ser mayor a 0");
            (string?, Boolean) codeResult = await propietarioService.BajaLogica(idPropietario);
            if (!codeResult.Item2) return Problem(codeResult.Item1);

            return Ok("El propietario fue dado de baja correctamente");
        }
        [HttpPost("Propietario/Alta")]
        public async Task<IActionResult> AltaPropietario([FromBody] int idPropietario) //testear
        {
            if (idPropietario <= 0) return BadRequest("Se requiere idPropietario y debe ser mayor a 0");
            (string?, Boolean) codeResult = await propietarioService.AltaLogica(idPropietario);
            if (!codeResult.Item2) return Problem(codeResult.Item1);
            return Ok("El propietario fue dado de alta correctamente");
        }
        [HttpGet("Propietario")]
        public IActionResult VistaPropietarios()
        {
            return View("~/Views/Propietarios/IndexPropietarios.cshtml");
        }
        [HttpGet("Propietario/New")]
        public IActionResult NuevoPropietario()
        {
            return View("~/Views/Propietarios/NewPropietario.cshtml");
        }
        [HttpGet("Propietario/Update")]
        public async Task<IActionResult> VistaActualizarPropietario(int id)
        {
            var persona = await personaService.GetPersonaById(id, true);
            if (persona.Item2 == null)
            {
                return NotFound("No se encontró la persona.");
            }
            return View("~/Views/Propietarios/EditPropietarios.cshtml", persona.Item2);
        }
    }
}
