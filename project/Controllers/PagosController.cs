using Microsoft.AspNetCore.Mvc;
using project.Models;
using project.Models.Interfaces;
using project.Helpers;
using project.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace project.Controllers
{
    [Authorize]
    public class PagosController : Controller
    {
        private IContratoService _contratoService;
        private IInmuebleService _inmuebleService;
        private IInquilinoService _inquilinoService;
        private IPropietarioService _propietarioService;
        private IPagosService _pagosService;

        // Elimina el constructor duplicado y deja solo el que recibe ambos servicios
        public PagosController(IContratoService contratoService, IInmuebleService inmuebleService, IInquilinoService inquilinoService, IPropietarioService propietarioService, IPagosService pagosService)
        {
            _contratoService = contratoService;
            _inmuebleService = inmuebleService;
            _inquilinoService = inquilinoService;
            _propietarioService = propietarioService;
            _pagosService = pagosService;
        }

        [HttpPost("pago/create")]
        public async Task<IActionResult> CreatePago([FromForm] PagoViewModel pagoViewModel)
        {
            if (!ModelState.IsValid)
            {
                return this.RedirectToActionWithError("GetAllContratos", "Contratos", pagoViewModel.IdContrato, "Error al crear pago");
            }


            var nuevoPago = new Pago
            {
                Detalle = pagoViewModel.Detalle ?? string.Empty,
                Importe = pagoViewModel.Importe,
                FechaConfeccion = DateOnly.FromDateTime(pagoViewModel.FechaConfeccion),
                IdContrato = pagoViewModel.IdContrato,
                Abonado = false,
                Alquiler = true,
                Estado = true
            };

            var (mensaje, ok) = await _pagosService.CreatePago(nuevoPago);
            if (!ok)
            {
                return this.RedirectToActionWithError("listar", "Contrato", "Ya hay un pago generado para este mes", "Error al crear pago");
            }

            return this.RedirectToActionWithSuccess("listar", "Contrato", "Pago registrado exitosamente", "Pago creado!!");
        }
        [HttpGet("pago/listar/{idContrato}")]
        public async Task<IActionResult> GetPagosByIdContrato(int idContrato, int? nroPagina = 1)
        {
            int registrosPorPagina = 10;
            int pagina = nroPagina ?? 1;
            (string?, List<Pago>?) pagosResult = await _pagosService.GetPagosByIdContrato(pagina, registrosPorPagina, idContrato);
            if (pagosResult.Item1 != null)
            {
                HelperFor.imprimirMensajeDeError(pagosResult.Item1 ?? "Error desconocido", nameof(PagosController), nameof(GetPagosByIdContrato));
                return this.RedirectToActionWithError("listar", "Contrato", "No hay pagos registrados para este contrato", "Error desconocido");
            }

            var viewModel = new PagoViewModel
            {
                IdContrato = idContrato,
                Pagos = pagosResult.Item2 ?? new List<Pago>()
            };

            ViewBag.nroPagina = pagina;
            ViewBag.registrosPorPagina = registrosPorPagina;
            ViewBag.totalPagos = viewModel.Pagos.Count;

            return View("~/Views/Pagos/GestionPagos.cshtml", viewModel);
        }
        [HttpGet("AsentarPago/{idPago}")]
        public async Task<IActionResult> AsentarPago(int idPago)
        {
            var (mensaje, ok) = await _pagosService.AsentarPago(new Pago { IdPago = idPago });
            if (!ok)
            {
                return this.RedirectToActionWithError("listar", "Pagos", idPago, "Error al asentar pago");
            }

            // Obtener el idContrato del pago asentado
            var pagoResult = await _pagosService.GetPagoById(idPago);
            int idContrato = pagoResult.Item2?.IdContrato ?? 0;

            return this.RedirectToActionWithSuccess(


                "GetPagosByIdContrato",
                "Pagos",
                "Pago asentado exitosamente",
                new { idContrato = idContrato },
                "Pago Asentado!!");

        }
        [HttpGet("AnularPago/{idPago}")]
        public async Task<IActionResult> AnularPago(int idPago)
        {
            var (mensaje, ok) = await _pagosService.AnularPago(new Pago { IdPago = idPago });
            if (!ok)
            {
                return this.RedirectToActionWithError("listar", "Pagos", idPago, "Error al anular pago");
            }

            // Obtener el idContrato del pago anulado
            var pagoResult = await _pagosService.GetPagoById(idPago);
            int idContrato = pagoResult.Item2?.IdContrato ?? 0;

            return this.RedirectToActionWithSuccess(
                "GetPagosByIdContrato",
                "Pagos",
                "Pago anulado exitosamente",
                new { idContrato = idContrato },
                "Pago Anulado!!");
        }
        [HttpGet("DarDeBaja/{idPago}/{idContrato}")]
        public async Task<IActionResult> DarDeBaja(int idPago, int idContrato)
        {
            (string? error, bool confirmacion) = await _pagosService.darDeBajaLogicaPago(idPago);
            if (!confirmacion && error != null)
            {
                return this.RedirectToActionWithError("GetPagosByIdContrato", "Pagos", idContrato, error);
            }

            return this.RedirectToActionWithSuccess("GetPagosByIdContrato", "Pagos", "Pago dado de baja exitosamente", new { idContrato = idContrato }, "Pago dado de baja!!");
        }
        [HttpGet("DarDeAlta/{idPago}/{idContrato}")]
        public async Task<IActionResult> DarDeAlta(int idPago, int idContrato)
        {
            (string? error, bool confirmacion) = await _pagosService.darAltaLogicaPago(idPago);
            if (!confirmacion && error != null)
            {
                return this.RedirectToActionWithError("GetPagosByIdContrato", "Pagos", error, new { idContrato = idContrato });
            }

            return this.RedirectToActionWithSuccess("GetPagosByIdContrato", "Pagos", "Pago dado de alta exitosamente", new { idContrato = idContrato }, "Pago dado de alta!!");
        }
        [HttpPost("ActualizarDetalle")]
        public async Task<IActionResult> ActualizarDetalle(int idPago, string detalle, int idContrato)
        {
            (string? error, bool confirmacion) = await _pagosService.UpdatePago(idPago, detalle);
            if (!confirmacion && error != null)
            {
                return this.RedirectToActionWithError("GetPagosByIdContrato", "Pagos", error, new { idContrato = idContrato });
            }

            return this.RedirectToActionWithSuccess("GetPagosByIdContrato", "Pagos", "Pago actualizado exitosamente", new { idContrato = idContrato }, "Pago actualizado!!");
        }
    }
}