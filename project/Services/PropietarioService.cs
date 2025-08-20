using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using project.Models;
using project.Models.Interfaces;

namespace project.Services
{
    public class PropietarioService(IConfiguration config) : IPropietarioService
    {
        private string _connectionString = config.GetConnectionString("Connection")!;

        public Task<int> Alta(Propietario propietario)
        {
            throw new NotImplementedException();
        }

        public Task<int> Baja(int idPropietario)
        {
            throw new NotImplementedException();
        }

        public Task<int> Editar(Propietario propietario)
        {
            throw new NotImplementedException();
        }

        public Task<int> ObtenerCantidad()
        {
            throw new NotImplementedException();
        }

        public Task<Propietario> ObtenerPorDni(int dni)
        {
            throw new NotImplementedException();
        }

        public Task<IList<Propietario>> ObtenerPorNombre(string nombre)
        {
            throw new NotImplementedException();
        }

        public Task<IList<Propietario>> ObtenerTodos()
        {
            throw new NotImplementedException();
        }

        public Task<int> Reestablecer(int idPropietario)
        {
            throw new NotImplementedException();
        }
    }
}