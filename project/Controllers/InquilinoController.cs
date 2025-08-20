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
        public async Task<IActionResult> GetAllInquilinos() //Testeado y funcional
        {

            (string?, List<Inquilino>) inquilinos = await inquilinoService.GetAllInquilinos();
            if (inquilinos.Item1 != null)
            {
                HelperFor.imprimirMensajeDeError(inquilinos.Item1, nameof(InquilinoController), nameof(GetAllInquilinos));
                return BadRequest(inquilinos.Item1);
            }
            Console.WriteLine(inquilinos.Item2);
            return Ok(inquilinos.Item2);
        }
        [HttpGet]
        public async Task<IActionResult> GetInquilinoById(int idInquilino) //Testeado y funcional
        {
            (string?, Inquilino?) inquilino = await inquilinoService.GetInquilinoById(idInquilino);
            if (inquilino.Item1 != null)
            {
                HelperFor.imprimirMensajeDeError(inquilino.Item1, nameof(InquilinoController), nameof(GetInquilinoById));
                return BadRequest(inquilino.Item1);
            }
            if (inquilino.Item2 == null)
            {
                return NotFound();
            }
            return Ok(inquilino.Item2);
        }
        [HttpPost("Inquilinos/Create")]
        public async Task<IActionResult> AddInquilino([FromBody] int idPersona) //crear persona y testear
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            (string?, bool?) validacion = await inquilinoService.validarQueNoEsteAgregadoElInquilino(idPersona);
            if (validacion.Item1 != null)
            {
                return BadRequest(validacion.Item1);
            }
            (string?, Persona?) persona = await personaService.GetPersonaById(idPersona, true);
            if (persona.Item1 != null)
            {
                return BadRequest(persona.Item1);
            }
            (string?, Inquilino?) result = await inquilinoService.AddInquilino(persona.Item2!);
            if (result.Item1 != null)
            {
                HelperFor.imprimirMensajeDeError(result.Item1, nameof(InquilinoController), nameof(AddInquilino));
                return BadRequest(result.Item1);
            }
            return Ok(result.Item2);
        }
        [HttpPost("Inquilinos/Update")]
        public async Task<IActionResult> UpdateInquilino([FromBody] Persona persona)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            int result = await personaService.Editar(persona);
            if (result == -1)
            {
                HelperFor.imprimirMensajeDeError("No se actualizo la persona", nameof(InquilinoController), nameof(UpdateInquilino));
                return BadRequest("No se actualizo la persona");
            }

            return Ok("Inquilino actualizado con exito");
        }
        [HttpPost]
        public async Task<IActionResult> LogicalDeleteInquilino(int idInquilino)
        {

            (string?, bool?) result = await inquilinoService.LogicalDeleteInquilino(idInquilino);
            if (result.Item1 != null)
            {
                HelperFor.imprimirMensajeDeError(result.Item1, nameof(InquilinoController), nameof(LogicalDeleteInquilino));
                return BadRequest(result.Item1);
            }
            if (result.Item2 == false)
            {
                return NotFound();
            }
            return Ok("El inquilino ha sido eliminado lógicamente.");
        }
        //Vistas
        [HttpGet("Inquilinos")]
        public IActionResult VistaInquilinos()
        {
            return View("~/Views/Inquilinos/IndexInquilinos.cshtml");
        }

        //New
        [HttpGet("Inquilinos/New")]
        public IActionResult VistaNuevoInquilino()
        { 
            return View("~/Views/Inquilinos/NewInquilino.cshtml");
        }
    }
}
