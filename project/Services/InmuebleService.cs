using MySql.Data.MySqlClient;
using project.Helpers;
using project.Models;
using project.Models.Interfaces;
using System.Data;
using System.Data.Common;

namespace project.Services
{
    public class InmuebleService(IConfiguration configuration) : IInmuebleService
    {
        private readonly string _connectionString = configuration.GetConnectionString("Connection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        public async Task<(string?, Inmueble?)> AgregarInmueble(Inmueble inmueble)
        {
            throw new NotImplementedException();
        }

        public Task<(string?, bool)> ActualizarInmueble(Inmueble inmueble)
        {
            throw new NotImplementedException();
        }

        public Task<(string?, bool)> DarDeBajaInmueble(int idInmueble)
        {
            throw new NotImplementedException();
        }

        public async Task<(string?, Inmueble?)> ObtenerInmueblePorContrato(int idContrato)
        {
            throw new NotImplementedException();
        }

        public async Task<(string?, Inmueble?)> ObtenerInmueblePorId(int idInmueble)
        {
            try
            {
                if (idInmueble <= 0) return ("El id del inmueble debe ser un valor positivo", null);
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @" SELECT i.* ,ti.*
                                      FROM inmueble as i
                                      inner join tipo_inmueble as ti on ti.id_tipo_inmueble = i.id_tipo_inmueble
                                      WHERE IdInmueble = @IdInmueble ";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdInmueble", idInmueble);
                        using (DbDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                Inmueble inmueble = new Inmueble();
                                Tipo_Inmueble tipo = new Tipo_Inmueble();
                                inmueble.IdInmueble = reader.GetInt32("IdInmueble");
                                inmueble.Uso = reader.GetString("Uso");
                                inmueble.Superficie = reader.GetInt32("Superficie");
                                inmueble.CantAmbientes = reader.GetInt32("CantAmbientes");
                                inmueble.Coordenadas = reader.GetDecimal("Coordenadas");
                                inmueble.Precio = reader.GetDecimal("Precio");
                                inmueble.Direccion = reader.GetString("Direccion");
                                inmueble.ciudad = reader.GetString("ciudad");
                                inmueble.IdPropietario = reader.GetInt32("IdPropietario");
                                inmueble.Disponible = reader.GetBoolean("Disponible");
                                inmueble.estado = reader.GetBoolean("estado");
                                tipo.id_tipo_inmueble = reader.GetInt32("id_tipo_inmueble");
                                tipo.nombre = reader.GetString("nombre");
                                inmueble.Tipo = tipo;
                                return (null, inmueble);
                            }
                            return ($"No se encontró ningún inmueble con el id {idInmueble}", null);
                            
                        }
                        
                    }
                }
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(InmuebleService), nameof(ObtenerInmueblePorId));
                return (ex.Message, null);
            }
        }

        public async Task<(string?, List<Inmueble>?)> ObtenerInmueblesPorPropietario(int dniPropietario)
        {
            try
            {
                if(dniPropietario <= 0) return ("El dni del propietario debe ser un valor positivo", null);
                List<Inmueble> inmuebles = new List<Inmueble>();
                using(MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @" SELECT i.* ,ti.*, p.*
                                      FROM inmueble as i
                                      inner join tipo_inmueble as ti on ti.id_tipo_inmueble = i.id_tipo_inmueble
                                      inner join propietario as p on p.idPropietario = i.idPropietario
                                      WHERE p.dni = @dniPropietario ";
                    using(MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@dniPropietario", dniPropietario);
                        using(DbDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while(await reader.ReadAsync())
                            {
                                Inmueble inmueble = new Inmueble();
                                Tipo_Inmueble tipo = new Tipo_Inmueble();
                                Propietario propietario = new Propietario();
                                inmueble.IdInmueble = reader.GetInt32("IdInmueble");
                                inmueble.Uso = reader.GetString("Uso");
                                inmueble.Superficie = reader.GetInt32("Superficie");
                                inmueble.CantAmbientes = reader.GetInt32("CantAmbientes");
                                inmueble.Coordenadas = reader.GetDecimal("Coordenadas");
                                inmueble.Precio = reader.GetDecimal("Precio");
                                inmueble.Direccion = reader.GetString("Direccion");
                                inmueble.ciudad = reader.GetString("ciudad");
                                inmueble.IdPropietario = reader.GetInt32("IdPropietario");
                                inmueble.Disponible = reader.GetBoolean("Disponible");
                                inmueble.estado = reader.GetBoolean("estado");
                                tipo.id_tipo_inmueble = reader.GetInt32("ti.id_tipo_inmueble");
                                tipo.nombre = reader.GetString("ti.nombre");
                                propietario.IdPropietario = reader.GetInt32("p.idPropietario");
                                inmueble.Tipo = tipo;
                                inmuebles.Add(inmueble);
                            }
                            await connection.CloseAsync();
                            if (inmuebles.Count == 0) return ($"No se encontraron inmuebles para el propietario con dni {dniPropietario}", null);
                            
                            return (null, inmuebles);
                            
                        }
                        
                    }
                }
            }
            catch(Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(InmuebleService), nameof(ObtenerInmueblesPorPropietario));
                return (ex.Message, null);
            }
        }

        public Task<(string?, List<Inmueble>?)> ObtenerTodosLosInmuebles()
        {
            throw new NotImplementedException();
        }

        public Task<(string?, Inmueble?)> BuscarInmueblePorDireccion(string direccion)
        {
            throw new NotImplementedException();
        }
    }
}
