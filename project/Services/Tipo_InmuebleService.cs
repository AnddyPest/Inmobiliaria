using MySql.Data.MySqlClient;
using project.Helpers;
using project.Models;
using System.Data.Common;
using System.Data;
using project.Models.Interfaces;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
namespace project.Services
{
    public class Tipo_InmuebleService(IConfiguration configuration) : ITipo_InmuebleService
    {
        private string _connectionString = configuration.GetConnectionString("Connection")!;

        public async Task<(string?, List<Tipo_Inmueble>?)> getAllTipoInmueble()
        {
            try
            {
                List<Tipo_Inmueble> listaTiposInmuebles = new();
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = @$"Select * from tipo_inmueble";
                    await connection.OpenAsync();
                    using (MySqlCommand command = new(query, connection))
                    {
                        using (DbDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Tipo_Inmueble tipoInmueble = new();
                                tipoInmueble.id_tipo_inmueble = reader.GetInt32("id_tipo_inmueble");
                                tipoInmueble.nombre = reader.GetString("nombre");
                                listaTiposInmuebles.Add(tipoInmueble);
                            }
                            if (listaTiposInmuebles.Count == 0)
                            {
                                HelperFor.imprimirMensajeDeError("No hay tipos de inmuebles en la base de datos", nameof(Tipo_InmuebleService), nameof(getAllTipoInmueble));
                                return ("No hay tipos de inmuebles en la base de datos", null);
                            }
                            return (null, listaTiposInmuebles);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(Tipo_InmuebleService), nameof(getAllTipoInmueble));
                return (ex.Message, null);
            }
        }
        public async Task<(string?, List<Tipo_Inmueble>?)> getAllTipoInmueble(int nroPagina, int cantidadPaginasPorHoja)
        {
            try
            {
                List<Tipo_Inmueble> listaTiposInmuebles = new();
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = @$"Select * from tipo_inmueble
                                     Limit {cantidadPaginasPorHoja} OFFSET {(nroPagina - 1) * cantidadPaginasPorHoja} ";
                    await connection.OpenAsync();
                    using (MySqlCommand command = new(query, connection))
                    {
                        using (DbDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Tipo_Inmueble tipoInmueble = new();
                                tipoInmueble.id_tipo_inmueble = reader.GetInt32("id_tipo_inmueble");
                                tipoInmueble.nombre = reader.GetString("nombre");
                                listaTiposInmuebles.Add(tipoInmueble);
                            }
                            if (listaTiposInmuebles.Count == 0)
                            {
                                HelperFor.imprimirMensajeDeError("No hay tipos de inmuebles en la base de datos", nameof(Tipo_InmuebleService), nameof(getAllTipoInmueble));
                                return ("No hay tipos de inmuebles en la base de datos", null);
                            }
                            return (null, listaTiposInmuebles);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(Tipo_InmuebleService), nameof(getAllTipoInmueble));
                return (ex.Message, null);
            }
        }
        public async Task<(string?, Tipo_Inmueble?)> buscarTipoInmueblePorId(int idTipoInmueble)
        {
            if (idTipoInmueble <= 0)
                return ("El id_tipo_inmueble debe ser mayor a 0", null);
            try
            {
                using(MySqlConnection connection = new (_connectionString))
                {
                    string query = @"   Select * 
                                        from tipo_inmueble 
                                        where id_tipo_inmueble = @id_tipo_inmueble";
                    using(MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@id_tipo_inmueble", idTipoInmueble);
                        await connection.OpenAsync();
                        using(var reader = await command.ExecuteReaderAsync())
                        {
                            Tipo_Inmueble? tipo_Inmueble = null;
                            if(await reader.ReadAsync())
                            {
                                tipo_Inmueble = new();
                                tipo_Inmueble.id_tipo_inmueble = reader.GetInt32("id_tipo_inmueble");
                                tipo_Inmueble.nombre = reader.GetString("nombre");
                            }
                            await connection.CloseAsync();
                            if(tipo_Inmueble == null)
                            {
                                return ($"No se ha encontrado un tipoInmueble registrado con id: {idTipoInmueble}", null);
                            }
                            return (null, tipo_Inmueble);
                        }
                    }
                }
            }catch(Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(Tipo_InmuebleService), nameof(buscarTipoInmueblePorId));
                return (ex.Message, null);
            }
        }

