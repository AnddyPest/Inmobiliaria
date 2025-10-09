using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using project.Helpers;
using project.Models;
using project.Models.Interfaces;


namespace project.Controllers
{
    [Authorize]
    public class PropietarioController : Controller
    {
        private IPropietarioService propietarioService;
        private IPersonaService personaService;
        public PropietarioController(IPropietarioService propietarioServ, IPersonaService personaService)
        {
            this.propietarioService = propietarioServ;
            this.personaService = personaService;
        }

        [HttpGet("Propietario/listar")]
        public async Task<IActionResult> ObtenerTodos() //Testeado y funcional
        {

            (string?, List<Propietario>?) propietarios = await propietarioService.ObtenerTodos(null);
            if (propietarios.Item1 != null)
            {
                HelperFor.imprimirMensajeDeError(propietarios.Item1, nameof(PropietarioController), nameof(ObtenerTodos));
                return this.RedirectToActionWithError(nameof(VistaPropietarios),propietarios.Item1,"Interanl Server Error");
            }
            Console.WriteLine(propietarios.Item2);
            return View("~/Views/Propietarios/GestionPropietarios.cshtml", propietarios.Item2);
        }

        [HttpPost("Propietario/Create")]
        public async Task<IActionResult> AgregarPropietario(Persona persona) //testear
        {
            if (!ModelState.IsValid) 
                return this.RedirectToActionWithError(nameof(ObtenerTodos),ModelState.GetErrorMessages());
            if (persona.Dni <= 0) 
                return this.RedirectToActionWithWarning(nameof(NuevoPropietario), "Se requiere dni y debe ser mayor que 0");
            Persona? personaRegistrada = await personaService.ObtenerPorDni(persona.Dni);

            if (personaRegistrada == null)
            {
                int codeResult = await personaService.Alta(persona);
                if (codeResult == -1)
                    return this.RedirectToActionWithError(nameof(ObtenerTodos), "No se registro a la persona.");
                persona.IdPersona = codeResult;
            }
            else
            {
                persona.IdPersona = personaRegistrada.IdPersona;
            }
            (string?, Boolean) propietario = await propietarioService.Alta(persona.IdPersona);
            if (propietario.Item1 != null && !propietario.Item2) 
                return this.RedirectToActionWithError(nameof(NuevoPropietario),propietario.Item1,nameof(BadRequest));

            return this.RedirectToActionWithSuccess(nameof(ObtenerTodos),"El propietario se ha registrado correctamente","Propietario Registrado!!!");
        }
        [HttpPost("Propietario/Update")]
        public async Task<IActionResult> ActualizarPropietario(Persona personaEnviadaDesdeElFront) //testear
        {
            if (!ModelState.IsValid) 
                return this.RedirectToActionWithError(nameof(ActualizarPropietario),ModelState.GetErrorMessages(), new {id = personaEnviadaDesdeElFront.IdPersona});
            if (personaEnviadaDesdeElFront.IdPersona <= 0)
                return this.RedirectToActionWithError(nameof(ActualizarPropietario), "Se requiere idPersona y debe ser mayor que 0", new { id = personaEnviadaDesdeElFront.IdPersona });
            
            (string?, Persona?) personaDesdeDB = await personaService.GetPersonaById(personaEnviadaDesdeElFront.IdPersona, true);
            if (personaDesdeDB.Item1 != null)
                return this.RedirectToActionWithError(nameof(ActualizarPropietario), personaDesdeDB.Item1, "Error al intentar actualizar");

            int codeResult = await personaService.Editar(personaEnviadaDesdeElFront);
            if (codeResult == -1)
            {
                HelperFor.imprimirMensajeDeError("No se pudo actualizar la persona", nameof(PropietarioController), nameof(ActualizarPropietario));
                SweetAlertHelper.SweetAlertError(this, "No se pudo actualizar la persona", "Error");
                return Redirect($"/Propietario/Update?id={personaEnviadaDesdeElFront.IdPersona}");
            }    

            (string?, Propietario?) propietario = await propietarioService.getPropietarioByIdPersona(personaEnviadaDesdeElFront.IdPersona);
            if (propietario.Item1 != null) 
                return this.RedirectToActionWithError(nameof(ActualizarPropietario),propietario.Item1, "Internal Server Error");

            return this.RedirectToActionWithSuccess(nameof(ObtenerTodos),"Propietario Actualizado con exito");
        }
        [Authorize(Roles = "Administrador")]
        [HttpPost("Propietario/Baja")]
        public async Task<IActionResult> BajaPropietario([FromBody] int idPropietario) //testear
        {

            if (idPropietario <= 0) 
                return this.RedirectToActionWithError(nameof(ObtenerTodos), "Se requiere idPropietario y debe ser mayor a 0",nameof(BadRequest));
            (string?, Boolean) codeResult = await propietarioService.BajaLogica(idPropietario);
            if (!codeResult.Item2 && codeResult.Item1 != null) 
                return this.RedirectToActionWithError(nameof(ObtenerTodos), codeResult.Item1,nameof(BadRequest));

            return this.RedirectToActionWithSuccess(nameof(ObtenerTodos), "El propietario fue dado de baja correctamente");
        }
        [Authorize(Roles = "Administrador")]
        [HttpPost("Propietario/Alta")]
        public async Task<IActionResult> AltaPropietario([FromBody] int idPropietario) //testear
        {
            Console.WriteLine("ingreso");
            if (idPropietario <= 0)
                return this.RedirectToActionWithError(nameof(ObtenerTodos), "Se requiere idPropietario y debe ser mayor a 0", nameof(BadRequest));
            (string?, Boolean) codeResult = await propietarioService.AltaLogica(idPropietario);
            if (!codeResult.Item2 && codeResult.Item1 != null)
                return this.RedirectToActionWithError(nameof(ObtenerTodos), codeResult.Item1, nameof(BadRequest));
            return this.RedirectToActionWithSuccess(nameof(ObtenerTodos), "El propietario fue dado de baja correctamente");

        }
        [HttpGet("Propietario")]
        public IActionResult VistaPropietarios()
        {
            return View("~/Views/Propietarios/IndexPropietarios.cshtml");
        }
        [HttpGet("Propietario/New")]
        public IActionResult NuevoPropietario()
        {
            return View("~/Views/Propietarios/NewPropietario.cshtml");
        }
        [HttpGet("Propietario/Update")]
        public async Task<IActionResult> ActualizarPropietario(int id)
        {
            var persona = await propietarioService.getPropietarioByIdPersona(id);
            if (persona.Item1 != null) return NotFound(persona.Item1);
            return View("~/Views/Propietarios/EditPropietarios.cshtml", persona.Item2);
        }

        //METODO ESPECIFICO PARA BUSCAR PERSONA POR DNI Y DETERMINAR SI ES PROPIETARIO
        [HttpGet("Propietario/BuscarPorDni")]
        public async Task<IActionResult> BuscarPorDni(int dni)
        {
            var persona = await personaService.ObtenerPorDni(dni);
            if (persona == null || persona.IdPersona == 0)
                return NotFound();

            // Comprobar si ya es propietario
            var propietario = await propietarioService.getPropietarioByIdPersona(persona.IdPersona);
            bool esPropietario = propietario.Item2 != null;

            // Devolver persona + info de propietario
            return Json(new
            {
                nombre = persona.Nombre,
                apellido = persona.Apellido,
                telefono = persona.Telefono,
                direccion = persona.Direccion,
                email = persona.Email,
                esPropietario = esPropietario
            });
        }
    }
}
