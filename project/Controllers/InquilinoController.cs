using Microsoft.AspNetCore.Mvc;
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
        public async Task<List<Inquilino>> getAllInquilinos()
        {

            (string?,List<Inquilino>) inquilinos = await inquilinoService.GetAllInquilinos();
            Console.WriteLine(inquilinos.Item2);
            return inquilinos.Item2;
        }

    }
}
