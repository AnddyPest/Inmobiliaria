using Microsoft.AspNetCore.Mvc;

using project.Models.Interfaces;

using Microsoft.AspNetCore.Authorization;
using project.Models;
namespace project.Controllers
{
    [Authorize]
    public class AuditoriaController : Controller
    {
        private IAuditoriaService _auditoriaService;
        private IEmpleadoService _empleadoService;
        private IPagosService _pagosService;
        private IContratoService _contratoService;

        public AuditoriaController(IAuditoriaService auditoriaService, IEmpleadoService empleadoService, IPagosService pagosService, IContratoService contratoService)
        {
            _auditoriaService = auditoriaService;
            _empleadoService = empleadoService;
            _pagosService = pagosService;
            _contratoService = contratoService;
        }

        [HttpGet("/auditoria/contrato/{id}")]
        public async Task<IActionResult> GetAuditoriasByContrato(int id)
        {
            var auditoriasContrato = await _auditoriaService.GetAuditoriasByContrato(id);
            if (auditoriasContrato.Item1 != null)
            {
                return NotFound("Error al buscar la audditoría.");
            }
            return Json(new { auditorias = auditoriasContrato.Item2 });
        }
        [HttpGet("/auditoria/pagos/{id}")]
        public async Task<IActionResult> GetAuditoriasByPago(int id)
        {
            var auditoriasPago = await _auditoriaService.GetAuditoriasByPago(id);
            if (auditoriasPago.Item1 != null)
            {
                return NotFound("Error al buscar la audditoría.");
            }
            return Json(new { auditorias = auditoriasPago.Item2 });
        }
    }
}
    
