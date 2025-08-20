using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using project.Models;
using project.Models.Interfaces;

namespace project.Services
{
    public class PropietarioService(IConfiguration config) : IPropietarioService
    {
        private string _connectionString = config.GetConnectionString("Connection");

        public async Task<int> Alta(Propietario propietario)
        {
            int res = -1;
            try
            {
                string query = @"INSERT INTO Persona (Nombre, Apellido, Dni, Telefono, Direccion, Email, Estado) 
                                VALUES (@Nombre, @Apellido, @Dni, @Telefono, @Direccion, @Email, @Estado)";
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Nombre", propietario.Nombre);
                        cmd.Parameters.AddWithValue("@Apellido", propietario.Apellido);
                        cmd.Parameters.AddWithValue("@Dni", propietario.Dni);
                        cmd.Parameters.AddWithValue("@Telefono", propietario.Telefono);
                        cmd.Parameters.AddWithValue("@Direccion", propietario.Direccion);
                        cmd.Parameters.AddWithValue("@Email", propietario.Email);
                        cmd.Parameters.AddWithValue("@Estado", true);
                            await conn.OpenAsync();
                        res = await cmd.ExecuteNonQueryAsync();
                    }
                    var idPersona = res;
                    string queryPropietario =   @"INSERT INTO Propietario (IdPersona, Estado) 
                                                VALUES (@idPersona, @estado)";
                    using (MySqlCommand cmd = new MySqlCommand(queryPropietario, conn))
                        {
                            cmd.Parameters.AddWithValue("@idPersona", idPersona);
                            cmd.Parameters.AddWithValue("@estado", true);
                                await cmd.ExecuteNonQueryAsync();
                        }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Alta Propietario: {ex.Message}");
            }
            
            return res;
        }

        public async Task<int> Baja(int idPropietario)
        {
            throw new NotImplementedException();
        }

        public async Task<int> Editar(Propietario propietario)
        {
            throw new NotImplementedException();
        }

        public async Task<int> ObtenerCantidad()
        {
            throw new NotImplementedException();
        }

        public async Task<Propietario> ObtenerPorDni(int dni)
        {
            throw new NotImplementedException();
        }

        public async Task<IList<Propietario>> ObtenerPorNombre(string nombre)
        {
            throw new NotImplementedException();
        }

        public async Task<IList<Propietario>> ObtenerTodos()
        {
            throw new NotImplementedException();
        }

        public async Task<int> Reestablecer(int idPropietario)
        {
            throw new NotImplementedException();
        }
    }
}