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
        public async Task<IActionResult> AddInquilino(Persona model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 1. Buscar persona por DNI (Comprueba q la persona no exista para no crearla al pedo)
            var personaExistente = await personaService.ObtenerPorDni(model.Dni);
            int idPersona;

            if (personaExistente != null)
            {
                // Si existe, usa su id
                idPersona = personaExistente.IdPersona;
            }
            else
            {
                // Si no existe, la crea
                int altaRes = await personaService.Alta(model);
                if (altaRes <= 0)
                    return BadRequest("No se pudo crear la persona.");

                // Una vez creada, la buscamos y usamos su id
                var nuevaPersona = await personaService.ObtenerPorDni(model.Dni);
                if (nuevaPersona == null)
                    return BadRequest("No se pudo obtener la persona creada.");
                idPersona = nuevaPersona.IdPersona;
            }

            // 2. Validamos q ya no sea inquilino para no agregarlo al pedo
            (string? validacion, bool puedeAgregar) = await inquilinoService.validarQueNoEsteAgregadoElInquilino(idPersona);
            if (!puedeAgregar)
                return BadRequest(validacion);

            // 3. si no es inquilino, si no lo es, lo agregamos.
            model.IdPersona = idPersona;
            (string? errorInq, Inquilino? inquilino) = await inquilinoService.AddInquilino(model);
            if (errorInq != null)
                return BadRequest(errorInq);

            return RedirectToAction("Inquilinos");
        }
        [HttpPost("Inquilinos/Update")]
        public async Task<IActionResult> UpdateInquilino([FromBody] Persona persona)
        {

            if (!ModelState.IsValid) return BadRequest(ModelState);
            Console.WriteLine(persona.Dni);
            if (persona.Dni <= 0) return BadRequest("Se requiere el Dni de la persona");
            
            var existingPersona = await personaService.ObtenerPorDni(persona.Dni);
            if (existingPersona == null) return BadRequest("No se encuentra registrada la persona");
            
            (string?,Inquilino?) inquilinoResult = await inquilinoService.getInquilinoByIdPersona(existingPersona.IdPersona);
            if(inquilinoResult.Item1 != null)
            {
                HelperFor.imprimirMensajeDeError(inquilinoResult.Item1, nameof(InquilinoController), nameof(UpdateInquilino));
                return BadRequest(inquilinoResult.Item1);
            }
            persona.IdPersona = existingPersona.IdPersona;
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
