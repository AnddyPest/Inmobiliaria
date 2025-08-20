using System.Data;
using MySql.Data.MySqlClient;
using project.Models;
using project.Models.Interfaces;

namespace project.Services
{
    public class PersonaService(IConfiguration config) : IPersonaService
    {
        private string _connectionString = config.GetConnectionString("Connection") ?? throw new InvalidOperationException("Connection string 'Connection' not found.");

        //ALTA DE PERSONA
        public async Task<int> Alta(Persona persona)
        {
            int res = -1;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    string query = @"INSERT INTO Persona (Nombre, Apellido, Dni, Telefono, Direccion, Email, Estado) 
                                    VALUES (@Nombre, @Apellido, @Dni, @Telefono, @Direccion, @Email, @Estado)";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Nombre", persona.Nombre);
                        cmd.Parameters.AddWithValue("@Apellido", persona.Apellido);
                        cmd.Parameters.AddWithValue("@Dni", persona.Dni);
                        cmd.Parameters.AddWithValue("@Telefono", persona.Telefono);
                        cmd.Parameters.AddWithValue("@Direccion", persona.Direccion);
                        cmd.Parameters.AddWithValue("@Email", persona.Email);
                        cmd.Parameters.AddWithValue("@Estado", true);
                        await conn.OpenAsync();
                        res = await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Alta Persona: {ex.Message}");
            }

            return res;
        }
        //BAJA LOGICA DE PERSONA
        public async Task<int> Baja(int idPersona)
        {
            int res = -1;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    string query = @"UPDATE Persona SET Estado = @Estado WHERE Id = @Id";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", idPersona);
                        cmd.Parameters.AddWithValue("@Estado", false);
                        await conn.OpenAsync();
                        res = await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Baja Persona: {ex.Message}");
            }
            return res;
        }

        //EDICION DE PERSONA
        public async Task<int> Editar(Persona? persona)
        {
            int res = -1;
            if (persona == null)
            {
                throw new ArgumentNullException(nameof(persona), "El parámetro persona no puede ser nulo.");
            }
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    string query = @"UPDATE Persona SET Nombre = @Nombre, Apellido = @Apellido, Dni = @Dni, Telefono = @Telefono, Direccion = @Direccion, Email = @Email WHERE Id = @Id";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Nombre", persona.Nombre);
                        cmd.Parameters.AddWithValue("@Apellido", persona.Apellido);
                        cmd.Parameters.AddWithValue("@Dni", persona.Dni);
                        cmd.Parameters.AddWithValue("@Telefono", persona.Telefono);
                        cmd.Parameters.AddWithValue("@Direccion", persona.Direccion);
                        cmd.Parameters.AddWithValue("@Email", persona.Email);
                        await conn.OpenAsync();
                        res = await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Edicion Persona: {ex.Message}");
            }
            return res;
        }
        // OBTENCION DE CANTIDAD DE PERSONAS
        public async Task<int> ObtenerCantidad()
        {
            int cantidad = 0;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    string query = @"SELECT COUNT(*) FROM Persona WHERE Estado = @Estado";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Estado", true);
                        await conn.OpenAsync();
                        cantidad = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ObtenerCantidad Personas: {ex.Message}");
            }
            return cantidad;
        }
        //FILTRADO POR DNI
        public async Task<Persona?> ObtenerPorDni(int dni)
        {
            Persona? persona = null;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    string query = @"SELECT * FROM Persona WHERE Dni = @Dni AND Estado = @Estado";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Dni", dni);
                        cmd.Parameters.AddWithValue("@Estado", true);
                        await conn.OpenAsync();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                persona = new Persona
                                {
                                    IdPersona = reader.GetInt32("IdPersona"),
                                    Nombre = reader.GetString("Nombre"),
                                    Apellido = reader.GetString("Apellido"),
                                    Dni = reader.GetInt32("Dni"),
                                    Telefono = reader.GetString("Telefono"),
                                    Direccion = reader.GetString("Direccion"),
                                    Email = reader.GetString("Email"),
                                    Estado = reader.GetBoolean("Estado")
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ObtenerPorDni Persona: {ex.Message}");
            }
            return persona;
        }
        //FILTRADO POR NOMBRE
        public async Task<IList<Persona?>> ObtenerPorNombre(string nombre)
        {
            var personas = new List<Persona?>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    string query = @"SELECT * FROM Persona WHERE Nombre LIKE @Nombre AND Estado = @Estado";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Nombre", $"%{nombre}%");
                        cmd.Parameters.AddWithValue("@Estado", true);
                        await conn.OpenAsync();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var persona = new Persona
                                {
                                    IdPersona = reader.GetInt32("IdPersona"),
                                    Nombre = reader.GetString("Nombre"),
                                    Apellido = reader.GetString("Apellido"),
                                    Dni = reader.GetInt32("Dni"),
                                    Telefono = reader.GetString("Telefono"),
                                    Direccion = reader.GetString("Direccion"),
                                    Email = reader.GetString("Email"),
                                    Estado = reader.GetBoolean("Estado")
                                };
                                personas.Add(persona);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ObtenerPorNombre Persona: {ex.Message}");
            }
            return personas;
        }
        //LISTADO GENERAL
        public async Task<IList<Persona?>> ObtenerTodos()
        {
            var personas = new List<Persona?>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    string query = @"SELECT * FROM Persona WHERE Estado = @Estado";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Estado", true);
                        await conn.OpenAsync();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var persona = new Persona
                                {
                                    IdPersona = reader.GetInt32("IdPersona"),
                                    Nombre = reader.GetString("Nombre"),
                                    Apellido = reader.GetString("Apellido"),
                                    Dni = reader.GetInt32("Dni"),
                                    Telefono = reader.GetString("Telefono"),
                                    Direccion = reader.GetString("Direccion"),
                                    Email = reader.GetString("Email"),
                                    Estado = reader.GetBoolean("Estado")
                                };
                                personas.Add(persona);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ObtenerTodos Persona: {ex.Message}");
            }
            return personas;
        }
        //REESTABLECER LOGICO
        public async Task<int> Reestablecer(int idPersona)
        {
            int res = -1;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    string query = @"UPDATE Persona SET Estado = @Estado WHERE IdPersona = @IdPersona";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Estado", true);
                        cmd.Parameters.AddWithValue("@IdPersona", idPersona);
                        await conn.OpenAsync();
                        res = await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Reestablecer Persona: {ex.Message}");
                return res;
            }
            return res;
        }
    }
}