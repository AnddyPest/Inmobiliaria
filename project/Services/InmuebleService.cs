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
        private readonly string _ClassName = nameof(InmuebleService);

        public async Task<(string?, int?)> obtenerCantidadDeRegistros()
        {
            try
            {
                int response = 0;
                using(MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = @"Select count(idInmueble) from inmueble";
                    using(MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.CommandType = CommandType.Text;
                        await connection.OpenAsync();
                        DbDataReader reader = await command.ExecuteReaderAsync();
                        if (await reader.ReadAsync())
                        {
                            response = reader.GetInt32(0);
                        }
                        await connection.CloseAsync();
                        return (null, response);
                    }

                }
            }catch(Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(InmuebleService), nameof(obtenerCantidadDeRegistros));
                return (ex.Message, null);
            }
        }

        public async Task<(string?, Inmueble?)> AgregarInmueble(Inmueble inmueble) //TESTEAR
        {
            try
            {
                if(inmueble == null) return ("El inmueble no puede ser nulo", null);
                if( await _propietarioService.getPropietarioById(inmueble.IdPropietario) is (string errorServicio, null) ) return (errorServicio, null);
                if( await BuscarInmueblePorDireccion(inmueble.Direccion) is (null, Inmueble ) ) 
                    return ($"Ya existe un inmueble con la dirección {inmueble.Direccion}", null);
                using(MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = @" INSERT INTO inmueble 
                                      (Uso, id_tipo_inmueble, Superficie, CantidadAmbientes, Coordenadas, Precio, Direccion, ciudad, IdPropietario, Disponible, estado) 
                                      VALUES 
                                      (@Uso, @id_tipo_inmueble, @Superficie, @CantidadAmbientes, @Coordenadas, @Precio, @Direccion, @ciudad, @IdPropietario, @Disponible, @estado);
                                      SELECT LAST_INSERT_ID(); ";
                    connection.Open();
                    using(MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Uso", inmueble.Uso);
                        command.Parameters.AddWithValue("@id_tipo_inmueble", inmueble.idTipo);
                        command.Parameters.AddWithValue("@Superficie", inmueble.Superficie);
                        command.Parameters.AddWithValue("@CantidadAmbientes", inmueble.CantidadAmbientes);
                        command.Parameters.AddWithValue("@Coordenadas", inmueble.Coordenadas);
                        command.Parameters.AddWithValue("@Precio", inmueble.Precio);
                        command.Parameters.AddWithValue("@Direccion", inmueble.Direccion);
                        command.Parameters.AddWithValue("@ciudad", inmueble.Ciudad);
                        command.Parameters.AddWithValue("@IdPropietario", inmueble.IdPropietario);
                        command.Parameters.AddWithValue("@Disponible", inmueble.Disponible);
                        command.Parameters.AddWithValue("@estado", inmueble.Estado);
                        object? result = await command.ExecuteScalarAsync();
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
                (string?,Inmueble?) inmuebleSearched = await this.ObtenerInmueblePorId(inmueble.IdInmueble);
                if(inmuebleSearched.Item1 != null) return (inmuebleSearched.Item1, false);
                if(inmuebleSearched.Item2 == null) return ($"No se encontró ningún inmueble con el id {inmueble.IdInmueble}", false);
                if(inmueble.IdPropietario != inmuebleSearched.Item2.IdPropietario)
                {
                    if( await _propietarioService.getPropietarioById(inmueble.IdPropietario) is (string errorServicio, null) ) 
                        return (errorServicio, false);
                }
                if(inmueble.Direccion != inmuebleSearched.Item2.Direccion)
                {
                    if (await BuscarInmueblePorDireccion(inmueble.Direccion) is (null, Inmueble))
                        return ("No se puede agregar un inmueble con dicha direccion ya que hay uno registrado", false);
                }
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @" UPDATE inmueble 
                                      SET Uso = @Uso, 
                                          id_tipo_inmueble = @id_tipo_inmueble, 
                                          Superficie = @Superficie, 
                                          CantidadAmbientes = @CantidadAmbientes, 
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
                        command.Parameters.AddWithValue("@id_tipo_inmueble", inmueble.idTipo);
                        command.Parameters.AddWithValue("@Superficie", inmueble.Superficie);
                        command.Parameters.AddWithValue("@CantidadAmbientes", inmueble.CantidadAmbientes);
                        command.Parameters.AddWithValue("@Coordenadas", inmueble.Coordenadas);
                        command.Parameters.AddWithValue("@Precio", inmueble.Precio);
                        command.Parameters.AddWithValue("@Direccion", inmueble.Direccion);
                        command.Parameters.AddWithValue("@ciudad", inmueble.Ciudad);
                        command.Parameters.AddWithValue("@IdPropietario", inmueble.IdPropietario);
                        command.Parameters.AddWithValue("@Disponible", inmueble.Disponible);
                        command.Parameters.AddWithValue("@estado", inmueble.Estado);
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
                HelperFor.imprimirMensajeDeError(ex.Message, _ClassName, nameof(ActualizarInmueble));
                return (ex.Message, false);
            }
        }

        public async Task<(string?, bool)> DarDeBajaInmueble(int idInmueble) //TESTEAR
        {
            try
            {
                if (idInmueble <= 0)
                {
                    HelperFor.imprimirMensajeDeError("El idInmueble debe ser mayor a 0", nameof(InmuebleService), nameof(DarDeBajaInmueble));
                    return ("El idInmueble debe ser mayor a 0", false);

                }
                (string?, Inmueble?) inmuebleSearched = await this.ObtenerInmueblePorId(idInmueble);
                if(inmuebleSearched.Item1 != null && inmuebleSearched.Item2 == null)
                {
                    HelperFor.imprimirMensajeDeError(inmuebleSearched.Item1, _ClassName, nameof(DarDeBajaInmueble));
                    return (inmuebleSearched.Item1, false);
                }
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
                HelperFor.imprimirMensajeDeError(ex.Message, _ClassName, nameof(DarDeBajaInmueble));
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
                                inmueble.CantidadAmbientes = reader.GetInt32("CantidadAmbientes");
                                inmueble.Coordenadas = reader.GetDecimal("Coordenadas");
                                inmueble.Precio = reader.GetDecimal("Precio");
                                inmueble.Direccion = reader.GetString("Direccion");
                                inmueble.Ciudad = reader.GetString("ciudad");
                                inmueble.IdPropietario = reader.GetInt32("IdPropietario");
                                inmueble.Disponible = reader.GetBoolean("Disponible");
                                inmueble.Estado = reader.GetBoolean("estado");
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
                HelperFor.imprimirMensajeDeError(ex.Message, _ClassName, nameof(ObtenerInmueblePorContrato));
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
                                inmueble.CantidadAmbientes = reader.GetInt32("CantidadAmbientes");
                                inmueble.Coordenadas = reader.GetDecimal("Coordenadas");
                                inmueble.Precio = reader.GetDecimal("Precio");
                                inmueble.Direccion = reader.GetString("Direccion");
                                inmueble.Ciudad = reader.GetString("ciudad");
                                inmueble.IdPropietario = reader.GetInt32("IdPropietario");
                                inmueble.Disponible = reader.GetBoolean("Disponible");
                                inmueble.Estado = reader.GetBoolean("estado");
                                inmueble.idTipo = reader.GetInt32("id_tipo_inmueble");
                                tipo.id_tipo_inmueble = inmueble.idTipo;
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
                HelperFor.imprimirMensajeDeError(ex.Message, _ClassName, nameof(ObtenerInmueblePorId));
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
                                inmueble.CantidadAmbientes = reader.GetInt32("CantidadAmbientes");
                                inmueble.Coordenadas = reader.GetDecimal("Coordenadas");
                                inmueble.Precio = reader.GetDecimal("Precio");
                                inmueble.Direccion = reader.GetString("Direccion");
                                inmueble.Ciudad = reader.GetString("ciudad");
                                inmueble.IdPropietario = reader.GetInt32("IdPropietario");
                                inmueble.Disponible = reader.GetBoolean("Disponible");
                                inmueble.Estado = reader.GetBoolean("estado");
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
                HelperFor.imprimirMensajeDeError(ex.Message, _ClassName, nameof(ObtenerInmueblesPorPropietario));
                return (ex.Message, null);
            }
        }

        public async Task<(string?, List<Inmueble>?)> ObtenerTodosLosInmuebles(int paginaNro = 1, int tamPagina = 10)//TESTEAR
        {
            try
            {
                
                using(MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = @$"SELECT 
                                    i.*,
                                    p.idPropietario as PropietarioId,
                                    p.idPersona as IDPersona,
                                    p.estado as EstadoPropietario,
                                    perso.Nombre as PropietarioNombre,
                                    perso.Apellido as PropietarioApellido,
                                    perso.Dni as PropietarioDni,
                                    perso.Telefono as PropietarioTelefono,
                                    perso.Direccion as PropietarioDireccion,
                                    perso.Email as PropietarioEmail,
                                    perso.Estado as PropietarioEstado,
                                    tipoI.id_tipo_inmueble as TipoInmuebleId,
                                    tipoI.nombre as TipoInmuebleNombre
                                    FROM inmueble as i
                                    INNER JOIN propietario p ON p.idPropietario = i.idPropietario
                                    INNER JOIN persona perso ON perso.idPersona = p.idPersona
                                    INNER JOIN tipo_inmueble as tipoI ON i.id_tipo_inmueble = tipoI.id_tipo_inmueble
                                    LIMIT {tamPagina} OFFSET {(paginaNro - 1 ) * tamPagina}";
                    List<Inmueble> inmuebles = new();
                    using(MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        await connection.OpenAsync();
                        using (DbDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while ( await reader.ReadAsync() )
                            {
                                Inmueble inmueble = new Inmueble();
                                Tipo_Inmueble tipo_Inmueble = new Tipo_Inmueble();
                                Propietario propietario = new();
                                inmueble.IdInmueble = reader.GetInt32("idInmueble");
                                inmueble.Uso = reader.GetString("Uso");
                                inmueble.Superficie = reader.GetInt32("Superficie");
                                inmueble.CantidadAmbientes = reader.GetInt32("CantidadAmbientes");
                                inmueble.Coordenadas = reader.GetDecimal("Coordenadas");
                                inmueble.Precio = reader.GetDecimal("Precio");
                                inmueble.Direccion = reader.GetString("Direccion");
                                inmueble.Ciudad = reader.GetString("ciudad");
                                inmueble.Disponible = reader.GetBoolean("Disponible");
                                inmueble.Estado = reader.GetBoolean("estado");


                                inmueble.IdPropietario = reader.GetInt32("PropietarioId");
                                propietario.IdPropietario = inmueble.IdPropietario;
                                propietario.Nombre = reader.GetString("PropietarioNombre");
                                propietario.Apellido = reader.GetString("PropietarioApellido");
                                propietario.Dni = reader.GetInt32("PropietarioDni");
                                propietario.EstadoPropietario = reader.GetBoolean("PropietarioEstado");
                                propietario.Telefono = reader.GetString("PropietarioTelefono");
                                propietario.Direccion = reader.GetString("PropietarioDireccion");
                                propietario.Email = reader.GetString("PropietarioEmail");
                                propietario.Estado = reader.GetBoolean("PropietarioEstado");
                                propietario.IdPersona = reader.GetInt32("IDPersona");
                                inmueble.Propietario = propietario;
                                
                                inmueble.idTipo = reader.GetInt32("id_tipo_inmueble");
                                tipo_Inmueble.id_tipo_inmueble = inmueble.idTipo;
                                tipo_Inmueble.nombre = reader.GetString("TipoInmuebleNombre");
                                
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
                HelperFor.imprimirMensajeDeError(ex.Message, _ClassName, nameof(ObtenerTodosLosInmuebles));
                return (ex.Message, null);
            }
        }

        public async Task<(string?, Inmueble?)> BuscarInmueblePorDireccion(string direccion) //TESTEAR
        {
            try
            {
                using(MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
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
                                inmueble.CantidadAmbientes = reader.GetInt32("CantidadAmbientes");
                                inmueble.Coordenadas = reader.GetDecimal("Coordenadas");
                                inmueble.Precio = reader.GetDecimal("Precio");
                                inmueble.Direccion = reader.GetString("Direccion");
                                inmueble.Ciudad = reader.GetString("ciudad");
                                inmueble.IdPropietario = reader.GetInt32("IdPropietario");
                                inmueble.Disponible = reader.GetBoolean("Disponible");
                                inmueble.Estado = reader.GetBoolean("estado");
                                tipo.id_tipo_inmueble = reader.GetInt32("id_tipo_inmueble");
                                tipo.nombre = reader.GetString("nombre");
                                inmueble.Tipo = tipo;
                                return (null, inmueble);
                            }
                            HelperFor.imprimirMensajeDeError($"No se encontró ningún inmueble con la dirección {direccion}", _ClassName, nameof(BuscarInmueblePorDireccion));
                            return ($"No se encontró ningún inmueble con la dirección {direccion}", null);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, _ClassName, nameof(BuscarInmueblePorDireccion));
                return (ex.Message, null);
            }
        }

        public async Task<(string?, bool)> DarAltaLogica(int idInmueble)
        {
            try
            {
                if (idInmueble <= 0)
                {
                    HelperFor.imprimirMensajeDeError("El idInmueble debe ser mayor a 0", nameof(InmuebleService), nameof(DarDeBajaInmueble));
                    return ("El idInmueble debe ser mayor a 0", false);

                }
                (string?, Inmueble?) inmuebleSearched = await this.ObtenerInmueblePorId(idInmueble);
                if (inmuebleSearched.Item1 != null && inmuebleSearched.Item2 == null)
                {
                    HelperFor.imprimirMensajeDeError(inmuebleSearched.Item1, _ClassName, nameof(DarDeBajaInmueble));
                    return (inmuebleSearched.Item1, false);
                }
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @" UPDATE inmueble 
                                      SET estado = 1, Disponible = 1
                                      WHERE IdInmueble = @IdInmueble ";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
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
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, _ClassName, nameof(DarDeBajaInmueble));
                return (ex.Message, false);
            }
        }

        public async Task<(string?, bool)> MarcarAlquilado(int idInmueble)
        {
            {
                try
                {
                    if (idInmueble <= 0)
                    {
                        HelperFor.imprimirMensajeDeError("El idInmueble debe ser mayor a 0", nameof(InmuebleService), nameof(DarDeBajaInmueble));
                        return ("El idInmueble debe ser mayor a 0", false);

                    }
                    (string?, Inmueble?) inmuebleSearched = await this.ObtenerInmueblePorId(idInmueble);
                    if (inmuebleSearched.Item1 != null && inmuebleSearched.Item2 == null)
                    {
                        HelperFor.imprimirMensajeDeError(inmuebleSearched.Item1, _ClassName, nameof(DarDeBajaInmueble));
                        return (inmuebleSearched.Item1, false);
                    }
                    using (MySqlConnection connection = new MySqlConnection(_connectionString))
                    {
                        connection.Open();
                        string query = @" UPDATE inmueble 
                                      SET Disponible = 0
                                      WHERE IdInmueble = @IdInmueble ";
                        using (MySqlCommand command = new MySqlCommand(query, connection))
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
                catch (Exception ex)
                {
                    HelperFor.imprimirMensajeDeError(ex.Message, _ClassName, nameof(DarDeBajaInmueble));
                    return (ex.Message, false);
                }
            }
        }

        public async Task<(string?, bool)> MarcarLibre(int idInmueble)
        {
            {
                try
                {
                    if (idInmueble <= 0)
                    {
                        HelperFor.imprimirMensajeDeError("El idInmueble debe ser mayor a 0", nameof(InmuebleService), nameof(DarDeBajaInmueble));
                        return ("El idInmueble debe ser mayor a 0", false);

                    }
                    (string?, Inmueble?) inmuebleSearched = await this.ObtenerInmueblePorId(idInmueble);
                    if (inmuebleSearched.Item1 != null && inmuebleSearched.Item2 == null)
                    {
                        HelperFor.imprimirMensajeDeError(inmuebleSearched.Item1, _ClassName, nameof(DarDeBajaInmueble));
                        return (inmuebleSearched.Item1, false);
                    }
                    using (MySqlConnection connection = new MySqlConnection(_connectionString))
                    {
                        connection.Open();
                        string query = @" UPDATE inmueble 
                                      SET Disponible = 1
                                      WHERE IdInmueble = @IdInmueble ";
                        using (MySqlCommand command = new MySqlCommand(query, connection))
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
                catch (Exception ex)
                {
                    HelperFor.imprimirMensajeDeError(ex.Message, _ClassName, nameof(DarDeBajaInmueble));
                    return (ex.Message, false);
                }
            }
        }
    }
}
