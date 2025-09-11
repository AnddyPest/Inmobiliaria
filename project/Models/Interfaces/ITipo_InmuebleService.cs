namespace project.Models.Interfaces
{
    public interface ITipo_InmuebleService
    {
        public Task<(string?, List<Tipo_Inmueble>?)> getAllTipoInmueble();
        public Task<(string?, bool)> ValidarQueNoEsteAsignado(int id_tipo_inmueble);
        public Task<(string?, List<Tipo_Inmueble>?)> getAllTipoInmueble(int nroPagina, int cantidadPaginasPorHoja);
        public Task<(string?, bool)> createTipoInmueble(Tipo_Inmueble tipo_Inmueble);
        public Task<(string?, bool)> updateTipoInmueble(Tipo_Inmueble tipo_Inmueble);
        public Task<(string?, bool)> deleteTipoInmueble(int idTipoInmueble);
        public Task<(string?, Tipo_Inmueble?)> buscarTipoInmueblePorId(int idTipoInmueble);
        public Task<(string?, Tipo_Inmueble?)> buscarTipoInmueblePorNombre(string nombre);
        public Task<(string?, int?)> cantidadRegistros();

    }
}