        public async Task<(string?, Tipo_Inmueble?)> buscarTipoInmueblePorNombre(string nombre)
        {
            if (nombre.Length < 3)
                return ("El nombre debe ser mayor a 3 caracteres", null);
            try
            {
                using (MySqlConnection connection = new(_connectionString))
                {
                    string query = @"   Select * 
                                        from tipo_inmueble 
                                        where nombre = @nombre";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@nombre", nombre);
                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            Tipo_Inmueble? tipo_Inmueble = null;
                            if (await reader.ReadAsync())
                            {
                                tipo_Inmueble = new();
                                tipo_Inmueble.id_tipo_inmueble = reader.GetInt32("id_tipo_inmueble");
                                tipo_Inmueble.nombre = reader.GetString("nombre");
                            }
                            await connection.CloseAsync();
                            if (tipo_Inmueble == null)
                            {
                                return ($"No se ha encontrado un tipoInmueble registrado con el nombre: {nombre}", null);
                            }
                            return (null, tipo_Inmueble);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(Tipo_InmuebleService), nameof(buscarTipoInmueblePorId));
                return (ex.Message, null);
            }
        }

        public async Task<(string?, int?)> cantidadRegistros()
        {
            try
            {
                int response = 0;
                using(MySqlConnection connection = new(_connectionString))
                {
                    string query = @" Select Count(id_tipo_inmueble) from tipo_inmueble ";
                    using(MySqlCommand command = new(query, connection))
                    {
                        command.CommandType = CommandType.Text;
                        await connection.OpenAsync();
                        DbDataReader reader = await command.ExecuteReaderAsync();
                        if(await reader.ReadAsync())
                        {
                            response = reader.GetInt32(0);
                        }
                        await connection.CloseAsync();
                   
                        return (null, response);
                    }
                }
            }catch(Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(Tipo_InmuebleService), nameof(cantidadRegistros));
                return (ex.Message, null);
            }
        }

        public async Task<(string?, bool)> createTipoInmueble(Tipo_Inmueble tipo_Inmueble)
        {
            try
            {
                if (await buscarTipoInmueblePorNombre(tipo_Inmueble.nombre) is (null, Tipo_Inmueble tipo) && tipo.nombre.ToLower() == tipo_Inmueble.nombre.ToLower())
                    return ("Ya existe un tipo_inmueble con nombre: " + tipo_Inmueble.nombre , false);
                using(MySqlConnection connection = new(_connectionString))
                {
                    string query = $@" Insert into tipo_inmueble(nombre)
                                       Values (@nombre);
                                       SELECT LAST_INSERT_ID();";
                    using(MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        await connection.OpenAsync();
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@nombre", tipo_Inmueble.nombre);
                        var result = await command.ExecuteScalarAsync();
                        if(result != null && int.TryParse(result.ToString(), out int idTipoInmueble))
                        {
                            return (null, true);
                        }
                        await connection.CloseAsync();
                        return ("No se registro un tipo_inmueble", false);
                    }
                }
            }catch(Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(Tipo_InmuebleService), nameof(createTipoInmueble));
                return (ex.Message, false);
            }
        }

        public async Task<(string?, bool)> deleteTipoInmueble(int idTipoInmueble)
        {
            try
            {
                if (idTipoInmueble < 1)
                    return ("El id_tipo_inmueble debe ser mayor a 0", false);
                if (await buscarTipoInmueblePorId(idTipoInmueble) is (string error, null))
                    return (error, false);
                if (await ValidarQueNoEsteAsignado(idTipoInmueble) is (string errorValidacion, bool validacion) && !validacion)
                    return (errorValidacion, false);
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                    {
                        string query = @" Delete from tipo_inmueble 
                                      Where id_tipo_inmueble = @idTipoInmueble";
                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            command.CommandType = CommandType.Text;
                            command.Parameters.AddWithValue("@idTipoInmueble", idTipoInmueble);
                            await connection.OpenAsync();
                            int filasAfectadas = await command.ExecuteNonQueryAsync();
                            if (filasAfectadas > 0)
                                return (null, true);
                            return ("No se elimino el registro", false);
                        }
                    }
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(Tipo_InmuebleService), nameof(deleteTipoInmueble));
                return (ex.Message, false);
            }
        }

        public async Task<(string?, bool)> ValidarQueNoEsteAsignado(int id_tipo_inmueble)

        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string sql = @$"Select count(idInmueble) from inmueble where id_tipo_inmueble = @id_tipo_inmueble";
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@id_tipo_inmueble", id_tipo_inmueble);
                        await connection.OpenAsync();
                        int? filasAfectadas = Convert.ToInt32(await command.ExecuteScalarAsync());
                        if (filasAfectadas == 0 || filasAfectadas == null)
                            return (null, true);
                        return ($"El registro esta asignado a {filasAfectadas} inmuebles. Debe modificar los inmuebles que tengan asignado dicho tipo de inmueble para poder eliminarlo.", false);
                    }
                }
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(Tipo_InmuebleService), nameof(ValidarQueNoEsteAsignado));
                return (ex.Message, false);
            }

        }

        public async Task<(string?, bool)> updateTipoInmueble(Tipo_Inmueble tipo_Inmueble)
        {
            if (tipo_Inmueble.id_tipo_inmueble < 1)
                return ("El id_tipo_inmueble debe ser mayor a 0", false);
            try
            {
                if (await buscarTipoInmueblePorId(tipo_Inmueble.id_tipo_inmueble) is (string error, null))
                    return (error, false);
                if (await buscarTipoInmueblePorNombre(tipo_Inmueble.nombre) is (null, Tipo_Inmueble tipoInmuebleSearched) && tipoInmuebleSearched.id_tipo_inmueble != tipo_Inmueble.id_tipo_inmueble)
                    return ($"Ya existe un registro con nombre: {tipo_Inmueble.nombre}", false);
                using(MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = @"UPDATE tipo_inmueble
                                     set nombre= @nombre
                                     where id_tipo_inmueble = @idTipoInmueble";
                    using(MySqlCommand command = new(query, connection))
                    {
                        await connection.OpenAsync();
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@nombre", tipo_Inmueble.nombre);
                        command.Parameters.AddWithValue("@idTipoInmueble", tipo_Inmueble.id_tipo_inmueble);
                        int filasAfectadas = await command.ExecuteNonQueryAsync();
                        if (filasAfectadas >= 1)
                            return (null, true);
                        return ($"No se actualizo el tipo_inmueble con nombre: {tipo_Inmueble.nombre}", false);
                    }
                }
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(Tipo_InmuebleService), nameof(updateTipoInmueble));
                return (ex.Message, false);
            }
        }
    }
    
}
