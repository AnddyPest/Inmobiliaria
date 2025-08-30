using MySql.Data.MySqlClient;
using project.Helpers;
using project.Models;
using project.Models.Interfaces;
using System.Data;
using System.Data.Common;

namespace project.Services
{
    public class InmuebleService(IConfiguration configuration, IPropietarioService propietarioService ) : IInmuebleService
    {
        private readonly string _connectionString = configuration.GetConnectionString("Connection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        private readonly IPropietarioService _propietarioService = propietarioService;
        public async Task<(string?, Inmueble?)> AgregarInmueble(Inmueble inmueble) //TESTEAR
        {
            try
            {
                if(inmueble == null) return ("El inmueble no puede ser nulo", null);
                if( await _propietarioService.getPropietarioById(inmueble.IdPropietario) is (string errorServicio, null) ) return (errorServicio, null);
                if( await BuscarInmueblePorDireccion(inmueble.Direccion) is (string error, Inmueble foundInmueble) && foundInmueble != null ) 
                    return ($"Ya existe un inmueble con la dirección {inmueble.Direccion}", null);
                using(MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = @" INSERT INTO inmueble 
                                      (Uso, id_tipo_inmueble, Superficie, CantAmbientes, Coordenadas, Precio, Direccion, ciudad, IdPropietario, Disponible, estado) 
                                      VALUES 
                                      (@Uso, @id_tipo_inmueble, @Superficie, @CantAmbientes, @Coordenadas, @Precio, @Direccion, @ciudad, @IdPropietario, @Disponible, @estado);
                                      SELECT LAST_INSERT_ID(); ";
                    connection.Open();
                    using(MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Uso", inmueble.Uso);
                        command.Parameters.AddWithValue("@id_tipo_inmueble", inmueble.Tipo.id_tipo_inmueble);
                        command.Parameters.AddWithValue("@Superficie", inmueble.Superficie);
                        command.Parameters.AddWithValue("@CantAmbientes", inmueble.CantAmbientes);
                        command.Parameters.AddWithValue("@Coordenadas", inmueble.Coordenadas);
                        command.Parameters.AddWithValue("@Precio", inmueble.Precio);
                        command.Parameters.AddWithValue("@Direccion", inmueble.Direccion);
                        command.Parameters.AddWithValue("@ciudad", inmueble.ciudad);
                        command.Parameters.AddWithValue("@IdPropietario", inmueble.IdPropietario);
                        command.Parameters.AddWithValue("@Disponible", inmueble.Disponible);
                        command.Parameters.AddWithValue("@estado", inmueble.estado);
                        object result = await command.ExecuteScalarAsync();
                        if (result != null && int.TryParse(result.ToString(), out int newId))
                        {
                            inmueble.IdInmueble = newId;
                            return (null, inmueble);
                        }
                        else
                        {
                            return ("No se pudo obtener el id del nuevo inmueble insertado", null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(InmuebleService), nameof(AgregarInmueble));
                return (ex.Message, null);
            }
        }

        public async Task<(string?, bool)> ActualizarInmueble(Inmueble inmueble) //TESTEAR
        {
            try
            {
                if( inmueble.IdInmueble <= 0) return ("El id del inmueble debe ser un valor positivo", false);
                if (await this.ObtenerInmueblePorId(inmueble.IdInmueble) is (string error, null)) return (error, false);
                using(MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @" UPDATE inmueble 
                                      SET Uso = @Uso, 
                                          id_tipo_inmueble = @id_tipo_inmueble, 
                                          Superficie = @Superficie, 
                                          CantAmbientes = @CantAmbientes, 
                                          Coordenadas = @Coordenadas, 
                                          Precio = @Precio, 
                                          Direccion = @Direccion, 
                                          ciudad = @ciudad, 
                                          IdPropietario = @IdPropietario, 
                                          Disponible = @Disponible, 
                                          estado = @estado
                                      WHERE IdInmueble = @IdInmueble ";
                    using(MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdInmueble", inmueble.IdInmueble);
                        command.Parameters.AddWithValue("@Uso", inmueble.Uso);
                        command.Parameters.AddWithValue("@id_tipo_inmueble", inmueble.Tipo.id_tipo_inmueble);
                        command.Parameters.AddWithValue("@Superficie", inmueble.Superficie);
                        command.Parameters.AddWithValue("@CantAmbientes", inmueble.CantAmbientes);
                        command.Parameters.AddWithValue("@Coordenadas", inmueble.Coordenadas);
                        command.Parameters.AddWithValue("@Precio", inmueble.Precio);
                        command.Parameters.AddWithValue("@Direccion", inmueble.Direccion);
                        command.Parameters.AddWithValue("@ciudad", inmueble.ciudad);
                        command.Parameters.AddWithValue("@IdPropietario", inmueble.IdPropietario);
                        command.Parameters.AddWithValue("@Disponible", inmueble.Disponible);
                        command.Parameters.AddWithValue("@estado", inmueble.estado);
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            return (null, true);
                        }
                        else
                        {
                            return ($"No se encontró ningún inmueble con el id {inmueble.IdInmueble}", false);
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(InmuebleService), nameof(ActualizarInmueble));
                return (ex.Message, false);
            }
        }

        public async Task<(string?, bool)> DarDeBajaInmueble(int idInmueble) //TESTEAR
        {
            try
            {
                using(MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @" UPDATE inmueble 
                                      SET estado = 0, Disponible = 0
                                      WHERE IdInmueble = @IdInmueble ";
                    using(MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdInmueble", idInmueble);
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            return (null, true);
                        }
                        else
                        {
                            return ($"No se encontró ningún inmueble con el id {idInmueble}", false);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(InmuebleService), nameof(DarDeBajaInmueble));
                return (ex.Message, false);
            }
        }

        public async Task<(string?, Inmueble?)> ObtenerInmueblePorContrato(int idContrato) //TESTEAR
        {
            try
            {
                using(MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @" SELECT i.* ,ti.*
                                      FROM inmueble as i
                                      inner join tipo_inmueble as ti on ti.id_tipo_inmueble = i.id_tipo_inmueble
                                      inner join contrato as c on c.idInmueble = i.idInmueble
                                      WHERE c.idContrato = @idContrato ";
                    using(MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@idContrato", idContrato);
                        using(DbDataReader reader = await command.ExecuteReaderAsync())
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
                            return ($"No se encontró ningún inmueble para el contrato con id {idContrato}", null);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(InmuebleService), nameof(ObtenerInmueblePorContrato));
                return (ex.Message, null);
            }
        }

        public async Task<(string?, Inmueble?)> ObtenerInmueblePorId(int idInmueble) //TESTEAR
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

        public async Task<(string?, List<Inmueble>?)> ObtenerInmueblesPorPropietario(int dniPropietario) //TESTEAR
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

        public async Task<(string?, List<Inmueble>?)> ObtenerTodosLosInmuebles()//TESTEAR
        {
            try
            {
                
                using(MySqlConnection connection = new MySqlConnection())
                {
                    string query = @"Select i.* 
                                 from inmueble as i";
                    List<Inmueble> inmuebles = new();
                    using(MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (DbDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while ( await reader.ReadAsync() )
                            {
                                Inmueble inmueble = new Inmueble();
                                Tipo_Inmueble tipo_Inmueble = new Tipo_Inmueble();
                                inmueble.IdInmueble = reader.GetInt32("idInmueble");
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
                                tipo_Inmueble.id_tipo_inmueble = reader.GetInt32("id_tipo_inmueble");
                                inmueble.Tipo = tipo_Inmueble;
                                inmuebles.Add(inmueble);

                            }
                            await connection.CloseAsync();
                            if (inmuebles.Count == 0) return ("No se encontraron inmuebles", null);
                            return (null, inmuebles);
                        }
                    }
                }
            }catch(Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(InmuebleService), nameof(ObtenerTodosLosInmuebles));
                return (ex.Message, null);
            }
        }

        public async Task<(string?, Inmueble?)> BuscarInmueblePorDireccion(string direccion) //TESTEAR
        {
            try
            {
                using(MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @" SELECT i.* ,ti.*
                                      FROM inmueble as i
                                      inner join tipo_inmueble as ti on ti.id_tipo_inmueble = i.id_tipo_inmueble
                                      WHERE Direccion = @Direccion ";
                    using(MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Direccion", direccion);
                        using(DbDataReader reader = await command.ExecuteReaderAsync())
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
                            return ($"No se encontró ningún inmueble con la dirección {direccion}", null);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(InmuebleService), nameof(BuscarInmueblePorDireccion));
                return (ex.Message, null);
            }
        }
    }
}
