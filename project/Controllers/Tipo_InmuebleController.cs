using Microsoft.AspNetCore.Mvc;
using project.Helpers;
using project.Models;
using project.Models.Interfaces;
using project.Models.ViewModels;

namespace project.Controllers
{
    public class Tipo_InmuebleController : Controller
    {
        ITipo_InmuebleService _tipoInmuebleService;
        public Tipo_InmuebleController(ITipo_InmuebleService tipo_InmuebleService)
        {
            _tipoInmuebleService = tipo_InmuebleService;
        }
        [HttpGet]
        public IActionResult RegistrarTipoInmueble()
        {
            return View("~/Views/Tipo_Inmueble/VistaRegistrarTipoInmueble.cshtml");
        }
        [HttpGet("ActualizarTipoInmueble/{id_tipo_inmueble}")]
        public async Task<IActionResult> ActualizarTipoInmueble(int id_tipo_inmueble)
        {
            if (id_tipo_inmueble <= 0)
            {
                return this.RedirectToActionWithError(nameof(ListarTiposDeInmueble), "EL parametro 'id' debe ser mayor a '0'");
            }
            (string?, Tipo_Inmueble?) resultsFromService = await _tipoInmuebleService.buscarTipoInmueblePorId(id_tipo_inmueble); ;
            if (resultsFromService.Item1 != null)
                return this.RedirectToActionWithError(nameof(ListarTiposDeInmueble), "Error en el Servicio que busca el tipo de inmueble", "Internal Server Error");
            InmuebleViewModel viewModel = new();
            viewModel.tipo_InmuebleOnly = resultsFromService.Item2;
            return View("~/Views/Tipo_Inmueble/ActualizarTipoInmueble.cshtml" , viewModel);
        }
        [HttpGet]
        public async Task<IActionResult> ListarTiposDeInmueble(int nroPagina = 1)
        {
            ViewBag.nroPagina = nroPagina;
            const int registrosPorPagina = 8; 
            (string?, List<Tipo_Inmueble>?) resultsFromService = await _tipoInmuebleService.getAllTipoInmueble(Math.Max(nroPagina,1) ,registrosPorPagina);
            if (resultsFromService.Item1 != null)
                return this.RedirectToActionWithError("Privacy","Home",resultsFromService.Item1);
            (string?, int?) cantidadRegistros = await _tipoInmuebleService.cantidadRegistros();
            if (cantidadRegistros.Item1 != null)
            {
                return this.RedirectToActionWithError("Privacy", "Home", cantidadRegistros.Item1);
            }
            InmuebleViewModel viewModel = new();
            viewModel.tipo_Inmueble = resultsFromService.Item2;
            viewModel.cantidadTotalDePaginas = cantidadRegistros.Item2 % registrosPorPagina == 0 ? cantidadRegistros.Item2 / registrosPorPagina : cantidadRegistros.Item2 / registrosPorPagina + 1; ;
            return View("~/Views/Tipo_Inmueble/VistaListaTipoInmueble.cshtml", viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> RegistrarTipoInmueble(Tipo_Inmueble tipo_Inmueble)
        {
            if (!ModelState.IsValid)
                return this.RedirectToActionWithError(nameof(ListarTiposDeInmueble), ModelStateExtensions.GetErrorMessages(ModelState),"Error");
            (string?, bool) resultsFromService = await _tipoInmuebleService.createTipoInmueble(tipo_Inmueble);
            if(resultsFromService.Item1 != null)
                return this.RedirectToActionWithError(nameof(ListarTiposDeInmueble), "Hubo un error en el servicio que se encarga de registrar el tipo Inmueble", "Internal Server Error");
            return this.RedirectToActionWithSuccess(nameof(ListarTiposDeInmueble), $"{tipo_Inmueble.nombre} registrado con exito");
        }
        [HttpPost("ActualizarTipoInmuebles")]
        public async Task<IActionResult> ActualizarTipoInmuebles(Tipo_Inmueble tipo_Inmueble)
        {
            if (!ModelState.IsValid)
                return this.RedirectToActionWithError(nameof(ListarTiposDeInmueble), ModelStateExtensions.GetErrorMessages(ModelState));

            (string?, bool) resultsFromService = await _tipoInmuebleService.updateTipoInmueble(tipo_Inmueble);
            if (resultsFromService.Item1 != null)
                return this.RedirectToActionWithError(nameof(ListarTiposDeInmueble), resultsFromService.Item1, "Error al intentar actualizar");
            return this.RedirectToActionWithSuccess(nameof(ListarTiposDeInmueble), $"Tipo de Inmueble Actualizado con exito","Registro Actualizado con Exito!!!");

        }
        [HttpGet("EliminarTipoInmueble/{id}")]
        public async Task<IActionResult> EliminarTipoInmueble(int id)
        {
            if(id <= 0 )
                return this.RedirectToActionWithError(nameof(ListarTiposDeInmueble), "EL parametro 'id' enviado deber ser mayor a '0'");
            (string?, bool) resultsFromService = await _tipoInmuebleService.deleteTipoInmueble(id);
            if (resultsFromService.Item1 != null)
                return this.RedirectToActionWithError(nameof(ListarTiposDeInmueble), resultsFromService.Item1);
            
            return this.RedirectToActionWithSuccess(nameof(ListarTiposDeInmueble), $"Tipo de inmueble eliminado con éxito");
        }
    }
}
