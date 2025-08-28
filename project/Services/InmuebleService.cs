using project.Models;
using project.Models.Interfaces;

namespace project.Services
{
    public class InmuebleService(IConfiguration configuration) : IInmuebleService
    {
        private readonly string _connectionString = configuration.GetConnectionString("Connection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        public (string?, bool) ActualizarInmueble(Inmueble inmueble)
        {
            throw new NotImplementedException();
        }

        public (string?, bool) AgregarInmueble(Inmueble inmueble)
        {
            throw new NotImplementedException();
        }

        public (string?, bool) DarDeBajaInmueble(int idInmueble)
        {
            throw new NotImplementedException();
        }

        public (string?, List<Inmueble>?) ObtenerInmueblePorContrato(int idContrato)
        {
            throw new NotImplementedException();
        }

        public (string?, Inmueble?) ObtenerInmueblePorId(int idInmueble)
        {
            throw new NotImplementedException();
        }

        public (string?, List<Inmueble>?) ObtenerInmueblesPorPropietario(int dniPropietario)
        {
            throw new NotImplementedException();
        }

        public (string?, List<Inmueble>?) ObtenerTodosLosInmuebles()
        {
            throw new NotImplementedException();
        }
    }
}
