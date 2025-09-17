namespace project.Models.Interfaces
{
    public interface IInmuebleService
    {
        public Task<(string?, Inmueble?)> AgregarInmueble(Inmueble inmueble);
        public Task<(string?, bool)> ActualizarInmueble(Inmueble inmueble);
        public Task<(string?, bool)> DarAltaLogica(int idInmueble);
        public Task<(string?, bool)> DarDeBajaInmueble(int idInmueble);
        public Task<(string?, bool)> MarcarAlquilado(int idInmueble);
        public Task<(string?, bool)> MarcarLibre(int idInmueble);
        public Task<(string?, Inmueble?)> BuscarInmueblePorDireccion(string direccion);
        public Task<(string?, Inmueble?)> ObtenerInmueblePorId(int idInmueble);
        public Task<(string?, List<Inmueble>?, int? totalRegistros)> ObtenerTodosLosInmuebles(int paginaNro = 1, int tamPagina = 10, bool? disponibilidad = null, int? dniPropietario = null, string? uso = null, string? tipoInmueble = null, int? cantidadAmbientes = null, int? precio = null, DateOnly? fechaDesde = null, DateOnly? fechaHasta = null);
        public Task<(string?, List<Inmueble>?)> ObtenerInmueblesPorPropietario(int dniPropietario);
        public Task<(string?, Inmueble?)> ObtenerInmueblePorContrato(int idContrato);
        public Task<(string?, int?)> obtenerCantidadDeRegistros();

        public Task<(string?, List<Inmueble>?)> ObtenerTodosLosInmueblesAPI();

        //METODOS PARA MANEJO DE IMAGENES HAY Q TEsteARLOS Hermano
        public Task<(string?, bool)> CargarImagen(bool esPortada, int idInmueble, Microsoft.AspNetCore.Http.IFormFile file);

        public Task<(string?, int)> ObtenerCantidadImagenes(int idInmueble);
        public Task<(string?, List<string>?)> ObtenerImagenesInmueble(int idInmueble);
        public Task<(string?, string?)> ObtenerImagenPortada(int idInmueble);
        public Task<(string?, bool)> EliminarImagen(int idInmueble, string nombreImagen);
    }
}
