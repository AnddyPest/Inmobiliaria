using Microsoft.AspNetCore.Mvc;
using project.Models;
using project.Models.Interfaces;
using project.Helpers;
using project.Models.ViewModels;
using Microsoft.Extensions.Configuration;

namespace project.Controllers
{
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
                return this.RedirectToActionWithError("GetAllContratos","Contratos",pagoViewModel.IdContrato,"Error al crear pago");
            }

            
            var fechaConfeccion = pagoViewModel.FechaConfeccion == default(DateTime)
                ? DateTime.Today
                : pagoViewModel.FechaConfeccion;

            var nuevoPago = new Pago
            {
                Detalle = pagoViewModel.Detalle ?? string.Empty,
                Importe = pagoViewModel.Importe,
                FechaConfeccion = DateOnly.FromDateTime(pagoViewModel.FechaConfeccion),
                IdContrato = pagoViewModel.IdContrato,
                Abonado = false,
                Alquiler= true,
                Estado = true
            };

            var (mensaje, ok) = await _pagosService.CreatePago(nuevoPago);
            if (!ok)
            {
                return this.RedirectToActionWithError("listar", "Contrato", "No se pudo generar el pago", "Error al crear pago");
            }

            return this.RedirectToActionWithSuccess("listar", "Contrato", "Pago registrado exitosamente", "Pago creado!!");
        }
    }
}