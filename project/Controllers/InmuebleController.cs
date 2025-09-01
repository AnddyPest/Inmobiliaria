
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using project.Helpers;
using project.Models;
using project.Models.Interfaces;

namespace project.Controllers
{
    public class InmuebleController(IInmuebleService inmuebleService) : Controller
    {
        private IInmuebleService _inmuebleService = inmuebleService;
        [HttpGet("Inmueble")]
        public IActionResult Index()
        {
            return View("~/Views/Inmuebles/IndexInmueble.cshtml");
        }
        [HttpGet("Inmueble/Agregar")]
        public IActionResult Agregar()
        {
            return View("~/Views/Inmuebles/VistaRegistrarInmueble.cshtml");
        }
        [HttpGet("inmueble/listar")]
        public async Task<IActionResult> GetAllInmuebles()
        {
            (string?, List<Inmueble>?) inmuebles = await _inmuebleService.ObtenerTodosLosInmuebles();
            if (inmuebles.Item1 != null)
            {
                HelperFor.imprimirMensajeDeError(inmuebles.Item1, nameof(InmuebleController), nameof(GetAllInmuebles));
                return BadRequest(inmuebles.Item1);
            }
            Console.WriteLine(inmuebles.Item2);
            return Ok(inmuebles.Item2);
        }
        [HttpGet("inmueble/find/{idInmueble}")]
        public async Task<IActionResult> GetInmuebleById(int idInmueble)
        {
            (string?, Inmueble?) inmueble = await _inmuebleService.ObtenerInmueblePorId(idInmueble);
            if (inmueble.Item1 != null)
            {
                HelperFor.imprimirMensajeDeError(inmueble.Item1, nameof(InmuebleController), nameof(GetInmuebleById));
                return BadRequest(inmueble.Item1);
            }
            if (inmueble.Item2 == null)
            {
                return NotFound();
            }
            return Ok(inmueble.Item2);
        }
        [HttpPost("inmueble/crear")]
        public async Task<IActionResult> AddInmueble(Inmueble model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            (string?, Inmueble?) inmuebleCreated = await _inmuebleService.AgregarInmueble(model);
            if (inmuebleCreated.Item1 != null)
                return BadRequest(inmuebleCreated.Item1);
            if (inmuebleCreated.Item2?.IdInmueble == null || inmuebleCreated.Item2.IdInmueble == 0)
                return BadRequest("No se pudo crear el inmueble.");
            return Ok(inmuebleCreated.Item2);
        }

        [HttpPost("inmueble/actualizar")]
        public async Task<IActionResult> UpdateInmueble(Inmueble model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            (string?, bool) inmuebleUpdated = await _inmuebleService.ActualizarInmueble(model);
            if (inmuebleUpdated.Item1 != null)
                return BadRequest(inmuebleUpdated.Item1);
            if (!inmuebleUpdated.Item2)
                return BadRequest("No se pudo actualizar el inmueble.");
            return Ok("Inmueble actualizado correctamente.");
        }
        [HttpPost("inmueble/darDeBaja/{idInmueble}")]
        public async Task<IActionResult> DeleteInmueble(int idInmueble)
        {
            (string?, bool) inmuebleDeleted = await _inmuebleService.DarDeBajaInmueble(idInmueble);
            if (inmuebleDeleted.Item1 != null)
                return BadRequest(inmuebleDeleted.Item1);
            if (!inmuebleDeleted.Item2)
                return BadRequest("No se pudo dar de baja el inmueble.");
            return Ok("Inmueble dado de baja correctamente.");
        }

    }
}
