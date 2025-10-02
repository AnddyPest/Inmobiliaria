using MySql.Data.MySqlClient;
using project.Helpers;
using project.Models;
using project.Models.Interfaces;
using System.Data;
using System.Data.Common;

namespace project.Services
{
    public class InmuebleService(IConfiguration configuration, IPropietarioService propietarioService) : IInmuebleService
    {
        private readonly string _connectionString = configuration.GetConnectionString("Connection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        private readonly IPropietarioService _propietarioService = propietarioService;
        private readonly string _ClassName = nameof(InmuebleService);

        public async Task<(string?, int?)> obtenerCantidadDeRegistros()
        {
            try
            {
                int response = 0;
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = @"Select count(idInmueble) from inmueble";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
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
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(InmuebleService), nameof(obtenerCantidadDeRegistros));
                return (ex.Message, null);
            }
        }

        public async Task<(string?, Inmueble?)> AgregarInmueble(Inmueble inmueble) //TESTEAR
        {
            try
            {
                if (inmueble == null) return ("El inmueble no puede ser nulo", null);
                if (await _propietarioService.getPropietarioById(inmueble.IdPropietario) is (string errorServicio, null)) return (errorServicio, null);
                if (await BuscarInmueblePorDireccion(inmueble.Direccion) is (null, Inmueble))
                    return ($"Ya existe un inmueble con la dirección {inmueble.Direccion}", null);
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = @" INSERT INTO inmueble 
                                      (Uso, id_tipo_inmueble, Superficie, CantidadAmbientes, Coordenadas, Precio, Direccion, ciudad, IdPropietario, Disponible, estado) 
                                      VALUES 
                                      (@Uso, @id_tipo_inmueble, @Superficie, @CantidadAmbientes, @Coordenadas, @Precio, @Direccion, @ciudad, @IdPropietario, @Disponible, @estado);
                                      SELECT LAST_INSERT_ID(); ";
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
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
                if (inmueble.IdInmueble <= 0) return ("El id del inmueble debe ser un valor positivo", false);
                (string?, Inmueble?) inmuebleSearched = await this.ObtenerInmueblePorId(inmueble.IdInmueble);
                if (inmuebleSearched.Item1 != null) return (inmuebleSearched.Item1, false);
                if (inmuebleSearched.Item2 == null) return ($"No se encontró ningún inmueble con el id {inmueble.IdInmueble}", false);
                if (inmueble.IdPropietario != inmuebleSearched.Item2.IdPropietario)
                {
                    if (await _propietarioService.getPropietarioById(inmueble.IdPropietario) is (string errorServicio, null))
                        return (errorServicio, false);
                }
                if (inmueble.Direccion != inmuebleSearched.Item2.Direccion)
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
                                          
                                          estado = @estado
                                      WHERE IdInmueble = @IdInmueble ";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
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
                if (inmuebleSearched.Item1 != null && inmuebleSearched.Item2 == null)
                {
                    HelperFor.imprimirMensajeDeError(inmuebleSearched.Item1, _ClassName, nameof(DarDeBajaInmueble));
                    return (inmuebleSearched.Item1, false);
                }
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @" UPDATE inmueble 
                                      SET estado = 0, Disponible = 0
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

        public async Task<(string?, Inmueble?)> ObtenerInmueblePorContrato(int idContrato) //TESTEAR
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @" SELECT i.* ,ti.*
                                      FROM inmueble as i
                                      inner join tipo_inmueble as ti on ti.id_tipo_inmueble = i.id_tipo_inmueble
                                      inner join contrato as c on c.idInmueble = i.idInmueble
                                      WHERE c.idContrato = @idContrato ";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@idContrato", idContrato);
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
            catch (Exception ex)
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
                if (dniPropietario <= 0) return ("El dni del propietario debe ser un valor positivo", null);
                List<Inmueble> inmuebles = new List<Inmueble>();
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @" SELECT i.* ,ti.*, p.*
                                      FROM inmueble as i
                                      inner join tipo_inmueble as ti on ti.id_tipo_inmueble = i.id_tipo_inmueble
                                      inner join propietario as p on p.idPropietario = i.idPropietario
                                      WHERE p.dni = @dniPropietario ";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@dniPropietario", dniPropietario);
                        using (DbDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
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
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, _ClassName, nameof(ObtenerInmueblesPorPropietario));
                return (ex.Message, null);
            }
        }

        public async Task<(string?, List<Inmueble>?, int? totalRegistros)> ObtenerTodosLosInmuebles(int paginaNro = 1, int tamPagina = 10, bool? disponibilidad = null, int? dniPropietario = null, string? uso = null, string? tipoInmueble = null, int? cantidadAmbientes = null, int? precio = null, DateOnly? fechaDesde = null, DateOnly? fechaHasta = null)//TESTEAR
        {
            try
            {
                int cantidadRegistros;
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = @$"SELECT SQL_CALC_FOUND_ROWS
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
                                    tipoI.nombre as TipoInmuebleNombre,
                                    contract.*,
                                    inquil.idInquilino as InquilinoId,
                                    inquil.idPersona as InquilinoIDPersona,
                                    inquil.estado as EstadoInquilino,
                                    pe.Nombre as InquilinoNombre,
                                    pe.Apellido as InquilinoApellido,
                                    pe.Dni as InquilinoDni,
                                    pe.Telefono as InquilinoTelefono,
                                    pe.Direccion as InquilinoDireccion,
                                    pe.Email as InquilinoEmail,
                                    pe.Estado as InquilinoEstado
                                    FROM inmueble as i
                                    INNER JOIN propietario p ON p.idPropietario = i.idPropietario
                                    INNER JOIN persona perso ON perso.idPersona = p.idPersona
                                    INNER JOIN tipo_inmueble as tipoI ON i.id_tipo_inmueble = tipoI.id_tipo_inmueble
                                    LEFT JOIN contrato as contract ON contract.idInmueble = i.idInmueble AND contract.Estado = 1  
                                    LEFT JOIN inquilino as inquil ON inquil.idInquilino = contract.idInquilino
                                    LEFT JOIN persona as pe on inquil.idPersona = pe.idPersona
                                    GROUP BY i.idInmueble
                                    
                                    ";
                    List<string> querys = new();
                    if (disponibilidad != null) //Hay que encontrar una manera de simplificar esto y mejorar porq con muchos filtros va a ser un caos
                    {
                        querys.Add(@$" i.Disponible = {((disponibilidad == true) ? "1" : "0")} ");
                    }
                    if (dniPropietario != null)
                    {
                        querys.Add(@$" perso.dni = {dniPropietario} ");
                    }
                    if (uso != null)
                    {
                        querys.Add(@$" i.Uso = '{uso}' ");
                    }
                    if (tipoInmueble != null)
                    {
                        querys.Add(@$" tipoI.nombre = '{tipoInmueble}' ");
                    }
                    if (cantidadAmbientes != null)
                    {
                        querys.Add(@$" i.CantidadAmbientes <= {cantidadAmbientes} ");
                    }
                    if (precio != null)
                    {
                        querys.Add(@$" i.Precio <= {precio} ");
                    }
                    if (fechaDesde != null && fechaHasta != null)
                    {

                        querys.Add(@$"  (contract.idContrato IS NULL OR 
                                        (contract.FechaFin < '{fechaDesde!.Value:yyyy-MM-dd}' OR 
                                        contract.FechaInicio > '{fechaHasta.Value:yyyy-MM-dd}')) ");
                    }
                    query += HelperFor.construirSqlWhereAnd(querys);
                    query += @$" ORDER BY i.idInmueble
                                LIMIT {tamPagina} OFFSET {(paginaNro - 1) * tamPagina}; ";
                    Console.WriteLine(query);
                    List<Inmueble> inmuebles = new();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        await connection.OpenAsync();
                        using (DbDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Inmueble inmueble = new Inmueble();
                                Tipo_Inmueble tipo_Inmueble = new Tipo_Inmueble();
                                Propietario propietario = new();
                                Contrato? contrato = null;
                                Inquilino? inquilino = null;

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

                                if (reader["idContrato"] != DBNull.Value)
                                {
                                    contrato = new();
                                    contrato.IdContrato = reader.GetInt32("idContrato");
                                    contrato.IdInmueble = reader.GetInt32("idInmueble");
                                    contrato.IdInquilino = reader.GetInt32("idInquilino");
                                    contrato.Monto = reader.GetDecimal("Monto");
                                    contrato.FechaInicio = reader.GetDateTime("FechaInicio");
                                    contrato.FechaFin = reader.GetDateTime("FechaFin");
                                    contrato.estado = reader.GetBoolean("estado");
                                    contrato.FechaRescision = reader.IsDBNull(reader.GetOrdinal("fechaRescision"))
                                        ? (DateTime?)null
                                        : reader.GetDateTime(reader.GetOrdinal("fechaRescision"));

                                    inquilino = new();
                                    inquilino.IdInquilino = reader.GetInt32("InquilinoId");
                                    inquilino.IdPersona = reader.GetInt32("InquilinoIDPersona");
                                    inquilino.Nombre = reader.GetString("InquilinoNombre");
                                    inquilino.Apellido = reader.GetString("InquilinoApellido");
                                    inquilino.Dni = reader.GetInt32("InquilinoDni");
                                    inquilino.Estado = reader.GetBoolean("InquilinoEstado");
                                    inquilino.Telefono = reader.GetString("InquilinoTelefono");
                                    inquilino.Direccion = reader.GetString("InquilinoDireccion");
                                    inquilino.Email = reader.GetString("InquilinoEmail");
                                    contrato.Inquilino = inquilino;

                                }




                                inmueble.idTipo = reader.GetInt32("id_tipo_inmueble");
                                tipo_Inmueble.id_tipo_inmueble = inmueble.idTipo;
                                tipo_Inmueble.nombre = reader.GetString("TipoInmuebleNombre");

                                inmueble.Tipo = tipo_Inmueble;
                                inmueble.contrato = contrato;

                                inmuebles.Add(inmueble);
                            }

                        }
                        using (MySqlCommand countCommand = new MySqlCommand("select found_rows()", connection))
                        {
                            cantidadRegistros = Convert.ToInt32(await countCommand.ExecuteScalarAsync());
                        }
                        await connection.CloseAsync();
                        if (inmuebles.Count == 0)
                            return ("No se encontraron inmuebles", null, cantidadRegistros);
                        return (null, inmuebles, cantidadRegistros);
                    }
                }
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, _ClassName, nameof(ObtenerTodosLosInmuebles));
                return (ex.Message, null, null);
            }
        }

        public async Task<(string?, Inmueble?)> BuscarInmueblePorDireccion(string direccion) //TESTEAR
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @" SELECT i.* ,ti.*
                                      FROM inmueble as i
                                      inner join tipo_inmueble as ti on ti.id_tipo_inmueble = i.id_tipo_inmueble
                                      WHERE Direccion = @Direccion ";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Direccion", direccion);
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
            catch (Exception ex)
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

        public async Task<(string?, List<Inmueble>?)> ObtenerTodosLosInmueblesAPI()
        {
            try
            {

                using (MySqlConnection connection = new MySqlConnection(_connectionString))
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
                                    ";



                    System.Console.WriteLine(query);
                    List<Inmueble> inmuebles = new();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        await connection.OpenAsync();
                        using (DbDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
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
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, _ClassName, nameof(ObtenerTodosLosInmuebles));
                return (ex.Message, null);
            }
        }
        public async Task<(string?, bool)> CargarImagen(bool esPortada, int idInmueble, Microsoft.AspNetCore.Http.IFormFile file)
        {
            try
            {
                string mkdirPath = Path.Combine("wwwroot", "Images", "Inmuebles", idInmueble.ToString());
                if (!Directory.Exists(mkdirPath))
                {
                    Directory.CreateDirectory(mkdirPath);
                }

                string filePath;
                if (esPortada)
                {
                    filePath = Path.Combine(mkdirPath, $"{idInmueble}_Portada.png");
                    if (File.Exists(filePath))
                        return ("La imagen de portada ya existe", false);
                }
                else
                {
                    var (_, cantidadImagenes) = await ObtenerCantidadImagenes(idInmueble);
                    if (cantidadImagenes >= 6)
                        return ("Ya hay 6 imágenes para este inmueble", false);

                    filePath = Path.Combine(mkdirPath, $"{idInmueble}_{cantidadImagenes + 1}.png");
                    if (File.Exists(filePath))
                        return ("La imagen ya existe", false);
                }

                using (var flujo = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(flujo);
                }

                // SE Serializa el array de URLs y se guarda en la base de datos (Formato JSON)
                string nuevaUrl = $"/Images/Inmuebles/{idInmueble}/{Path.GetFileName(filePath)}";
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    // Obtener las URLs existentes, esto para luego concatenar la nueva y no pisar las anteriores
                    string selectQuery = "SELECT ImagenesUrl FROM inmueble WHERE IdInmueble = @IdInmueble";
                    using (MySqlCommand selectCommand = new MySqlCommand(selectQuery, connection))
                    {
                        selectCommand.Parameters.AddWithValue("@IdInmueble", idInmueble);
                        object? result = await selectCommand.ExecuteScalarAsync();
                        List<string> urls = new();
                        if (result != null && result != DBNull.Value)
                        {
                            string json = result.ToString()!;
                            if (!string.IsNullOrWhiteSpace(json))
                            {
                                urls = System.Text.Json.JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
                            }
                        }
                        // Agregar la nueva URL
                        if (!urls.Contains(nuevaUrl))
                        {
                            urls.Add(nuevaUrl);
                        }
                        // Aca actualizamos el array, lo serializamos (A JSON) y lo guardamos
                        string updateQuery = "UPDATE inmueble SET ImagenesUrl = @ImagenesUrl WHERE IdInmueble = @IdInmueble";
                        using (MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@ImagenesUrl", System.Text.Json.JsonSerializer.Serialize(urls));
                            updateCommand.Parameters.AddWithValue("@IdInmueble", idInmueble);
                            await updateCommand.ExecuteNonQueryAsync();
                        }
                    }
                }

                return (null, true);
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, _ClassName, nameof(CargarImagen));
                return (ex.Message, false);
            }
        }
        public async Task<(string?, int)> ObtenerCantidadImagenes(int idInmueble)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string selectQuery = "SELECT ImagenesUrl FROM inmueble WHERE IdInmueble = @IdInmueble";
                    using (MySqlCommand selectCommand = new MySqlCommand(selectQuery, connection))
                    {
                        selectCommand.Parameters.AddWithValue("@IdInmueble", idInmueble);
                        object? result = await selectCommand.ExecuteScalarAsync();
                        List<string> urls = new();
                        if (result != null && result != DBNull.Value)
                        {
                            string json = result.ToString()!;
                            if (!string.IsNullOrWhiteSpace(json))
                            {
                                urls = System.Text.Json.JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
                            }
                        }
                        // Excluimos la portada para contar solo las imagenes adicionales
                        int count = urls.Count(u => !u.Contains("Portada"));
                        return (null, count);
                    }
                }
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, _ClassName, nameof(ObtenerCantidadImagenes));
                return (ex.Message, 0);
            }
        }

        public async Task<(string?, List<string>?)> ObtenerImagenesInmueble(int idInmueble)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string selectQuery = "SELECT ImagenesUrl FROM inmueble WHERE IdInmueble = @IdInmueble";
                    using (MySqlCommand selectCommand = new MySqlCommand(selectQuery, connection))
                    {
                        selectCommand.Parameters.AddWithValue("@IdInmueble", idInmueble);
                        object? result = await selectCommand.ExecuteScalarAsync();
                        List<string> urls = new();
                        if (result != null && result != DBNull.Value)
                        {
                            string json = result.ToString()!;
                            if (!string.IsNullOrWhiteSpace(json))
                            {
                                urls = System.Text.Json.JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
                            }
                        }
                        // Excluimos la portada para traerla por otro metodo
                        urls = urls.Where(u => !u.Contains("Portada")).ToList();
                        return (null, urls);
                    }
                }
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, _ClassName, nameof(ObtenerImagenesInmueble));
                return (ex.Message, null);
            }
        }
        // ACA traemos la portada wachin
        public async Task<(string?, string?)> ObtenerImagenPortada(int idInmueble)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string selectQuery = "SELECT ImagenesUrl FROM inmueble WHERE IdInmueble = @IdInmueble";
                    using (MySqlCommand selectCommand = new MySqlCommand(selectQuery, connection))
                    {
                        selectCommand.Parameters.AddWithValue("@IdInmueble", idInmueble);
                        object? result = await selectCommand.ExecuteScalarAsync();
                        List<string> urls = new();
                        if (result != null && result != DBNull.Value)
                        {
                            string json = result.ToString()!;
                            if (!string.IsNullOrWhiteSpace(json))
                            {
                                urls = System.Text.Json.JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
                            }
                        }
                        // Buscamos la portada
                        string? portadaUrl = urls.FirstOrDefault(u => u.Contains("Portada"));
                        return (null, portadaUrl);
                    }
                }
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, _ClassName, nameof(ObtenerImagenPortada));
                return (ex.Message, null);
            }
        }
        public async Task<(string?, bool)> EliminarImagen(int idInmueble, string imageUrl)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string selectQuery = "SELECT ImagenesUrl FROM inmueble WHERE IdInmueble = @IdInmueble";
                    using (MySqlCommand selectCommand = new MySqlCommand(selectQuery, connection))
                    {
                        selectCommand.Parameters.AddWithValue("@IdInmueble", idInmueble);
                        object? result = await selectCommand.ExecuteScalarAsync();
                        List<string> urls = new();
                        if (result != null && result != DBNull.Value)
                        {
                            string json = result.ToString()!;
                            if (!string.IsNullOrWhiteSpace(json))
                            {
                                urls = System.Text.Json.JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
                            }
                        }

                        if (!urls.Contains(imageUrl))
                        {
                            return ("La imagen no existe en la base de datos", false);
                        }

                        // DE LA LISTA Q DESERIALIZAMOS, REMOVEMOS LA URL Q DESEAMOS ELIMINAR
                        urls.Remove(imageUrl);

                        // Actualizar la base de datos
                        string updateQuery = "UPDATE inmueble SET ImagenesUrl = @ImagenesUrl WHERE IdInmueble = @IdInmueble";
                        using (MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@ImagenesUrl", System.Text.Json.JsonSerializer.Serialize(urls));
                            updateCommand.Parameters.AddWithValue("@IdInmueble", idInmueble);
                            await updateCommand.ExecuteNonQueryAsync();
                        }

                        // ELIMINAMOS EL ARCHIVO FISICAMENTE (Son pesados y es mejor eliminarlos )
                        string filePath = Path.Combine("wwwroot", imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }
                        else
                        {
                            return ("La imagen no existe en el servidor, pero fue eliminada de la base de datos", true);
                        }

                        return (null, true);
                    }
                }
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, _ClassName, nameof(EliminarImagen));
                return (ex.Message, false);
            }
        }

    public async Task<(string?, List<string>?)> ObtenerFechasOcupadas(int idInmueble)
        {
            try
            {
                List<string> fechasOcupadas = new List<string>();
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"SELECT FechaInicio, FechaFin
                                    FROM contrato
                                    WHERE idInmueble = @idInmueble AND estado = 1";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@idInmueble", idInmueble);
                        using (DbDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                DateTime inicio = reader.GetDateTime("FechaInicio");
                                DateTime fin = reader.GetDateTime("FechaFin");
                                for (var date = inicio.Date; date <= fin.Date; date = date.AddDays(1))
                                {
                                    fechasOcupadas.Add(date.ToString("yyyy-MM-dd"));
                                }
                            }
                        }
                    }
                    await connection.CloseAsync();
                }
                return (null, fechasOcupadas);
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, _ClassName, nameof(ObtenerFechasOcupadas));
                return (ex.Message, null);
            }
        }

    }
}