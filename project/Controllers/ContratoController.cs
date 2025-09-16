using Microsoft.AspNetCore.Mvc;
using project.Models;
using project.Models.Interfaces;
using project.Helpers;
using project.Models.ViewModels;
namespace project.Controllers
{
    public class ContratoController : Controller
    {
        private IContratoService _contratoService;
        private IInmuebleService _inmuebleService;
        private IInquilinoService _inquilinoService;
        private IPropietarioService _propietarioService;

        // Elimina el constructor duplicado y deja solo el que recibe ambos servicios
        public ContratoController(IContratoService contratoService, IInmuebleService inmuebleService, IInquilinoService inquilinoService, IPropietarioService propietarioService)
        {
            _contratoService = contratoService;
            _inmuebleService = inmuebleService;
            _inquilinoService = inquilinoService;
            _propietarioService = propietarioService;
        }

        [HttpGet("contrato/listar")]
        public async Task<IActionResult> GetAllContratos(int nroPagina = 1, string? disponibilidad = null, int ? fechaCompare = null)
            // Depuración: mostrar parámetros recibidos desde el frontend
        {
            Console.WriteLine($"[DEBUG] disponibilidad: {disponibilidad}");
            Console.WriteLine($"[DEBUG] fechaCompare: {fechaCompare}");
            ContratoViewModel viewModel = new();
            ViewBag.nroPagina = nroPagina;
            const int registrosPorPagina = 5;
            ViewBag.registrosPorPagina = registrosPorPagina;
            (string?, List<Contrato>?) contratosResult = await _contratoService.GetAllContratos(nroPagina, registrosPorPagina, disponibilidad, fechaCompare);
            if (contratosResult.Item2 == null)
            {
                HelperFor.imprimirMensajeDeError(contratosResult.Item1 ?? "Error desconocido", nameof(ContratoController), nameof(GetAllContratos));
                return this.RedirectToActionWithError(nameof(Index), contratosResult.Item1 ?? "Error desconocido");
            }
            viewModel.contratos = contratosResult.Item2;
            int totalContratos = 0;
            int.TryParse(contratosResult.Item1, out totalContratos);
            viewModel.cantidadTotalDePaginas = totalContratos % registrosPorPagina == 0
                ? totalContratos / registrosPorPagina
                : totalContratos / registrosPorPagina + 1;
            // Poblar inquilinos, propietarios e inmuebles para la vista
            (string?, List<Inquilino>?) inquilinosResult = await _inquilinoService.GetAllInquilinos();
            if (inquilinosResult.Item1 == null)
                viewModel.inquilinos = inquilinosResult.Item2;
            (string?, List<Propietario>?) propietariosResult = await _propietarioService.ObtenerTodos();
            if (propietariosResult.Item1 == null)
                viewModel.propietarios = propietariosResult.Item2;
            (string?, List<Inmueble>?) inmueblesResult = await _inmuebleService.ObtenerTodosLosInmueblesAPI();
            if (inmueblesResult.Item1 == null)
                viewModel.inmueble = inmueblesResult.Item2;

            return View("~/Views/Contratos/GestionContratos.cshtml", viewModel);
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
            
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            (string?, bool) contratoCreated = await _contratoService.CreateContrato(model);
            if (contratoCreated.Item1 != null)
                return this.RedirectToActionWithError("GetAllInmuebles","Inmueble",contratoCreated.Item1,"Error al crear contrato");
            if (!contratoCreated.Item2)
                return this.RedirectToActionWithError("GetAllInmuebles", "Inmueble", "No se pudo registrar el contrato", "Error al crear contrato");
            // Marcar inmueble como alquilado en el backend
            (string?, bool) alquiladoResult = await _inmuebleService.MarcarAlquilado(model.IdInmueble);
            if (alquiladoResult.Item1 != null || !alquiladoResult.Item2)
                Console.WriteLine($"[CONTRATO] Error al marcar inmueble como alquilado: {alquiladoResult.Item1}");
            return this.RedirectToActionWithSuccess("GetAllInmuebles", "Inmueble","Contrato registrado exitosamente","Contrato creado!!");
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
            (string?, Contrato?) contrato = await _contratoService.GetContratoById(idContrato);
            if (contrato.Item1 != null || contrato.Item2 == null)
                return BadRequest(contrato.Item1 ?? "No se encontró el contrato");

            (string?, bool) contratoDeleted = await _contratoService.DarBajaContrato(idContrato);
            if (contratoDeleted.Item1 != null)
                return BadRequest(contratoDeleted.Item1);
            if (!contratoDeleted.Item2)
                return BadRequest("No se pudo dar de baja el contrato.");

            // Marcar inmueble como disponible
            int idInmueble = contrato.Item2.IdInmueble;
            (string?, bool) disponibleResult = await _inmuebleService.MarcarLibre(idInmueble);
            if (disponibleResult.Item1 != null || !disponibleResult.Item2)
                return this.RedirectToActionWithError("GetAllInmuebles", "Inmueble", "Contrato dado de baja, pero no se pudo marcar el inmueble como disponible: " + (disponibleResult.Item1 ?? "Error"));
            
            return this.RedirectToActionWithSuccess("GetAllInmuebles", "Inmueble","Contrato anulado exitosamente","Contrato anulado!!");
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
        [HttpGet("contrato/getContratoByIdInmueble/{idInmueble}")]
        public async Task<IActionResult> GetContratoByIdInmueble(int idInmueble)
        {
            (string?, List<Contrato>?) contratosResult = await _contratoService.GetContratoByIdInmueble(idInmueble);
            if (contratosResult.Item1 != null)
            {
                return BadRequest(contratosResult.Item1);
            }
            if (contratosResult.Item2 == null || contratosResult.Item2.Count == 0)
            {
                return NotFound();
            }
            return Ok(contratosResult.Item2);
        }
        [HttpPost("contrato/renovarContrato")]
        public async Task<IActionResult> RenovarContrato( Contrato request)
        {
            if (request == null || !ModelState.IsValid)
                return this.RedirectToActionWithError("Error en el servicio de contratos", ModelState.GetErrorMessages());
            (string?, bool) resultado = await _contratoService.RenovarContrato(
                request.IdContrato,
                request.FechaInicio,
                request.FechaFin,
                request.Monto
            );

            if (resultado.Item1 != null)
                return this.RedirectToActionWithError("GetAllContratos", "Contrato", "Error en el servicio de contratos", "No se pudo renovar el contrato.");
            if (!resultado.Item2)
                return this.RedirectToActionWithError("GetAllContratos", "Contrato", "Error al renovar el contrato", "No se pudo renovar el contrato.");

            return this.RedirectToActionWithSuccess("GetAllContratos", "Contrato", "Contrato Renovado exitosamente", $"Contrato renovado. Periodo: {request.FechaInicio.ToString("dd/MM/yyyy")} - {request.FechaFin.ToString("dd/MM/yyyy") } - Monto: ${request.Monto}");

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
