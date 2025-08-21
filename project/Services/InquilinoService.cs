using Microsoft.AspNetCore.Components.Web;
using MySql.Data.MySqlClient;

using project.Helpers;
using project.Models;
using project.Models.Interfaces;
using System.Data;
using System.Threading.Tasks;

namespace project.Services
{
    public class InquilinoService(IConfiguration config) : IInquilinoService
    {
        private readonly string _connectionString = config.GetConnectionString("Connection") ?? throw new System.InvalidOperationException("Connection string 'Connection' not found.");

        public async Task<(string?, bool)> validarQueNoEsteAgregadoElInquilino(int idPersona)
        {
            try
            {
                using(MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = @"SELECT * FROM inquilino WHERE idPersona = @idPersona";
                    using(MySqlCommand command = new MySqlCommand(query, connection))
                    {

                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@idPersona", idPersona);
                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            
                            if(await reader.ReadAsync())
                            {
                                return ("Ya existe un registro", false);
                            }
                        }
                        await connection.CloseAsync();
                    }
                }
                return (null, true);
            }catch(Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(InquilinoService), nameof(validarQueNoEsteAgregadoElInquilino));
                return (ex.Message, false);
            }
        }
        public async Task<(string?, Inquilino?)> getInquilinoByIdPersona(int idPersona)
        {
            try
            {
                if(idPersona <= 0)
                {
                    return ("El ID de la persona debe ser mayor que 0.", null);
                }
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = @"SELECT p.*, i.estado as estado_inquilino, i.idInquilino
                                    FROM persona as p
                                    INNER JOIN inquilino i ON p.idPersona = i.idPersona
                                    WHERE p.idPersona = @idPersona";
                    using(MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@idPersona", idPersona);
                        await connection.OpenAsync();
                        Inquilino inquilinoFromDatabase = new Inquilino();
                        using(var reader = await command.ExecuteReaderAsync())
                        {
                            if(await reader.ReadAsync())
                            {
                                inquilinoFromDatabase.IdInquilino = reader.GetInt32("idInquilino");
                                inquilinoFromDatabase.IdPersona = reader.GetInt32("idPersona");
                                inquilinoFromDatabase.Nombre = reader.GetString("Nombre");
                                inquilinoFromDatabase.Apellido = reader.GetString("Apellido");
                                inquilinoFromDatabase.Dni = reader.GetInt32("Dni");
                                inquilinoFromDatabase.Telefono = reader.GetString("Telefono");
                                inquilinoFromDatabase.Direccion = reader.GetString("Direccion");
                                inquilinoFromDatabase.Email = reader.GetString("Email");
                                inquilinoFromDatabase.Estado = reader.GetBoolean("estado");
                                inquilinoFromDatabase.EstadoInquilino = reader.GetBoolean("estado_inquilino");
                            }
                        }
                        if(inquilinoFromDatabase.IdInquilino == 0 || inquilinoFromDatabase == null)
                        {
                            return ($"No se encontró un inquilino con ID de persona {idPersona}", null);
                        }
                        await connection.CloseAsync();
                        return (null, inquilinoFromDatabase);
                    }
                }
            }
            catch(Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(InquilinoService), nameof(getInquilinoByIdPersona));
                return (ex.Message, null);
            }
        }
        public async Task<(string?, Inquilino?)> AddInquilino(Persona persona) //Solo hay que enviarle idPersona y el estado(opcional)
        {
            try
            {
                 
                using (var connection = new MySqlConnection(_connectionString))
                {
                    string query = @"Insert into inquilino(idPersona, estado) 
                                    values (@idPersona, @estado);
                                     Select last_insert_id()";
                    
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@idPersona", persona.IdPersona);
                        command.Parameters.AddWithValue("@estado", true);
                        await connection.OpenAsync();
                        var result = Convert.ToInt32(await command.ExecuteScalarAsync());
                        Inquilino inquilino = new Inquilino
                        {
                            IdInquilino = result,
                            IdPersona = persona.IdPersona,
                            Nombre = persona.Nombre,
                            Apellido = persona.Apellido,
                            Dni = persona.Dni,
                            Telefono = persona.Telefono,
                            Direccion = persona.Direccion,
                            Email = persona.Email,
                            EstadoInquilino = persona.Estado,
                            Estado = true
                            
                        };
                        
                        await connection.CloseAsync();
                        
                        return (null, inquilino);
                    }
                    
                    
                }
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(InquilinoService), nameof(AddInquilino));
                return ($"Error al agregar el inquilino: {ex}", null);
            }
        }

        public async Task<(string?, List<Inquilino>)> GetAllInquilinos() //Testeado y funcional
        {
            try
            {
                string query = @"Select p.*, i.estado as estado_inquilino, i.idInquilino
                                from persona as p 
                                inner join inquilino i On p.idPersona = i.idPersona";
                using (MySqlConnection conexion = new MySqlConnection(_connectionString))
                {
                    await conexion.OpenAsync();
                    using (MySqlCommand command = new MySqlCommand(query, conexion))
                    {
                        List<Inquilino> inquilinos = new List<Inquilino>();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Inquilino inquilino = new Inquilino();
                                inquilino.IdInquilino = reader.GetInt32("idInquilino");
                                inquilino.IdPersona= reader.GetInt32("idPersona");
                                inquilino.Nombre = reader.GetString("Nombre");
                                inquilino.Apellido = reader.GetString("Apellido");
                                inquilino.Dni = reader.GetInt32("Dni");
                                inquilino.Telefono = reader.GetString("Telefono");
                                inquilino.Direccion = reader.GetString("Direccion");
                                inquilino.Email = reader.GetString("Email");
                                inquilino.Estado = reader.GetBoolean("estado");
                                inquilino.EstadoInquilino = reader.GetBoolean("estado_Inquilino");
                                inquilinos.Add(inquilino);
                            }

                        }
                        await conexion.CloseAsync();
                        Console.WriteLine("Funciono");
                        Console.WriteLine(inquilinos);
                        return (null, inquilinos);
                    }
                }
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(InquilinoService), nameof(GetAllInquilinos));
                return ($"Error al obtener los inquilinos: {ex}", new List<Inquilino>());
            }
        }

        public async Task<(string?, Inquilino?)> GetInquilinoById(int idInquilino) //testeado y funcional
        {
            try
            {
                string query = @$"select p.*, i.estado as estado_inquilino, i.idInquilino
                                from persona as p
                                inner join inquilino i on p.idPersona = i.idPersona
                                where i.idInquilino = @idInquilino";

                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (MySqlCommand commando = new MySqlCommand(query, connection))
                    {
                        Inquilino inquilinoFromDatabase = new Inquilino();
                        commando.Parameters.AddWithValue("@idInquilino", idInquilino);
                        using (var reader = await commando.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                inquilinoFromDatabase.IdInquilino = reader.GetInt32("idInquilino");
                                inquilinoFromDatabase.IdPersona = reader.GetInt32("idPersona");
                                inquilinoFromDatabase.Nombre = reader.GetString("Nombre");
                                inquilinoFromDatabase.Apellido = reader.GetString("Apellido");
                                inquilinoFromDatabase.Dni = reader.GetInt32("Dni");
                                inquilinoFromDatabase.Telefono = reader.GetString("Telefono");
                                inquilinoFromDatabase.Direccion = reader.GetString("Direccion");
                                inquilinoFromDatabase.Email = reader.GetString("Email");
                                inquilinoFromDatabase.Estado = reader.GetBoolean("estado");
                                inquilinoFromDatabase.EstadoInquilino = reader.GetBoolean("estado_inquilino");

                            }
                        }
                        if (inquilinoFromDatabase.IdInquilino == 0 || inquilinoFromDatabase == null)
                        {
                            return ($"No se encontró un inquilino con ID {idInquilino}", null);
                        }
                        await connection.CloseAsync();
                        return (null, inquilinoFromDatabase);
                    }
                }
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(InquilinoService), nameof(GetInquilinoById));
                return ($"Error al obtener el inquilino con ID {idInquilino}: {ex}", null);
            }

        }

        public async Task<(string?, bool?)> LogicalDeleteInquilino(int idInquilino)
        {
            if (idInquilino <= 0) return ("El ID del inquilino debe ser mayor que 0.", null);
            try
            {
                (string?, Inquilino?) inquilino = await this.GetInquilinoById(idInquilino);
                if (inquilino.Item1 != null) return ($"El inquilino no se encuentra registrado con el Id: {idInquilino}", null);

                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                const string sql = "UPDATE inquilino SET estado = false WHERE idInquilino = @id;";
                await using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", idInquilino);

                var rows = await cmd.ExecuteNonQueryAsync();
                return (null, rows > 0);
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(InquilinoService), nameof(GetInquilinoById));
                return ($"Error al obtener el inquilino con ID {idInquilino}: {ex}", null);
            }
        }
        public async Task<(string?,bool?)> AltaLogicaInquilino(int idInquilino)
        {
            if (idInquilino <= 0) return ("El ID del inquilino debe ser mayor que 0.", null);
            try
            {
                (string?, Inquilino?) inquilino = await this.GetInquilinoById(idInquilino);
                if (inquilino.Item1 != null) return ($"El inquilino no se encuentra registrado con el Id: {idInquilino}", null);
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                const string sql = "UPDATE inquilino SET estado = true WHERE idInquilino = @id;";
                await using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", idInquilino);

                var rows = await cmd.ExecuteNonQueryAsync();
                return (null, rows > 0);
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(InquilinoService), nameof(AltaLogicaInquilino));
                return (ex.Message, null);
            }
        }

    }
}
