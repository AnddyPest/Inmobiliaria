using Microsoft.AspNetCore.Mvc;
using project.Helpers;
using project.Models;
using project.Models.Interfaces;
using project.Models.ViewModels;


namespace project.Controllers
{
    public class InmuebleController : Controller
    {
        private IInmuebleService _inmuebleService;
        private ITipo_InmuebleService _tipoInmuebleService;
        private IPropietarioService _propietarioService;
        private IContratoService _contratoService;
        private IInquilinoService _inquilinoService;

        public InmuebleController(IInmuebleService inmuebleService, ITipo_InmuebleService iTipoInmuebleService, IPropietarioService iPropietarioService, IContratoService iContratoService, IInquilinoService inquilinoService) : base()
        {
            _inmuebleService = inmuebleService;
            _tipoInmuebleService = iTipoInmuebleService;
            _propietarioService = iPropietarioService;
            _contratoService = iContratoService;
            _inquilinoService = inquilinoService;
        }

        [HttpGet("Inmueble")]
        public IActionResult Index()
        {
            return View("~/Views/Inmuebles/IndexInmueble.cshtml");
        }
        [HttpGet("Inmueble/{idInmueble}")]
        public async Task<IActionResult> Actualizar(int idInmueble)
        {
            InmuebleViewModel viewModel = new();
            (string?,Inmueble?) inmuebleFromService =  await _inmuebleService.ObtenerInmueblePorId(idInmueble);
            if(inmuebleFromService.Item1 != null)
            {
                HelperFor.imprimirMensajeDeError(inmuebleFromService.Item1, nameof(InmuebleController), nameof(Actualizar));
                return BadRequest(inmuebleFromService.Item1);
            }
            if (inmuebleFromService.Item2 != null)
                viewModel.InmuebleOnly = inmuebleFromService.Item2;
            
            (string?, List<Tipo_Inmueble>?) typesFromService = await _tipoInmuebleService.getAllTipoInmueble();
            if (typesFromService.Item2 != null)
            {
                viewModel.tipo_Inmueble = typesFromService.Item2;
            }
            (string?, List<Propietario>?) propietarioFromService = await _propietarioService.ObtenerTodos();
            if(propietarioFromService.Item2 != null)
            {
                viewModel.propietarios = propietarioFromService.Item2;
            }
            

            return View("~/Views/Inmuebles/VistaActualizarInmueble.cshtml", viewModel);
        }
        [HttpGet("Inmueble/Agregar")]
        public async Task<IActionResult> Agregar()
        {
            InmuebleViewModel viewModel = new();
            (string?, List<Tipo_Inmueble>?) listaInmueblesFromService = await _tipoInmuebleService.getAllTipoInmueble();
            if (listaInmueblesFromService.Item2 != null) viewModel.tipo_Inmueble = listaInmueblesFromService.Item2;
            (string?, List<Propietario>?) propietariosFromService = await _propietarioService.ObtenerTodos();
            if (propietariosFromService.Item2 != null) viewModel.propietarios = propietariosFromService.Item2;
                
            return View("~/Views/Inmuebles/VistaRegistrarInmueble.cshtml", viewModel);
        }
        

        [HttpGet("inmueble/listar")]
        public async Task<IActionResult> GetAllInmuebles( int nroPagina = 1, bool? disponibilidad = null ,int? dniPropietario = null,string? uso = null, string? tipoInmueble = null, int? cantidadAmbientes = null, int? precio = null,DateOnly? fechaDesde = null, DateOnly? fechaHasta = null)
        {
            InmuebleViewModel viewModel = new();
            ViewBag.nroPagina = nroPagina;
            const int registrosPorPagina = 5;
            (string?, List<Inmueble>?) inmuebles = await _inmuebleService.ObtenerTodosLosInmuebles(Math.Max(nroPagina,1), registrosPorPagina, disponibilidad, dniPropietario, uso, tipoInmueble, cantidadAmbientes, precio, fechaDesde, fechaHasta);
            if (inmuebles.Item1 != null && inmuebles.Item1 != "No se encontraron inmuebles")
            {
                HelperFor.imprimirMensajeDeError(inmuebles.Item1, nameof(InmuebleController), nameof(GetAllInmuebles));
                return this.RedirectToActionWithError(nameof(Index), inmuebles.Item1);
            }
            (string?, int?) cantidadRegistros = await _inmuebleService.obtenerCantidadDeRegistros();
            if (cantidadRegistros.Item1 != null)
            {
                HelperFor.imprimirMensajeDeError(cantidadRegistros.Item1, nameof(InmuebleController), nameof(GetAllInmuebles));
                return this.RedirectToActionWithError(nameof(Index),cantidadRegistros.Item1);
            }
            Console.WriteLine($"Cantidad de registros: {cantidadRegistros.Item2}");
            viewModel.cantidadTotalDePaginas = cantidadRegistros.Item2 % registrosPorPagina == 0 ? cantidadRegistros.Item2 / registrosPorPagina : cantidadRegistros.Item2 / registrosPorPagina + 1;
            viewModel.inmueble = inmuebles.Item2;
            
            // Obtener todos los contratos y asignar al ViewModel
            (string?, List<Contrato>?) contratosResult = await _contratoService.GetContratosAPI();
            if (contratosResult.Item1 != null)
            {
                HelperFor.imprimirMensajeDeError(contratosResult.Item1, nameof(InmuebleController), nameof(GetAllInmuebles));
                viewModel.Contratos = new List<Contrato>();
            }
            else
            {
                viewModel.Contratos = contratosResult.Item2;
            }
            
            // Poblar la lista de inquilinos en el ViewModel
            (string?, List<Inquilino>) inquilinosResult = await _inquilinoService.GetAllInquilinos();
            if (inquilinosResult.Item1 != null)
            {
                HelperFor.imprimirMensajeDeError(inquilinosResult.Item1, nameof(InmuebleController), nameof(GetAllInmuebles));
                viewModel.inquilinos = new List<Inquilino>();
            }
            else
            {
                viewModel.inquilinos = inquilinosResult.Item2;
            }
            ViewBag.dniPropietario = dniPropietario;
            ViewBag.disponibilidad = disponibilidad;
            return View("~/Views/Inmuebles/VistaLIstaInmuebles.cshtml", viewModel);
        }
        //BUSCAR INMUEBLE POR ID
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
        //AGREGAR INMUEBLE
        [HttpPost("/Inmueble/crear")]
        public async Task<IActionResult> AddInmueble(Inmueble model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            (string?, Inmueble?) inmuebleCreated = await _inmuebleService.AgregarInmueble(model);
            if (inmuebleCreated.Item1 != null)
                return this.RedirectToActionWithError(nameof(GetAllInmuebles), inmuebleCreated.Item1);
            if (inmuebleCreated.Item2?.IdInmueble == null || inmuebleCreated.Item2.IdInmueble == 0)
                return this.RedirectToActionWithError(nameof(GetAllInmuebles), "No se pudo crear el inmueble.");
            
            return this.RedirectToActionWithSuccess(nameof(GetAllInmuebles), "Inmueble registrado con exito","Inmueble Registrado!!");
        }

