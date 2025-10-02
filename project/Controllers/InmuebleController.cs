using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using project.Helpers;
using project.Models;
using project.Models.Interfaces;
using project.Models.ViewModels;


namespace project.Controllers
{
    [Authorize]
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
            (string?, Inmueble?) inmuebleFromService = await _inmuebleService.ObtenerInmueblePorId(idInmueble);
            if (inmuebleFromService.Item1 != null)
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
            (string?, List<Propietario>?) propietarioFromService = await _propietarioService.ObtenerTodos(null);
            if (propietarioFromService.Item2 != null)
            {
                viewModel.propietarios = propietarioFromService.Item2;
            }


            return View("~/Views/Inmuebles/VistaActualizarInmueble.cshtml", viewModel);
        }
        [HttpGet("Inmueble/Agregar")]
        public async Task<IActionResult> Agregar(string? propietarioFiltro, int? validacion = 0)
        {
            InmuebleViewModel viewModel = new();
            (string?, List<Tipo_Inmueble>?) listaInmueblesFromService = await _tipoInmuebleService.getAllTipoInmueble();
            if (listaInmueblesFromService.Item2 != null) viewModel.tipo_Inmueble = listaInmueblesFromService.Item2;
            if (validacion == 0)
                viewModel.propietarios = new List<Propietario>();
            if (validacion == 1)
            {
                (string?, List<Propietario>?) propietariosFromService = await _propietarioService.ObtenerTodos(propietarioFiltro);
                if (propietariosFromService.Item2 != null) viewModel.propietarios = propietariosFromService.Item2;
            }

            return View("~/Views/Inmuebles/VistaRegistrarInmueble.cshtml", viewModel);
        }


        [HttpGet("inmueble/listar")]
        public async Task<IActionResult> GetAllInmuebles(int nroPagina = 1, bool? disponibilidad = null, int? dniPropietario = null, string? uso = null, string? tipo = null, int? cantAmbientes = null, int? precio = null, DateOnly? fechaDesde = null, DateOnly? fechaHasta = null)
        {
            InmuebleViewModel viewModel = new();
            ViewBag.nroPagina = nroPagina;
            const int registrosPorPagina = 4;
            (string?, List<Inmueble>?, int? cantidadTotalRegistros) inmuebles = await _inmuebleService.ObtenerTodosLosInmuebles(Math.Max(nroPagina, 1), registrosPorPagina, disponibilidad, dniPropietario, uso, tipo, cantAmbientes, precio, fechaDesde, fechaHasta);
            if (inmuebles.Item1 != null && inmuebles.Item1 != "No se encontraron inmuebles")
            {
                HelperFor.imprimirMensajeDeError(inmuebles.Item1, nameof(InmuebleController), nameof(GetAllInmuebles));
                return this.RedirectToActionWithError(nameof(Index), inmuebles.Item1);
            }
            foreach (Inmueble inmueble in inmuebles.Item2!)
            {
                (string?, List<Contrato>?) contratosByInmueble = await _contratoService.GetContratoByIdInmueble(inmueble.IdInmueble);
                if (contratosByInmueble.Item2 != null)
                {
                    inmueble.contratos = contratosByInmueble.Item2;
                }
            }
            (string?, List<Tipo_Inmueble>?) tiposDeInmuebleFromService = await _tipoInmuebleService.getAllTipoInmueble();
            if (tiposDeInmuebleFromService.Item1 != null)
                return this.RedirectToActionWithError(nameof(Index), "Error del servicio que obtiene los tipos de Inmueble", "Internal Server Error");

            viewModel.cantidadTotalDePaginas = inmuebles.cantidadTotalRegistros % registrosPorPagina == 0 ? inmuebles.cantidadTotalRegistros / registrosPorPagina : inmuebles.cantidadTotalRegistros / registrosPorPagina + 1;
            viewModel.inmueble = inmuebles.Item2;
            viewModel.tipo_Inmueble = tiposDeInmuebleFromService.Item2;
            // Obtener todos los contratos y asignar al ViewModel
            // (string?, List<Contrato>?) contratosResult = await _contratoService.GetContratosAPI();
            // if (contratosResult.Item1 != null)
            // {
            //     HelperFor.imprimirMensajeDeError(contratosResult.Item1, nameof(InmuebleController), nameof(GetAllInmuebles));
            //     viewModel.Contratos = new List<Contrato>();
            // }
            // else
            // {
            //     viewModel.Contratos = contratosResult.Item2;
            // }

            // Poblar la lista de inquilinos en el ViewModel
            // (string?, List<Inquilino>) inquilinosResult = await _inquilinoService.GetAllInquilinos();
            // if (inquilinosResult.Item1 != null)
            // {
            //     HelperFor.imprimirMensajeDeError(inquilinosResult.Item1, nameof(InmuebleController), nameof(GetAllInmuebles));
            //     viewModel.inquilinos = new List<Inquilino>();
            // }
            // else
            // {
            //     viewModel.inquilinos = inquilinosResult.Item2;
            // }
            ViewBag.dniPropietario = dniPropietario;
            ViewBag.disponibilidad = disponibilidad;
            ViewBag.uso = uso;
            ViewBag.tipo = tipo;
            ViewBag.cantAmbientes = cantAmbientes;
            ViewBag.precio = precio;
            if (fechaDesde != null && fechaHasta != null)
            {
                ViewBag.fechaDesde = fechaDesde.Value.ToString("yyyy-MM-dd");
                ViewBag.fechaHasta = fechaHasta.Value.ToString("yyyy-MM-dd");
            }

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

            return this.RedirectToActionWithSuccess(nameof(GetAllInmuebles), "Inmueble registrado con exito", "Inmueble Registrado!!");
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
            return this.RedirectToActionWithSuccess(nameof(GetAllInmuebles), "Inmueble actualizado con exito", "Inmueble Actualizado!!");
        }
        //DAR DE BAJA LOGICA
        [Authorize(Roles = "Administrador")]
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
        [Authorize(Roles = "Administrador")]
        [HttpGet("Inmueble/DarAltaLogica/{idInmueble}")]
        public async Task<IActionResult> DarAltaLogica(int idInmueble)
        {

            (string?, bool) inmuebleUp = await _inmuebleService.DarAltaLogica(idInmueble);
            if (inmuebleUp.Item1 != null && !inmuebleUp.Item2)
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
            if (inmuebleLow.Item1 != null)
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
            return this.RedirectToActionWithSuccess(nameof(GetAllInmuebles), "Inmueble marcado como disponible", "Inmueble Disponible!");
        }

        [HttpGet("Inmueble/API/listar")]
        public async Task<IActionResult> ListarInmueblesAPI(int nroPagina = 1, bool? disponibilidad = null, int? dniPropietario = null)
        {
            (string?, List<Inmueble>?, int?) inmuebles = await _inmuebleService.ObtenerTodosLosInmuebles(nroPagina, 10, disponibilidad, dniPropietario);
            if (inmuebles.Item1 != null)
            {
                return BadRequest(inmuebles.Item1);
            }
            return Ok(inmuebles.Item2);
        }
        [HttpGet("Inmueble/VerImagenes/{idInmueble}")]
        public async Task<IActionResult> VerImagenes(int idInmueble)
        {
            (string?, Inmueble?) inmueble = await _inmuebleService.ObtenerInmueblePorId(idInmueble);
            if (inmueble.Item1 != null)
            {
                HelperFor.imprimirMensajeDeError(inmueble.Item1, nameof(InmuebleController), nameof(VerImagenes));
                return this.RedirectToActionWithError(nameof(GetAllInmuebles), inmueble.Item1);
            }
            if (inmueble.Item2 == null)
            {
                return this.RedirectToActionWithError(nameof(GetAllInmuebles), "No se encontro el inmueble");
            }
            ViewBag.Inmueble = inmueble.Item2;
            (string?, String?) inmuebleFromService = await _inmuebleService.ObtenerImagenPortada(idInmueble);
            (string?, List<String>?) imagenesInmueble = await _inmuebleService.ObtenerImagenesInmueble(idInmueble);
            if (inmuebleFromService.Item1 != null)
            {
                HelperFor.imprimirMensajeDeError(inmuebleFromService.Item1, nameof(InmuebleController), nameof(VerImagenes));
                return this.RedirectToActionWithError(nameof(GetAllInmuebles), inmuebleFromService.Item1);
            }
            ViewBag.ImagenPortada = inmuebleFromService.Item2;
            if (imagenesInmueble.Item1 != null)
            {
                HelperFor.imprimirMensajeDeError(imagenesInmueble.Item1, nameof(InmuebleController), nameof(VerImagenes));
                return this.RedirectToActionWithError(nameof(GetAllInmuebles), imagenesInmueble.Item1);
            }
            ViewBag.Imagenes = imagenesInmueble.Item2;
            return View("~/Views/Inmuebles/ImagenesInmuebles.cshtml");
        }

        [HttpPost("Inmueble/UploadImages")]
        public async Task<IActionResult> UploadImages(int idInmueble, List<IFormFile> imageFiles, bool esPortada = false)
        {
            try
            {
                if (imageFiles == null || imageFiles.Count == 0)
                {
                    return this.RedirectToActionWithError(nameof(VerImagenes), "Inmueble", "No se seleccionaron imágenes para subir.", new { idInmueble });
                }

                int sucessfulUploads = 0;
                int failedUploads = 0;
                string? lastError = null;

                foreach (var file in imageFiles)
                {
                    if (file != null && file.Length > 0)
                    {
                        (string?, bool) uploadResult = await _inmuebleService.CargarImagen(esPortada, idInmueble, file);
                        if (uploadResult.Item1 != null)
                        {
                            HelperFor.imprimirMensajeDeError(uploadResult.Item1, nameof(InmuebleController), nameof(UploadImages));
                            lastError = uploadResult.Item1;
                            failedUploads++;
                        }
                        else if (uploadResult.Item2)
                        {
                            sucessfulUploads++;
                        }
                        else
                        {
                            failedUploads++;
                        }
                    }
                }

                string messageType = esPortada ? "imagen de portada" : "imágenes";
                if (sucessfulUploads > 0 && failedUploads == 0)
                {
                    return this.RedirectToActionWithSuccess(nameof(VerImagenes), "Inmueble", $"Se subió la {messageType} con éxito.", new { idInmueble }, esPortada ? "Portada Subida!" : "Imágenes Subidas!");
                }
                else if (sucessfulUploads > 0)
                {
                    return this.RedirectToActionWithSuccess(nameof(VerImagenes), "Inmueble", $"Se subieron {sucessfulUploads} {messageType}. {failedUploads} fallaron.", new { idInmueble }, "Subida Parcial");
                }
                else
                {
                    return this.RedirectToActionWithError(nameof(VerImagenes), "Inmueble", lastError ?? $"No se pudo subir la {messageType}.", new { idInmueble });
                }
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(InmuebleController), nameof(UploadImages));
                return this.RedirectToActionWithError(nameof(VerImagenes), "Inmueble", "Error al subir las imágenes: " + ex.Message, new { idInmueble });
            }
        }
        [HttpPost("Inmueble/DeleteImage")]
        public async Task<IActionResult> DeleteImage(int idInmueble, string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                {
                    return this.RedirectToActionWithError(nameof(VerImagenes), "Inmueble", "No se proporcionó una URL de imagen válida para eliminar.", new { idInmueble });
                }

                (string?, bool) deleteResult = await _inmuebleService.EliminarImagen(idInmueble, imageUrl);
                if (deleteResult.Item1 != null)
                {
                    HelperFor.imprimirMensajeDeError(deleteResult.Item1, nameof(InmuebleController), nameof(DeleteImage));
                    return this.RedirectToActionWithError(nameof(VerImagenes), "Inmueble", deleteResult.Item1, new { idInmueble });
                }
                if (!deleteResult.Item2)
                {
                    return this.RedirectToActionWithError(nameof(VerImagenes), "Inmueble", "No se pudo eliminar la imagen.", new { idInmueble });
                }

                return this.RedirectToActionWithSuccess(nameof(VerImagenes), "Inmueble", "Imagen eliminada con éxito.", new { idInmueble }, "Imagen Eliminada!");
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(InmuebleController), nameof(DeleteImage));
                return this.RedirectToActionWithError(nameof(VerImagenes), "Inmueble", "Error al eliminar la imagen: " + ex.Message, new { idInmueble });
            }
        }
        [HttpGet("Inmueble/FechasOcupadas/{idInmueble}")]
        public async Task<IActionResult> FechasOcupadas(int idInmueble)
        {
            try
            {
                var (error, fechasOcupadas) = await _inmuebleService.ObtenerFechasOcupadas(idInmueble);
                if (error != null || fechasOcupadas == null)
                    return BadRequest("Error al obtener las fechas ocupadas: " + error);
                return Json(fechasOcupadas);
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(InmuebleController), nameof(FechasOcupadas));
                return BadRequest("Error al obtener las fechas ocupadas: " + ex.Message);
            }
        }
    }
}
