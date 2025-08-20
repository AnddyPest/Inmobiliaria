using Microsoft.AspNetCore.Mvc;
using project.Helpers;
using project.Models;
using project.Models.Interfaces;
using project.Services;

namespace project.Controllers
{
    public class InquilinoController : Controller
    {
        private IInquilinoService inquilinoService;
        private IPersonaService personaService;
        public InquilinoController(IInquilinoService inquilinoService, IPersonaService personaService)
        {
            this.inquilinoService = inquilinoService;
            this.personaService = personaService;

        }
        [HttpGet]
        public async Task<IActionResult> getAllInquilinos() //Testeado y funcional
        {

            (string?, List<Inquilino>) inquilinos = await inquilinoService.GetAllInquilinos();
            if (inquilinos.Item1 != null)
            {
                HelperFor.imprimirMensajeDeError(inquilinos.Item1, nameof(InquilinoController), nameof(getAllInquilinos));
                return BadRequest(inquilinos.Item1);
            }
            Console.WriteLine(inquilinos.Item2);
            return Ok(inquilinos.Item2);
        }
        [HttpGet]
        public async Task<IActionResult> getInquilinoById(int idInquilino) //Testeado y funcional
        {
            (string?, Inquilino?) inquilino = await inquilinoService.GetInquilinoById(idInquilino);
            if (inquilino.Item1 != null)
            {
                HelperFor.imprimirMensajeDeError(inquilino.Item1, nameof(InquilinoController), nameof(getInquilinoById));
                return BadRequest(inquilino.Item1);
            }
            if (inquilino.Item2 == null)
            {
                return NotFound();
            }
            return Ok(inquilino.Item2);
        }
        [HttpPost]
        public async Task<IActionResult> addInquilino([FromBody] int idPersona) //crear persona y testear
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            (string?, bool?) validacion = await inquilinoService.validarQueNoEsteAgregadoElInquilino(idPersona);
            if(validacion.Item1 != null)
            {
                return BadRequest(validacion.Item1);
            }
            (string? ,Persona? ) persona = await personaService.GetPersonaById(idPersona, true);
            if(persona.Item1 != null)
            {
                return BadRequest(persona.Item1);
            }
            (string?, Inquilino?) result = await inquilinoService.AddInquilino(persona.Item2!);
            if (result.Item1 != null)
            {
                HelperFor.imprimirMensajeDeError(result.Item1, nameof(InquilinoController), nameof(addInquilino));
                return BadRequest(result.Item1);
            }
            return Ok(result.Item2);
        }
        [HttpPost]
        public async Task<IActionResult> updateInquilino([FromBody] Persona persona)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            int result = await personaService.Editar(persona);
            if (result == -1)
            {
                HelperFor.imprimirMensajeDeError("No se actualizo la persona", nameof(InquilinoController), nameof(updateInquilino));
                return BadRequest("No se actualizo la persona");
            }
            
            return Ok("Inquilino actualizado con exito");
        }
        [HttpPost]
        public async Task<IActionResult> logicalDeleteInquilino(int idInquilino)
        {
            
            (string?, bool?) result = await inquilinoService.LogicalDeleteInquilino(idInquilino);
            if (result.Item1 != null)
            {
                HelperFor.imprimirMensajeDeError(result.Item1, nameof(InquilinoController), nameof(logicalDeleteInquilino));
                return BadRequest(result.Item1);
            }
            if (result.Item2 == false)
            {
                return NotFound();
            }
            return Ok("El inquilino ha sido eliminado lógicamente.");
        }
    }
}
