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
        public async Task<IActionResult> ListarTiposDeInmueble()
        {
            (string?, List<Tipo_Inmueble>?) resultsFromService = await _tipoInmuebleService.getAllTipoInmueble();
            if (resultsFromService.Item1 != null)
                return BadRequest(resultsFromService.Item1);
            InmuebleViewModel viewModel = new();
            viewModel.tipo_Inmueble = resultsFromService.Item2;
            return View("", viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> RegistrarTipoInmueble(Tipo_Inmueble tipo_Inmueble)
        {
            if (!ModelState.IsValid)
            {
                var dataError = ModelStateExtensions.GetErrorMessages(ModelState);
                return BadRequest(dataError);
            }
            (string?, bool) resultsFromService = await _tipoInmuebleService.createTipoInmueble(tipo_Inmueble);
            if(resultsFromService.Item1 != null)
                return BadRequest(resultsFromService.Item1);
            return Ok("Tipo de inmueble registrado con éxito");
        }
        [HttpPost]
        public async Task<IActionResult> ActualizarTipoInmueble(Tipo_Inmueble tipo_Inmueble)
        {
            if (!ModelState.IsValid)
            {
                var dataError = ModelStateExtensions.GetErrorMessages(ModelState);
                return BadRequest(dataError);
            }
            (string?, bool) resultsFromService = await _tipoInmuebleService.updateTipoInmueble(tipo_Inmueble);
            if (resultsFromService.Item1 != null)
                return BadRequest(resultsFromService.Item1);
            return Ok("Tipo de inmueble actualizado con éxito");
        }
        [HttpPost]
        public async Task<IActionResult> EliminarTipoInmueble(int id)
        {
            (string?, bool) resultsFromService = await _tipoInmuebleService.deleteTipoInmueble(id);
            if (resultsFromService.Item1 != null)
                return BadRequest(resultsFromService.Item1);
            return Ok("Tipo de inmueble eliminado con éxito");
        }
    }
}