        [HttpPost("Inmueble/actualizar")]
        public async Task<IActionResult> UpdateInmueble(Inmueble model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            (string?, bool) inmuebleUpdated = await _inmuebleService.ActualizarInmueble(model);
            if (inmuebleUpdated.Item1 != null)
                return this.RedirectToActionWithError(nameof(GetAllInmuebles), inmuebleUpdated.Item1);
            if (!inmuebleUpdated.Item2)
                return this.RedirectToActionWithError(nameof(GetAllInmuebles), "No se pudo actualizar el inmueble.");
            return this.RedirectToActionWithSuccess(nameof(GetAllInmuebles),"Inmueble actualizado con exito", "Inmueble Actualizado!!");
        }
        //DAR DE BAJA LOGICA
        [HttpGet("Inmueble/DarBajaLogica/{idInmueble}")]
        public async Task<IActionResult> DarBajaLogica(int idInmueble)
        {
            (string?, bool) inmuebleDeleted = await _inmuebleService.DarDeBajaInmueble(idInmueble);
            if (inmuebleDeleted.Item1 != null)
                return BadRequest(inmuebleDeleted.Item1);
            if (!inmuebleDeleted.Item2)
                return BadRequest("No se pudo dar de baja el inmueble.");
            return Redirect("/inmueble/listar");
        }
        [HttpGet("Inmueble/DarAltaLogica/{idInmueble}")]
        public async Task<IActionResult> DarAltaLogica(int idInmueble)
        {
            
            (string?, bool) inmuebleUp = await _inmuebleService.DarAltaLogica(idInmueble);
            if(inmuebleUp.Item1 != null && !inmuebleUp.Item2)
            {
                HelperFor.imprimirMensajeDeError(inmuebleUp.Item1, nameof(InmuebleController), nameof(DarAltaLogica));
                return BadRequest(inmuebleUp.Item1);
            }
            return Redirect("/inmueble/listar");

        }
        [HttpPost("Inmueble/MarcarAlquilado/{idInmueble}")]
        public async Task<IActionResult> MarcarAlquilado(int idInmueble)
        {
            Console.WriteLine($"[INMUEBLE] MarcarAlquilado llamado con idInmueble: {idInmueble}");
            (string?, bool) inmuebleLow = await _inmuebleService.MarcarAlquilado(idInmueble);
            Console.WriteLine($"[INMUEBLE] Respuesta de MarcarAlquilado: error={inmuebleLow.Item1}, exito={inmuebleLow.Item2}");
            if(inmuebleLow.Item1 != null)
            {
                HelperFor.imprimirMensajeDeError(inmuebleLow.Item1, nameof(InmuebleController), nameof(MarcarAlquilado));
                //AGREGAR MENSAJE 
            }
            return Redirect("/inmueble/listar");
        }
        [HttpPost("Inmueble/MarcarLibre/{idInmueble}")]
        public async Task<IActionResult> MarcarLibre(int idInmueble)
        {
            (string?, bool) inmuebleUp = await _inmuebleService.MarcarLibre(idInmueble);
            if (inmuebleUp.Item1 != null)
            {
                HelperFor.imprimirMensajeDeError(inmuebleUp.Item1, nameof(InmuebleController), nameof(MarcarAlquilado));
                return this.RedirectToActionWithError(nameof(GetAllInmuebles), inmuebleUp.Item1);
            }
            return this.RedirectToActionWithSuccess(nameof(GetAllInmuebles),"Inmueble marcado como disponible", "Inmueble Disponible!");
        }

    }
}
