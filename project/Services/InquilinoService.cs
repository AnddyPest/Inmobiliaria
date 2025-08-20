

using Microsoft.AspNetCore.Components.Web;
using MySql.Data.MySqlClient;
using project.Data;
using project.Helpers;
using project.Models;
using project.Models.Interfaces;
using System.Data;
using System.Threading.Tasks;

namespace project.Services
{
    public class InquilinoService : IInquilinoService
    {
        private string _connectionString;

        public InquilinoService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("Connection");
        }

        public async Task<(string?, Inquilino?)> AddInquilino(Inquilino inquilino) //Solo hay que enviarle idPersona y el estado(opcional)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    string query = @"Insert into inquilino(idPersona, estado) 
                                    values (@idPersona, @estado);
                                     Select last_insert_id()";
                    Inquilino inquilinoResponse = new Inquilino();
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@idPersona", inquilino.Persona.IdPersona);
                        command.Parameters.AddWithValue("@estado", inquilino.estado);
                        await connection.OpenAsync();
                        var result = Convert.ToInt32(await command.ExecuteScalarAsync());
                        inquilino.IdInquilino = result;
                        await connection.CloseAsync();
                    }
                    if (inquilinoResponse == null)
                    {
                        return ($"No se pudo agregar el inquilino", null);
                    }
                    return (null, inquilino);
                }
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(InquilinoService), nameof(AddInquilino));
                return ($"Error al agregar el inquilino: {ex}", null);
            }
        }

        public async Task<(string?, List<Inquilino>)> GetAllInquilinos()
        {
            try
            {
                string query = @"Select p.* 
                                from persona p 
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
                                inquilino.IdInquilino = reader.GetInt32("idPersona");
                                inquilino.Nombre = reader.GetString("Nombre");
                                inquilino.Apellido = reader.GetString("Apellido");
                                inquilino.Dni = reader.GetInt32("Dni");
                                inquilino.Telefono = reader.GetInt64("Telefono");
                                inquilino.Direccion = reader.GetString("Direccion");
                                inquilino.Email = reader.GetString("Email");
                                inquilino.estado = reader.GetBoolean("estado");
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
                return ($"Error al obtener los inquilinos: {ex}", null);
            }
        }

        public async Task<(string?, Inquilino?)> GetInquilinoById(int idInquilino)
        {
            try
            {
                string query = @$"select p.*
                                from personas as p
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
                                inquilinoFromDatabase.IdInquilino = reader.GetInt32("idPersona");
                                inquilinoFromDatabase.Nombre = reader.GetString("Nombre");
                                inquilinoFromDatabase.Apellido = reader.GetString("Apellido");
                                inquilinoFromDatabase.Dni = reader.GetInt32("Dni");
                                inquilinoFromDatabase.Telefono = reader.GetInt64("Telefono");
                                inquilinoFromDatabase.Direccion = reader.GetString("Direccion");
                                inquilinoFromDatabase.Email = reader.GetString("Email");
                                inquilinoFromDatabase.estado = reader.GetBoolean("estado");

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

        public async Task<(string?, bool?)> LogicalDeleteInquilino(int idInquilino,bool estado)
        {
            try
            {
                bool validation = false;
                using (var connection = new MySqlConnection(_connectionString)) 
                {
                    string query = @"Update inquilino 
                                    set estado=@estadoNuevo
                                    where idInquilino= @idInquilino";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@estadoNuevo", estado);
                        command.Parameters.AddWithValue("@idInquilino", idInquilino);
                        await connection.OpenAsync();
                        var res = await command.ExecuteNonQueryAsync();
                        await connection.CloseAsync();
                        if(res  != 0)
                        {
                            validation = true;
                        }
                    }
                }
                return (null, validation);
            }catch(Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(InquilinoService), nameof(LogicalDeleteInquilino));
                return ($"Error al dar la baja logica al inquilino con id: {idInquilino}", false);
            }
        }

    }
}
