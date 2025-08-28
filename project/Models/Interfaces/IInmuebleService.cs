namespace project.Models.Interfaces
{
    public interface IInmuebleService
    {
        public (string?,Boolean) AgregarInmueble(Inmueble inmueble);
        public (string?,Boolean) ActualizarInmueble(Inmueble inmueble);
        public (string?,Boolean) DarDeBajaInmueble(int idInmueble);
        public (string?,Inmueble?) BuscarInmueblePorDireccion(string direccion);
        public (string?, Inmueble?) ObtenerInmueblePorId(int idInmueble);
        public (string?, List<Inmueble>?) ObtenerTodosLosInmuebles();
        public (string?, List<Inmueble>?) ObtenerInmueblesPorPropietario(int dniPropietario);
        public (string?, List<Inmueble>?) ObtenerInmueblePorContrato(int idContrato);
    }
}
