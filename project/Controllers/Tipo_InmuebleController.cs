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
        public IActionResult ActualizarTipoInmueble()
        {
            return View("~/Views/Tipo_Inmueble/ActualizarTipoInmueble.cshtml");
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
                return this.RedirectToActionWithError(nameof(ListarTiposDeInmueble), ModelStateExtensions.GetErrorMessages(ModelState));
            (string?, bool) resultsFromService = await _tipoInmuebleService.createTipoInmueble(tipo_Inmueble);
            if(resultsFromService.Item1 != null)
                return this.RedirectToActionWithError(nameof(ListarTiposDeInmueble), resultsFromService.Item1);
            return this.RedirectToActionWithSuccess(nameof(ListarTiposDeInmueble), $"Tipo_Inmueble: {tipo_Inmueble.nombre}\nRegistrado con exito");
        }
        [HttpPost]
        public async Task<IActionResult> ActualizarTipoInmueble(Tipo_Inmueble tipo_Inmueble)
        {
            if (!ModelState.IsValid)
                return this.RedirectToActionWithError(nameof(ListarTiposDeInmueble), ModelStateExtensions.GetErrorMessages(ModelState));

            (string?, bool) resultsFromService = await _tipoInmuebleService.updateTipoInmueble(tipo_Inmueble);
            if (resultsFromService.Item1 != null)
                return this.RedirectToActionWithError(nameof(ListarTiposDeInmueble), ModelStateExtensions.GetErrorMessages(ModelState));
            return this.RedirectToActionWithSuccess(nameof(ListarTiposDeInmueble), $"Tipo_Inmueble: {tipo_Inmueble.nombre}\nActualizado con exito","Registro Actualizado con Exito!!!");

        }
        [HttpPost]
        public async Task<IActionResult> EliminarTipoInmueble(int id)
        {
            if(id <= 0 )
                return this.RedirectToActionWithError(nameof(ListarTiposDeInmueble), "EL parametro 'id' enviado deber ser mayor a '0'");
            (string?, bool) resultsFromService = await _tipoInmuebleService.deleteTipoInmueble(id);
            if (resultsFromService.Item1 != null)
                return this.RedirectToActionWithError(nameof(ListarTiposDeInmueble), resultsFromService.Item1);
            return this.RedirectToActionWithSuccess(nameof(ListarTiposDeInmueble), "Tipo de inmueble eliminado con éxito");
        }
    }
}
