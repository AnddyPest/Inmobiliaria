using Microsoft.AspNetCore.Mvc;
using project.Helpers;
using project.Models;
using project.Models.Interfaces;

namespace project.Controllers
{
    public class InquilinoController : Controller
    {
        private IInquilinoService inquilinoService;
        public InquilinoController(IInquilinoService inquilinoService) 
        {
           this.inquilinoService = inquilinoService;

        }
        [HttpGet]
        public async Task<IActionResult> getAllInquilinos()
        {

            (string?,List<Inquilino>) inquilinos = await inquilinoService.GetAllInquilinos();
            if(inquilinos.Item1 !=null)
            {
                HelperFor.imprimirMensajeDeError(inquilinos.Item1, nameof(InquilinoController), nameof(getAllInquilinos));
                return BadRequest(inquilinos.Item1);
            }
            Console.WriteLine(inquilinos.Item2);
            return Ok(inquilinos.Item2);
        }
        [HttpGet]
        public async Task<IActionResult> getInquilinoById(int idInquilino)
        {
            (string?, Inquilino?) inquilino = await inquilinoService.GetInquilinoById(idInquilino);
            if(inquilino.Item1 != null)
            {
                HelperFor.imprimirMensajeDeError(inquilino.Item1, nameof(InquilinoController), nameof(getInquilinoById));
                return BadRequest(inquilino.Item1);
            }
            if(inquilino.Item2 == null)
            {
                return NotFound();
            }
            return Ok(inquilino.Item2);
        }

    }
}
