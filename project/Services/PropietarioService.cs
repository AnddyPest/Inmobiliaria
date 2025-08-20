using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using project.Models;
using project.Models.Interfaces;

namespace project.Services
{
    public class PropietarioService(IConfiguration config) : IPropietarioService
    {
        private string _connectionString = config.GetConnectionString("Connection")!;

        //ALTA PROPIETARIO
        public async Task<int> Alta(Propietario propietario)
        {
            int res = -1;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    string query = @"INSERT INTO propietario (idPersona, estado) VALUES (@idPersona, @estado);
                                     SELECT LAST_INSERT_ID();";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@idPersona", propietario.Persona.IdPersona);
                        cmd.Parameters.AddWithValue("@estado", true);
                        await conn.OpenAsync();
                        res = await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Alta Propietario: {ex.Message}");

            }
            return res;
        }

        //BAJA LOGICA PROPIETARIO
        public async Task<int> Baja(int idPropietario)
        {
            int res = -1;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    string query = @"UPDATE propietario SET estado = false WHERE idPropietario = @idPropietario;";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@idPropietario", idPropietario);
                        await conn.OpenAsync();
                        res = await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Baja Propietario: {ex.Message}");
            }
            return res;
        }

        public Task<int> Editar(Propietario propietario)
        {
            throw new NotImplementedException();
        }

        public async Task<int> ObtenerCantidad()
        {
            int count = 0;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    string query = @"SELECT COUNT(*) FROM propietario WHERE estado = true;";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        await conn.OpenAsync();
                        count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ObtenerCantidad Propietario: {ex.Message}");
            }
            return count;
        }


        public Task<Propietario> ObtenerPorDni(int dni)
        {
            throw new NotImplementedException();
        }

        public Task<IList<Propietario>> ObtenerPorNombre(string nombre)
        {
            throw new NotImplementedException();
        }

        //OBTENER TODOS LOS PROPIETARIOS JOINT PERSONA POR ID PERSONA
        public async Task<IList<Propietario>> ObtenerTodos()
        {
            IList<Propietario> propietarios = new List<Propietario>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    string query = @"SELECT p.*, per.* FROM propietario p
                                     JOIN persona per ON p.idPersona = per.idPersona
                                     WHERE p.estado = true;";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        await conn.OpenAsync();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Propietario propietario = new Propietario
                                {
                                    IdPropietario = reader.GetInt32("idPropietario"),
                                    Persona = new Persona
                                    {
                                        IdPersona = reader.GetInt32("idPersona"),
                                        Nombre = reader.GetString("nombre"),
                                        Apellido = reader.GetString("apellido"),
                                        Dni = reader.GetInt32("dni")
                                    }
                                };
                                propietarios.Add(propietario);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ObtenerTodos Propietario: {ex.Message}");
            }
            return propietarios;
        }

        //REESTABLECIMIENTO LOGICO
        public async Task<int> Reestablecer(int idPropietario)
        {
            int res = 0;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    string query = @"UPDATE propietario SET estado = true WHERE idPropietario = @idPropietario;";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@idPropietario", idPropietario);
                        await conn.OpenAsync();
                        res = await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Reestablecer Propietario: {ex.Message}");
            }
            return res;
        }
    }
}