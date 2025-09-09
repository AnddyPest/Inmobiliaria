using MySql.Data.MySqlClient;
using project.Helpers;
using project.Models;
using System.Data.Common;
using System.Data;
using project.Models.Interfaces;
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
                using(MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = @"Select * from tipo_inmueble";
                    await connection.OpenAsync();
                    using (MySqlCommand command = new (query, connection))
                    {
                        using(DbDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while(await reader.ReadAsync())
                            {
                                Tipo_Inmueble tipoInmueble = new();
                                tipoInmueble.id_tipo_inmueble = reader.GetInt32("id_tipo_inmueble");
                                tipoInmueble.nombre = reader.GetString("nombre");
                                listaTiposInmuebles.Add(tipoInmueble);
                            }
                            if(listaTiposInmuebles.Count == 0)
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

    }
}
