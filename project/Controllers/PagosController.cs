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

            // Parsear la fecha en el controlador para aceptar dd/MM/yyyy y yyyy-MM-dd
            DateTime fechaConfeccion = pagoViewModel.FechaConfeccion;
            if (fechaConfeccion == default(DateTime))
            {
                fechaConfeccion = DateTime.Today;
            }
            else
            {
                // Si la fecha viene en formato dd/MM/yyyy como string, intentar parsear
                var fechaStr = Request.Form["FechaConfeccion"].ToString();
                if (!string.IsNullOrEmpty(fechaStr))
                {
                    if (!DateTime.TryParseExact(fechaStr, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out fechaConfeccion))
                    {
                        DateTime tempFecha;
                        if (DateTime.TryParseExact(fechaStr, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out tempFecha))
                        {
                            fechaConfeccion = tempFecha;
                        }
                    }
                }
            }

            // Log para depuración del valor final de fechaConfeccion
            Console.WriteLine($"[DEBUG] fechaConfeccion final: {fechaConfeccion:yyyy-MM-dd}");

            var nuevoPago = new Pago
            {
                Detalle = pagoViewModel.Detalle ?? string.Empty,
                Importe = pagoViewModel.Importe,
                FechaConfeccion = new DateOnly(fechaConfeccion.Year, fechaConfeccion.Month, fechaConfeccion.Day),
                IdContrato = pagoViewModel.IdContrato,
                Abonado = false,
                Alquiler= true,
                Estado = true
            };
            // validar que no haya un pago para el mismo mes y contrato con boolean alquiler = true
            var existePago = await _pagosService.ExistePagoAlquiler(
                nuevoPago.IdContrato,
                nuevoPago.FechaConfeccion);
            if (existePago)
            {
                return this.RedirectToActionWithError("listar", "Contrato", "No se registro el pago", "Ya existe un pago de alquiler para este mes");
            }
            else
            {
                await _pagosService.CreatePago(nuevoPago);
            }

            return this.RedirectToActionWithSuccess("listar", "Contrato", "Pago registrado exitosamente", "Pago creado!!");
        }
    }
}