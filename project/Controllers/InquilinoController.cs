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
        [HttpGet("inquilino/listar")]
        public async Task<IActionResult> GetAllInquilinos() //Testeado y funcional
        {

            (string?, List<Inquilino>) inquilinos = await inquilinoService.GetAllInquilinos();
            if (inquilinos.Item1 != null)
            {
                HelperFor.imprimirMensajeDeError(inquilinos.Item1, nameof(InquilinoController), nameof(GetAllInquilinos));
                return BadRequest(inquilinos.Item1);
            }
            Console.WriteLine(inquilinos.Item2);
            return View("~/Views/Inquilinos/GestionInquilinos.cshtml", inquilinos.Item2);
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
        [HttpPost("inquilino/Create")]
        public async Task<IActionResult> AddInquilino(Persona model) //Testeado y funcional
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

            return RedirectToAction("");
        }
        [HttpPost("inquilino/Update")]
        public async Task<IActionResult> UpdateInquilino(Persona model)//Testeado y funcional
        {

            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            if (model.IdPersona <= 0) return BadRequest("Se requiere el idPersona de la persona");

            var existingPersona = await personaService.GetPersonaById(model.IdPersona, true);
            if (existingPersona.Item1 != null) return BadRequest(existingPersona.Item1);
            
            (string?,Inquilino?) inquilinoResult = await inquilinoService.getInquilinoByIdPersona(existingPersona.Item2!.IdPersona);
            if(inquilinoResult.Item1 != null)
            {
                HelperFor.imprimirMensajeDeError(inquilinoResult.Item1, nameof(InquilinoController), nameof(UpdateInquilino));
                return BadRequest(inquilinoResult.Item1);
            }
            model.IdPersona = existingPersona.Item2.IdPersona!;
            int result = await personaService.Editar(model);
            if (result == -1)
            {
                HelperFor.imprimirMensajeDeError("No se actualizo la persona", nameof(InquilinoController), nameof(UpdateInquilino));
                return BadRequest("No se actualizo la persona");
            }
            (string?, Inquilino?) inquilinoUpdate = await inquilinoService.getInquilinoByIdPersona(existingPersona.Item2.IdPersona!);
            if (inquilinoResult.Item1 != null)
            {
                HelperFor.imprimirMensajeDeError(inquilinoResult.Item1, nameof(InquilinoController), nameof(UpdateInquilino));
                return BadRequest(inquilinoResult.Item1);
            }

            return RedirectToAction("GetAllInquilinos");
        }
        [HttpPost("inquilino/Baja")]
        public async Task<IActionResult> LogicalDeleteInquilino([FromBody] int idInquilino) //Testeado y funcional
        {
            if (idInquilino <= 0) return BadRequest("Error Message: Se requiere idInquilino y debe ser mayor a 0");
            
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
            return Ok("El inquilino ha sido dado de baja lógicamente.");
        }
        [HttpPost("inquilino/Alta")]
        public async Task<IActionResult> AltaLogicaInquilino([FromBody] int idInquilino) //Testeado y funcional
        {
            if (idInquilino <= 0) return BadRequest("Error Message: Se requiere idInquilino y debe ser mayor a 0");

            (string?, bool?) result = await inquilinoService.AltaLogicaInquilino(idInquilino);
            if (result.Item1 != null)
            {
                HelperFor.imprimirMensajeDeError(result.Item1, nameof(InquilinoController), nameof(AltaLogicaInquilino));
                return BadRequest(result.Item1);
            }
            if (result.Item2 == false)
            {
                return NotFound();
            }
            return Ok("El inquilino ha sido dado de alta lógicamente.");
        }
        //Vistas
        [HttpGet("Inquilino")]
        public IActionResult VistaInquilinos()
        {
            return View("~/Views/Inquilinos/IndexInquilinos.cshtml");
        }

        //New
        [HttpGet("inquilino/New")]
        public IActionResult VistaNuevoInquilino()
        { 
            return View("~/Views/Inquilinos/NewInquilino.cshtml");
        }
        [HttpGet("inquilino/Update")]
        public async Task<IActionResult> VistaActualizarInquilino(int id)
        {
            var persona = await personaService.GetPersonaById(id, true);
            if (persona.Item2 == null)
            {
                return NotFound("No se encontró la persona.");
            }
            return View("~/Views/Inquilinos/EditInquilinos.cshtml", persona.Item2);
        }
        
    }
}
