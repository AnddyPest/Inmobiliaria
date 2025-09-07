using Microsoft.AspNetCore.Mvc;
using project.Models;
using project.Models.Interfaces;

namespace project.Controllers
{
    public class ContratoController : Controller
    {
        private IContratoService _contratoService;

        public ContratoController(IContratoService contratoService)
        {
            _contratoService = contratoService;
        }

        [HttpGet("contrato/listar")]
        public async Task<IActionResult> GetAllContratos()
        {
            (string?, List<Contrato>?) contratos = await _contratoService.GetAllContratos();
            if (contratos.Item1 != null)
            {
                return BadRequest(contratos.Item1);
            }
            return Ok(contratos.Item2);
        }
        [HttpGet("contrato/find/{idContrato}")]
        public async Task<IActionResult> GetContratoById(int idContrato)
        {
            (string?, Contrato?) contrato = await _contratoService.GetContratoById(idContrato);
            if (contrato.Item1 != null)
            {
                return BadRequest(contrato.Item1);
            }
            if (contrato.Item2 == null)
            {
                return NotFound();
            }
            return Ok(contrato.Item2);
        }
        [HttpPost("contrato/crear")]
        public async Task<IActionResult> AddContrato(Contrato model)
        {
            Console.WriteLine($"[CONTRATO] IdInquilino recibido: {model.IdInquilino}");
            Console.WriteLine($"[CONTRATO] IdPropietario recibido: {model.IdPropietario}");
            Console.WriteLine($"[CONTRATO] IdInmueble recibido: {model.IdInmueble}");
            Console.WriteLine($"[CONTRATO] Monto recibido: {model.Monto}");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            (string?, bool) contratoCreated = await _contratoService.CreateContrato(model);
            if (contratoCreated.Item1 != null)
                return BadRequest(contratoCreated.Item1);
            if (!contratoCreated.Item2)
                return BadRequest("No se pudo crear el contrato.");
            return Ok(contratoCreated.Item2);
        }
        [HttpPost("contrato/actualizar")]
        public async Task<IActionResult> UpdateContrato(Contrato model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            (string?, bool) contratoUpdated = await _contratoService.UpdateContrato(model);
            if (contratoUpdated.Item1 != null)
                return BadRequest(contratoUpdated.Item1);
            if (!contratoUpdated.Item2)
                return BadRequest("No se pudo actualizar el contrato.");
            return Ok(true);
        }
        [HttpGet("contrato/darDeBaja/{idContrato}")]
        public async Task<IActionResult> DarDeBajaContrato(int idContrato)
        {
            (string?, bool) contratoDeleted = await _contratoService.DarBajaContrato(idContrato);
            if (contratoDeleted.Item1 != null)
                return BadRequest(contratoDeleted.Item1);
            if (!contratoDeleted.Item2)
                return BadRequest("No se pudo dar de baja el contrato.");
            return Ok(true);
        }
        [HttpGet("contrato/activar/{idContrato}")]
        public async Task<IActionResult> ActivarContrato(int idContrato)
        {
            (string?, bool) contratoActivated = await _contratoService.DarAltaContrato(idContrato);
            if (contratoActivated.Item1 != null)
                return BadRequest(contratoActivated.Item1);
            if (!contratoActivated.Item2)
                return BadRequest("No se pudo activar el contrato.");
            return Ok(true);
        }

        //VISTAS
        [HttpGet("contrato")]
        public IActionResult VistaContratos()
        {
            return View("~/Views/Contratos/IndexContratos.cshtml");
        }
        [HttpGet("contrato/new")]
        public IActionResult VistaNuevoContrato()
        {
            return View("~/Views/Contratos/NewContrato.cshtml");
        }

    }
}
