namespace project.Models.Interfaces
{
    public interface ITipo_InmuebleService
    {
        public Task<(string?, List<Tipo_Inmueble>?)> getAllTipoInmueble();
    }
}
