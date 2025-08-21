using MySql.Data.MySqlClient;
using project.Helpers;
using project.Models;
using project.Models.Interfaces;
using System.Data;
using System.Threading.Tasks;

namespace project.Services
{
    public class PropietarioService(IConfiguration config, IPersonaService personaService) : IPropietarioService
    {
        private string _connectionString = config.GetConnectionString("Connection") ?? throw new InvalidOperationException("Connection string 'Connection' not found.");
        private IPersonaService personaService = personaService;

        public async Task<(string?, Propietario?)> getPropietarioById(int idPropietario)
        {
            try
            {
                if (idPropietario <= 0) return ("El ID del propietario debe ser mayor que 0.", null);
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = @"SELECT p.*, propi.estado as estado_propietario, propi.idPropietario
                                    FROM persona as p
                                    INNER JOIN propietario propi ON p.idPersona = propi.idPersona
                                    WHERE propi.idPropietario = @idPropietario";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@idPropietario", idPropietario);
                        await connection.OpenAsync();
                        Propietario propietarioFromDatabase = new Propietario();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                propietarioFromDatabase.IdPropietario = reader.GetInt32("idPropietario");
                                propietarioFromDatabase.IdPersona = reader.GetInt32("idPersona");
                                propietarioFromDatabase.Nombre = reader.GetString("Nombre");
                                propietarioFromDatabase.Apellido = reader.GetString("Apellido");
                                propietarioFromDatabase.Dni = reader.GetInt32("Dni");
                                propietarioFromDatabase.Telefono = reader.GetString("Telefono");
                                propietarioFromDatabase.Direccion = reader.GetString("Direccion");
                                propietarioFromDatabase.Email = reader.GetString("Email");
                                propietarioFromDatabase.Estado = reader.GetBoolean("estado");
                                propietarioFromDatabase.EstadoPropietario = reader.GetBoolean("estado_propietario");
                            }
                        }
                        if (propietarioFromDatabase.IdPropietario == 0 || propietarioFromDatabase == null)
                        {
                            return ($"No se encontró un propietario con ID {idPropietario}", null);
                        }
                        await connection.CloseAsync();
                        return (null, propietarioFromDatabase);
                    }
                }
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(PropietarioService), nameof(getPropietarioById));
                return (ex.Message, null);
            } 
        }


        public async Task<(string?, bool)> validarQueNoEsteAgregadoElPropietario(int idPersona)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = @"SELECT * FROM propietario WHERE idPersona = @idPersona";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {

                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@idPersona", idPersona);
                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {

                            if (await reader.ReadAsync())
                            {
                                return ("Ya existe un registro", false);
                            }
                        }
                        await connection.CloseAsync();
                    }
                }
                return (null, true);
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(PropietarioService), nameof(validarQueNoEsteAgregadoElPropietario));
                return (ex.Message, false);
            }
        }
        public async Task<(string?, Propietario?)> getPropietarioByIdPersona(int idPersona) //testear
        {
            try
            {
                if (idPersona <= 0)
                {
                    return ("El ID de la persona debe ser mayor que 0.", null);
                }
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = @"SELECT p.*, propi.estado as estado_propietario, propi.idPropietario
                                    FROM persona as p
                                    INNER JOIN propietario propi ON p.idPersona = propi.idPersona
                                    WHERE p.idPersona = @idPersona";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@idPersona", idPersona);
                        await connection.OpenAsync();
                        Propietario propietarioFromDatabase = new Propietario();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                propietarioFromDatabase.IdPropietario = reader.GetInt32("idPropietario");
                                propietarioFromDatabase.IdPersona = reader.GetInt32("idPersona");
                                propietarioFromDatabase.Nombre = reader.GetString("Nombre");
                                propietarioFromDatabase.Apellido = reader.GetString("Apellido");
                                propietarioFromDatabase.Dni = reader.GetInt32("Dni");
                                propietarioFromDatabase.Telefono = reader.GetString("Telefono");
                                propietarioFromDatabase.Direccion = reader.GetString("Direccion");
                                propietarioFromDatabase.Email = reader.GetString("Email");
                                propietarioFromDatabase.Estado = reader.GetBoolean("estado");
                                propietarioFromDatabase.EstadoPropietario = reader.GetBoolean("estado_propietario");
                            }
                        }
                        if (propietarioFromDatabase.IdPropietario == 0 || propietarioFromDatabase == null)
                        {
                            return ($"No se encontró un propietario con ID de persona {idPersona}", null);
                        }
                        await connection.CloseAsync();
                        return (null, propietarioFromDatabase);
                    }
                }
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(PropietarioService), nameof(getPropietarioByIdPersona));
                return (ex.Message, null);
            }
        }

        //ALTA PROPIETARIO
        public async Task<int> Alta(int idPersona)
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
                        cmd.Parameters.AddWithValue("@idPersona", idPersona);
                        cmd.Parameters.AddWithValue("@estado", true);
                        await conn.OpenAsync();
                        object? idGenerated = await cmd.ExecuteScalarAsync();
                        res = Convert.ToInt32(idGenerated);
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
        public async Task<int> BajaLogica(int idPropietario)
        {
            int res = -1;
            try
            {
                (string?,Propietario?) propietario = await this.getPropietarioById(idPropietario);
                if(propietario.Item1 != null)
                {
                    Console.WriteLine(propietario.Item1);
                    return res;
                }
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


        public async Task<(string?, Propietario?)> getPropietarioPorDni(int dni)
        {
            if (dni <= 0) return ("El dni debe ser mayor que 0", null);
            try
            {
                Persona? persona = await personaService.ObtenerPorDni(dni);
                if (persona == null) return ("No se encuentra una persona registrada con dicho dni", null);
                using(MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = @"Select p.*, prop.estado as estado_propietario, prop.idPropietario 
                                    from persona as p
                                    inner join propietario as prop on p.idPersona = prop.idPersona
                                    where p.idPersona = @idPersona";
                    using(MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@idPersona", persona.IdPersona);
                        await connection.OpenAsync();
                        Propietario propietario = null;
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                propietario = new Propietario();
                                propietario.IdPropietario = reader.GetInt32("idPropietario");
                                propietario.IdPersona = reader.GetInt32("idPersona");
                                propietario.Nombre = reader.GetString("nombre");
                                propietario.Apellido = reader.GetString("Apellido");
                                propietario.Dni = reader.GetInt32("Dni");
                                propietario.Telefono = reader.GetString("Telefono");
                                propietario.Direccion = reader.GetString("Direccion");
                                propietario.Email = reader.GetString("Email");
                                propietario.Estado = reader.GetBoolean("estado");
                                propietario.EstadoPropietario = reader.GetBoolean("estado_propietario");
                            }
                            if (propietario == null) return ("No se encuentra un propietario con dicho dni", null);
                            return (null,propietario);
                        }
                    }
                }
            }catch(Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(PropietarioService), nameof(getPropietarioPorDni));
                return (ex.Message, null);
            }
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
        public async Task<int> AltaLogica(int idPropietario)
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