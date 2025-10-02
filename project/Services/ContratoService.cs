using MySql.Data.MySqlClient;
using project.Helpers;
using project.Models;
using project.Models.Interfaces;
using System.Data.Common;
using System.Data;


namespace project.Services
{
    public class ContratoService(IConfiguration configuration,IInmuebleService inmuebleService , IPropietarioService propietarioService, IInquilinoService inquilinoService) : IContratoService
    {
        private readonly string _connectionString = configuration.GetConnectionString("Connection")!;
        private IPropietarioService _propietarioService = propietarioService;
        private IInquilinoService _inquilinoService = inquilinoService;
        private IInmuebleService _inmuebleService = inmuebleService;
        public async Task<(string?, bool)> CreateContrato(Contrato contrato) //testear
        {
            try
            {
                if (contrato == null) return ("El contrato no puede ser nulo.", false);
                if (this.ComprobarContratoActivoPorIdInmueble(contrato.IdInmueble).Result.Item2)
                    return ($"El inmueble con Id {contrato.IdInmueble} ya tiene un contrato activo.", false);
                (string?, Inquilino?) inquilinoFinded = await _inquilinoService.GetInquilinoById(contrato.IdInquilino);
                if (inquilinoFinded.Item1 != null)
                    return ($"No se encontró un inquilino con Id {contrato.IdInquilino}.", false);
                (string?, Propietario?) propietarioFinded = await _propietarioService.getPropietarioById(contrato.IdPropietario);
                if (propietarioFinded.Item1 != null)
                    return ($"No se encontró un propietario con Id {contrato.IdPropietario}.", false);
                if (inquilinoFinded.Item2!.IdPersona == propietarioFinded.Item2!.IdPersona)
                    return ($"Un propietario no puede alquilar su propia propiedad", false);
                if (await ValidarNoSuperposicionFechas(contrato.IdInmueble, contrato.FechaInicio, contrato.FechaFin) is (string error, bool result) && error != null && !result)
                    return (error, false);
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = @"INSERT INTO Contrato (IdInquilino, IdInmueble, IdPropietario, Monto, FechaInicio, FechaFin, estado) 
                                     VALUES (@IdInquilino, @IdInmueble, @IdPropietario, @Monto, @FechaInicio, @FechaFin, 1)";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdInquilino", contrato.IdInquilino);
                        command.Parameters.AddWithValue("@IdInmueble", contrato.IdInmueble);
                        command.Parameters.AddWithValue("@IdPropietario", contrato.IdPropietario);
                        command.Parameters.AddWithValue("@Monto", contrato.Monto);
                        command.Parameters.AddWithValue("@FechaInicio", contrato.FechaInicio);
                        command.Parameters.AddWithValue("@FechaFin", contrato.FechaFin);

                        await connection.OpenAsync();
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        if (rowsAffected == 0)
                        {
                            await connection.CloseAsync();
                            return ("No se pudo crear el contrato.", false);
                        }
                        await connection.CloseAsync();
                        return (null, true);
                    }
                }
            }
            catch (Exception ex)
            {

                HelperFor.imprimirMensajeDeError(ex.Message, nameof(ContratoService), nameof(CreateContrato));
                return (ex.Message, false);
            }
        }
        public async Task<(string?, bool)> UpdateContrato(Contrato contrato) //testear
        {
            if (contrato == null) return ("El contrato no puede ser nulo.", false);
            if (contrato.IdContrato <= 0) return ("El id del contrato debe ser un número positivo.", false);
            (string?, Contrato?) contratoExistente = await GetContratoById(contrato.IdContrato);
            if (contratoExistente.Item1 != null) return (contratoExistente.Item1, false);
            if (contratoExistente.Item2 == null) return ($"No se encontró un contrato con Id {contrato.IdContrato}.", false);
            if (contratoExistente.Item2.estado == false) return ("No se puede actualizar un contrato que está dado de baja.", false);
            if (contratoExistente.Item2.IdInmueble != contrato.IdInmueble)
            {
                if (this.ComprobarContratoActivoPorIdInmueble(contrato.IdInmueble).Result.Item2)
                    return ($"El inmueble con Id {contrato.IdInmueble} ya tiene un contrato activo.", false);
            }
            (string?, Inquilino?) inquilinoFinded = await _inquilinoService.GetInquilinoById(contrato.IdInquilino);
            if (inquilinoFinded.Item1 != null)
                return ($"No se encontró un inquilino con Id {contrato.IdInquilino}.", false);
            (string?, Propietario?) propietarioFinded = await _propietarioService.getPropietarioById(contrato.IdPropietario);
            if (propietarioFinded.Item1 != null)
                return ($"No se encontró un propietario con Id {contrato.IdPropietario}.", false);
            if (inquilinoFinded.Item2!.IdPersona == propietarioFinded.Item2!.IdPersona)
                return ($"Un propietario no puede alquilar su propia propiedad", false);
            if (await ValidarNoSuperposicionFechas(contrato.IdInmueble, contrato.FechaInicio, contrato.FechaFin) is (string error, bool result) && error != null && !result)
                return (error, false);
            try
            {
                if ((await GetContratoById(contrato.IdContrato)).Item2 == null)
                    return ($"No se encontró un contrato con Id {contrato.IdContrato}.", false);
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = @"UPDATE Contrato 
                                     SET IdInquilino = @IdInquilino, 
                                         IdInmueble = @IdInmueble, 
                                         IdPropietario = @IdPropietario, 
                                         Monto = @Monto, 
                                         FechaInicio = @FechaInicio, 
                                         FechaFin = @FechaFin, 
                                         estado = @estado 
                                     WHERE IdContrato = @IdContrato";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdContrato", contrato.IdContrato);
                        command.Parameters.AddWithValue("@IdInquilino", contrato.IdInquilino);
                        command.Parameters.AddWithValue("@IdInmueble", contrato.IdInmueble);
                        command.Parameters.AddWithValue("@IdPropietario", contrato.IdPropietario);
                        command.Parameters.AddWithValue("@Monto", contrato.Monto);
                        command.Parameters.AddWithValue("@FechaInicio", contrato.FechaInicio);
                        command.Parameters.AddWithValue("@FechaFin", contrato.FechaFin);
                        command.Parameters.AddWithValue("@estado", contrato.estado);
                        await connection.OpenAsync();
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        if (rowsAffected == 0)
                        {
                            await connection.CloseAsync();
                            return ($"No se pudo actualizar el contrato con Id {contrato.IdContrato}.", false);
                        }
                        await connection.CloseAsync();
                        return (null, true);
                    }
                }
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(ContratoService), nameof(UpdateContrato));
                return (ex.Message, false);
            }
        }
        public async Task<(string?, bool)> DarAltaContrato(int idContrato) //testear
        {
            if (idContrato <= 0) return ("El id del contrato debe ser un número positivo.", false);
            try
            {
                if ((await GetContratoById(idContrato)).Item2 == null)
                    return ($"No se encontró un contrato con Id {idContrato}.", false);
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = "UPDATE Contrato SET estado = 1 WHERE IdContrato = @IdContrato";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdContrato", idContrato);
                        await connection.OpenAsync();
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        await connection.CloseAsync();
                        if (rowsAffected == 0)
                        {
                            return ($"No se pudo dar de alta el contrato con Id {idContrato}.", false);
                        }
                        return (null, true);
                    }
                }
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(ContratoService), nameof(DarAltaContrato));
                return (ex.Message, false);

            }
        }
        public async Task<(string?, bool)> DarBajaContrato(int idContrato) //testear
        {
            if (idContrato <= 0) return ("El id del contrato debe ser un número positivo.", false);
            try
            {
                if ((await GetContratoById(idContrato)).Item2 == null)
                    return ($"No se encontró un contrato con Id {idContrato}.", false);
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = "UPDATE Contrato SET estado = 0, fechaRescision = @fechaRescision WHERE IdContrato = @IdContrato";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdContrato", idContrato);
                        command.Parameters.AddWithValue("@FechaRescision", DateTime.Now);
                        await connection.OpenAsync();
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        await connection.CloseAsync();
                        if (rowsAffected == 0)
                        {
                            return ($"No se pudo dar de baja el contrato con Id {idContrato}.", false);
                        }
                        return (null, true);
                    }
                }
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(ContratoService), nameof(DarBajaContrato));
                return (ex.Message, false);

            }
        }

        public async Task<(string?, List<Contrato>?)> GetAllContratos(int? nroPagina, int? registrosPorPagina, string? disponibilidad, int? fechaCompare, string? inmueble) //testear
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = @"SELECT c.*,
                                     inquilinoPersona.idPersona as idPersonaInquilino,
                                     i.idInquilino as InquilinoId,
                                     inquilinoPersona.Nombre AS NombreInquilino, 
                                     inquilinoPersona.Apellido AS ApellidoInquilino,
                                     inquilinoPersona.Dni AS DniInquilino,

                                     propietarioPersona.idPersona as idPersonaPropietario,
                                     p.idPropietario as PropietarioId,
                                     propietarioPersona.Nombre AS NombrePropietario, 
                                     propietarioPersona.Apellido AS ApellidoPropietario,
                                     propietarioPersona.Dni AS DniPropietario,

                                     Inmueble.idInmueble as idInmuebleFisico,
                                     Inmueble.id_tipo_inmueble as id_tipo_inmuebleFisico,
                                     Inmueble.idPropietario as idPropietarioFisico,
                                     Inmueble.direccion as InmuebleDireccionFisico

                                     FROM Contrato as c
                                     LEFT JOIN Inmueble as Inmueble ON c.IdInmueble = Inmueble.IdInmueble 
                                     LEFT JOIN Propietario as p ON c.IdPropietario = p.IdPropietario
                                     LEFT JOIN Inquilino as i ON c.IdInquilino = i.IdInquilino
                                     LEFT JOIN Persona AS inquilinoPersona ON i.IdPersona = inquilinoPersona.IdPersona
                                     LEFT JOIN Persona AS propietarioPersona ON p.IdPersona = propietarioPersona.IdPersona
                                     ";
                    List<string> parametros = new List<string>();
                    if (!string.IsNullOrEmpty(disponibilidad))
                    {
                        if (disponibilidad.ToLower() == "vigente")
                            parametros.Add(" c.estado = 1 ");
                        //query += " WHERE c.estado = 1 ";
                        else if (disponibilidad.ToLower() == "no vigente")
                            parametros.Add(" c.estado = 0 ");
                        // query += " WHERE c.estado = 0 ";
                        // Si es 'todos' o vacío, no se agrega WHERE y se muestran ambos estados
                    }
                    if (fechaCompare.HasValue)
                    {
                        string filtroSQL = "";
                        if (fechaCompare.Value == 30)
                        {
                            filtroSQL = "FechaFin BETWEEN CURDATE() AND DATE_ADD(CURDATE(), INTERVAL 30 DAY)";
                        }
                        else if (fechaCompare.Value == 60)
                        {
                            filtroSQL = "FechaFin BETWEEN DATE_ADD(CURDATE(), INTERVAL 31 DAY) AND DATE_ADD(CURDATE(), INTERVAL 60 DAY)";
                        }
                        else if (fechaCompare.Value == 90)
                        {
                            filtroSQL = "FechaFin BETWEEN DATE_ADD(CURDATE(), INTERVAL 61 DAY) AND DATE_ADD(CURDATE(), INTERVAL 90 DAY)";
                        }
                        if (!string.IsNullOrEmpty(filtroSQL))
                        {
                            parametros.Add(filtroSQL);
                            // if (query.Contains("WHERE"))
                            //     query += $" AND {filtroSQL}";
                            // else
                            //     query += $" WHERE {filtroSQL}";
                        }


                    }
                    if (!string.IsNullOrEmpty(inmueble))
                    {
                        parametros.Add($" Inmueble.direccion LIKE '%{inmueble}%' ");
                    }
                    query += HelperFor.construirSqlWhereAnd(parametros);

                    List<Contrato> contratos = new List<Contrato>();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        connection.Open();
                        using (DbDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Contrato contrato = new Contrato();
                                Inquilino? inquilino = null;
                                Propietario? propietario = null;
                                Inmueble? inmuebleResponse = null;

                                contrato.IdContrato = reader.GetInt32("IdContrato");
                                contrato.IdInquilino = reader.GetInt32("IdInquilino");
                                contrato.IdInmueble = reader.GetInt32("IdInmueble");
                                contrato.IdPropietario = reader.GetInt32("IdPropietario");
                                contrato.Monto = reader.GetDecimal("Monto");
                                contrato.FechaInicio = reader.GetDateTime("FechaInicio");
                                contrato.FechaFin = reader.GetDateTime("FechaFin");
                                contrato.estado = reader.GetBoolean("estado");
                                contrato.FechaRescision = reader.IsDBNull(reader.GetOrdinal("fechaRescision"))
                                    ? (DateTime?)null
                                    : reader.GetDateTime(reader.GetOrdinal("fechaRescision"));
                                if (reader["NombreInquilino"] != DBNull.Value && reader["NombrePropietario"] != DBNull.Value && reader["idInmuebleFisico"] != DBNull.Value)
                                {
                                    inquilino = new Inquilino();
                                    inquilino.IdPersona = reader.GetInt32("idPersonaInquilino");
                                    inquilino.IdInquilino = reader.GetInt32("InquilinoId");
                                    inquilino.Nombre = reader.GetString("NombreInquilino");
                                    inquilino.Apellido = reader.GetString("ApellidoInquilino");
                                    inquilino.Dni = reader.GetInt32("DniInquilino");
                                    contrato.Inquilino = inquilino;

                                    propietario = new Propietario();
                                    propietario.IdPersona = reader.GetInt32("idPersonaPropietario");
                                    propietario.IdPropietario = reader.GetInt32("PropietarioId");
                                    propietario.Nombre = reader.GetString("NombrePropietario");
                                    propietario.Apellido = reader.GetString("ApellidoPropietario");
                                    propietario.Dni = reader.GetInt32("DniPropietario");

                                    contrato.Propietario = propietario;

                                    inmuebleResponse = new Inmueble();
                                    inmuebleResponse.IdInmueble = reader.GetInt32("idInmuebleFisico");
                                    inmuebleResponse.idTipo = reader.GetInt32("id_tipo_inmuebleFisico");
                                    inmuebleResponse.IdPropietario = reader.GetInt32("IdPropietarioFisico");
                                    inmuebleResponse.Direccion = reader.GetString("InmuebleDireccionFisico");
                                    contrato.inmueble = inmuebleResponse;
                                }
                                contratos.Add(contrato);
                            }
                        }
                        await connection.CloseAsync();
                        int totalContratos = contratos.Count;
                        // PAGINACIÓN
                        if (nroPagina.HasValue && registrosPorPagina.HasValue)
                        {
                            contratos = contratos.Skip((nroPagina.Value - 1) * registrosPorPagina.Value)
                                                .Take(registrosPorPagina.Value)
                                                .ToList();
                        }
                        // Retornar el total como primer elemento de la tupla (usando un string para compatibilidad)
                        return (totalContratos.ToString(), contratos);
                    }
                }
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(ContratoService), nameof(GetAllContratos));
                return (ex.Message, null);
            }
        }

        public async Task<(string?, Contrato?)> GetContratoById(int idContrato) //testear
        {
            if (idContrato <= 0) return ("El id del contrato debe ser un número positivo.", null);
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = "SELECT * FROM Contrato WHERE IdContrato = @IdContrato";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdContrato", idContrato);
                        await connection.OpenAsync();
                        using (DbDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                Contrato contrato = new Contrato();
                                contrato.IdContrato = reader.GetInt32("IdContrato");
                                contrato.IdInquilino = reader.GetInt32("IdInquilino");
                                contrato.IdInmueble = reader.GetInt32("IdInmueble");
                                contrato.IdPropietario = reader.GetInt32("IdPropietario");
                                contrato.Monto = reader.GetDecimal("Monto");
                                contrato.FechaInicio = reader.GetDateTime("FechaInicio");
                                contrato.FechaFin = reader.GetDateTime("FechaFin");
                                contrato.estado = reader.GetBoolean("estado");
                                contrato.FechaRescision = reader.IsDBNull(reader.GetOrdinal("fechaRescision"))
                                    ? (DateTime?)null
                                    : reader.GetDateTime(reader.GetOrdinal("fechaRescision"));
                                await connection.CloseAsync();
                                return (null, contrato);
                            }
                            else
                            {
                                await connection.CloseAsync();
                                return ($"No se encontró un contrato con Id {idContrato}.", null);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(ContratoService), nameof(GetContratoById));
                return (ex.Message, null);
            }
        }

        public async Task<(string?, List<Contrato>?)> GetContratoByIdInmueble(int idInmueble) //testear
        {

            try
            {

                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = "SELECT * FROM Contrato WHERE IdInmueble = @IdInmueble AND estado = 1";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdInmueble", idInmueble);
                        await connection.OpenAsync();
                        using (DbDataReader reader = await command.ExecuteReaderAsync())
                        {
                            List<Contrato> contratos = new List<Contrato>();
                            while (await reader.ReadAsync())
                            {
                                Contrato contrato = new Contrato();
                                contrato.IdContrato = reader.GetInt32("IdContrato");
                                contrato.IdInquilino = reader.GetInt32("IdInquilino");
                                contrato.IdInmueble = reader.GetInt32("IdInmueble");
                                contrato.IdPropietario = reader.GetInt32("IdPropietario");
                                contrato.Monto = reader.GetDecimal("Monto");
                                contrato.FechaInicio = reader.GetDateTime("FechaInicio");
                                contrato.FechaFin = reader.GetDateTime("FechaFin");
                                contrato.estado = reader.GetBoolean("estado");
                                contrato.FechaRescision = reader.IsDBNull(reader.GetOrdinal("fechaRescision"))
                                    ? (DateTime?)null
                                    : reader.GetDateTime(reader.GetOrdinal("fechaRescision"));
                                contratos.Add(contrato);
                            }
                            await connection.CloseAsync();
                            if (contratos.Count == 0) return ("No se encontraron contratos para el inmueble especificado.", null);
                            return (null, contratos);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(ContratoService), nameof(GetContratoByIdInmueble));
                return (ex.Message, null);
            }
        }

        public async Task<(string?, List<Contrato>?)> GetContratosByIdInquilino(int idInquilino) //testear
        {
            if (idInquilino <= 0) return ("El id del inquilino debe ser un número positivo.", null);
            try
            {
                if ((await _inquilinoService.GetInquilinoById(idInquilino)).Item2 == null)
                    return ($"No se encontró un inquilino con Id {idInquilino}.", null);
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = "SELECT * FROM Contrato WHERE IdInquilino = @IdInquilino";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdInquilino", idInquilino);
                        await connection.OpenAsync();
                        using (DbDataReader reader = await command.ExecuteReaderAsync())
                        {
                            List<Contrato> contratos = new List<Contrato>();
                            while (await reader.ReadAsync())
                            {
                                Contrato contrato = new Contrato();
                                contrato.IdContrato = reader.GetInt32("IdContrato");
                                contrato.IdInquilino = reader.GetInt32("IdInquilino");
                                contrato.IdInmueble = reader.GetInt32("IdInmueble");
                                contrato.IdPropietario = reader.GetInt32("IdPropietario");
                                contrato.Monto = reader.GetDecimal("Monto");
                                contrato.FechaInicio = reader.GetDateTime("FechaInicio");
                                contrato.FechaFin = reader.GetDateTime("FechaFin");
                                contrato.estado = reader.GetBoolean("estado");
                            }
                            await connection.CloseAsync();
                            if (contratos.Count == 0) return ("No se encontraron contratos para el inquilino especificado.", null);
                            return (null, contratos);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(ContratoService), nameof(GetContratosByIdPropietario));
                return (ex.Message, null);
            }
        }

        public async Task<(string?, List<Contrato>?)> GetContratosByIdPropietario(int idPropietario) //testear
        {
            if (idPropietario <= 0) return ("El id del propietario debe ser un número positivo.", null);
            try
            {
                if ((await _propietarioService.getPropietarioById(idPropietario)).Item2 == null)
                    return ($"No se encontró un propietario con Id {idPropietario}.", null);
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = "SELECT * FROM Contrato WHERE IdPropietario = @IdPropietario";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdPropietario", idPropietario);
                        await connection.OpenAsync();
                        using (DbDataReader reader = await command.ExecuteReaderAsync())
                        {
                            List<Contrato> contratos = new List<Contrato>();
                            while (await reader.ReadAsync())
                            {
                                Contrato contrato = new Contrato();
                                contrato.IdContrato = reader.GetInt32("IdContrato");
                                contrato.IdInquilino = reader.GetInt32("IdInquilino");
                                contrato.IdInmueble = reader.GetInt32("IdInmueble");
                                contrato.IdPropietario = reader.GetInt32("IdPropietario");
                                contrato.Monto = reader.GetDecimal("Monto");
                                contrato.FechaInicio = reader.GetDateTime("FechaInicio");
                                contrato.FechaFin = reader.GetDateTime("FechaFin");
                                contrato.estado = reader.GetBoolean("estado");
                            }
                            await connection.CloseAsync();
                            if (contratos.Count == 0) return ("No se encontraron contratos para el propietario especificado.", null);
                            return (null, contratos);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(ContratoService), nameof(GetContratosByIdPropietario));
                return (ex.Message, null);
            }
        }

        public async Task<(string?, List<Contrato>?)> GetContratosVigentes()//testear
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT * FROM Contrato WHERE estado = 1";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (DbDataReader reader = await command.ExecuteReaderAsync())
                        {
                            List<Contrato> contratos = new List<Contrato>();
                            while (await reader.ReadAsync())
                            {
                                Contrato contrato = new Contrato();

                                contrato.IdContrato = reader.GetInt32("IdContrato");
                                contrato.IdInquilino = reader.GetInt32("IdInquilino");
                                contrato.IdInmueble = reader.GetInt32("IdInmueble");
                                contrato.IdPropietario = reader.GetInt32("IdPropietario");
                                contrato.Monto = reader.GetDecimal("Monto");
                                contrato.FechaInicio = reader.GetDateTime("FechaI");
                                contrato.FechaFin = reader.GetDateTime("FechaF");
                                contrato.estado = reader.GetBoolean("estado");



                                contratos.Add(contrato);
                            }
                            await connection.CloseAsync();
                            if (contratos.Count == 0) return ("No se encontraron contratos vigentes.", null);

                            return (null, contratos);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(ContratoService), nameof(GetContratosVigentes));
                return (ex.Message, null);
            }
        }
        public async Task<(string?, bool)> ValidarNoSuperposicionFechas(int idInmueble, DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                if ((await _inmuebleService.ObtenerInmueblePorId(idInmueble)).Item2 == null)
                    return ($"No se encontró un inmueble con Id {idInmueble}.", false);
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = "SELECT COUNT(*) FROM Contrato WHERE IdInmueble = @IdInmueble AND (FechaInicio <= @FechaFin AND FechaFin >= @FechaInicio) AND estado = 1";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdInmueble", idInmueble);
                        command.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                        command.Parameters.AddWithValue("@FechaFin", fechaFin);
                        await connection.OpenAsync();
                        int count = Convert.ToInt32(await command.ExecuteScalarAsync());
                        await connection.CloseAsync();
                        if (count > 0) return ("Las fechas ingresadas se superponen con otras fechas de contratos vigentes.", false);
                        return (null, true);
                    }
                }
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(ContratoService), nameof(ValidarNoSuperposicionFechas));
                return ("Error al validar las fechas: Internal Server Error", false);
            }
        }

        public async Task<(string?, bool)> ComprobarContratoActivoPorIdInmueble(int idInmueble) //testear
        {
            if (idInmueble <= 0) return ("El id del inmueble debe ser un número positivo.", false);
            try
            {
                if ((await _inmuebleService.ObtenerInmueblePorId(idInmueble)).Item2 == null)
                    return ($"No se encontró un inmueble con Id {idInmueble}.", false);
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = "SELECT COUNT(*) FROM Contrato WHERE IdInmueble = @IdInmueble AND FechaFin >= CURDATE() AND Activo = 1";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdInmueble", idInmueble);
                        await connection.OpenAsync();
                        int count = Convert.ToInt32(await command.ExecuteScalarAsync());
                        bool tieneContratoActivo = count > 0;
                        await connection.CloseAsync();
                        return (null, tieneContratoActivo);
                    }
                }
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(ContratoService), nameof(ComprobarContratoActivoPorIdInmueble));
                return (ex.Message, false);
            }
        }
        public async Task<(string?, List<Contrato>?)> GetContratosAPI() //testear
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = "SELECT * FROM Contrato";
                    List<Contrato> contratos = new List<Contrato>();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        connection.Open();
                        using (DbDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Contrato contrato = new Contrato();
                                contrato.IdContrato = reader.GetInt32("IdContrato");
                                contrato.IdInquilino = reader.GetInt32("IdInquilino");
                                contrato.IdInmueble = reader.GetInt32("IdInmueble");
                                contrato.IdPropietario = reader.GetInt32("IdPropietario");
                                contrato.Monto = reader.GetDecimal("Monto");
                                contrato.FechaInicio = reader.GetDateTime("FechaInicio");
                                contrato.FechaFin = reader.GetDateTime("FechaFin");
                                contrato.estado = reader.GetBoolean("estado");
                                contratos.Add(contrato);
                            }
                        }
                        await connection.CloseAsync();
                        return (null, contratos);
                    }
                }
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(ContratoService), nameof(GetContratosAPI));
                return (ex.Message, null);
            }
        }
        public async Task<(string?, bool)> TerminarContrato(int idContrato) //testear
        {
            if (idContrato <= 0) return ("El id del contrato debe ser un número positivo.", false);
            try
            {
                if ((await GetContratoById(idContrato)).Item2 == null)
                    return ($"No se encontró un contrato con Id {idContrato}.", false);
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = "UPDATE Contrato SET estado = 0 WHERE IdContrato = @IdContrato";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdContrato", idContrato);
                        await connection.OpenAsync();
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        await connection.CloseAsync();
                        if (rowsAffected == 0)
                        {
                            return ($"No se pudo terminar el contrato con Id {idContrato}.", false);
                        }
                        return (null, true);
                    }
                }
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(ContratoService), nameof(TerminarContrato));
                return (ex.Message, false);

            }
        }
        public async Task<(string?, bool)> RenovarContrato(int idContrato, DateTime nuevaFechaInicio, DateTime nuevaFechaFin, decimal nuevoMonto)
        {
            if (idContrato <= 0) return ("El id del contrato debe ser un número positivo.", false);
            if (nuevaFechaFin <= nuevaFechaInicio) return ("La nueva fecha de fin debe ser posterior a la de inicio.", false);
            if (nuevoMonto <= 0) return ("El nuevo monto debe ser un valor positivo.", false);
            try
            {
                (string?, Contrato?) contratoExistente = await GetContratoById(idContrato);
                if (contratoExistente.Item1 != null) return (contratoExistente.Item1, false);
                if (contratoExistente.Item2 == null) return ($"No se encontró un contrato con Id {idContrato}.", false);
                if (contratoExistente.Item2.estado == false) return ("No se puede renovar un contrato que está dado de baja.", false);

                // Crear nuevo contrato con los mismos datos pero fechas y monto nuevos
                Contrato nuevoContrato = new Contrato
                // Lo mandamos con el DTO que se hizo en el FRONT, me da paja hacerlo en el back
                {
                    IdInquilino = contratoExistente.Item2.IdInquilino,
                    IdPropietario = contratoExistente.Item2.IdPropietario,
                    IdInmueble = contratoExistente.Item2.IdInmueble,
                    Monto = nuevoMonto,
                    FechaInicio = nuevaFechaInicio,
                    FechaFin = nuevaFechaFin,
                    estado = true
                };
                var creado = await CreateContrato(nuevoContrato);
                if (creado.Item1 != null || !creado.Item2)
                    return ($"No se pudo crear el nuevo contrato: {creado.Item1}", false);

                // Dar de baja el contrato anterior
                var baja = await TerminarContrato(idContrato);
                if (baja.Item1 != null || !baja.Item2)
                    return ($"El nuevo contrato fue creado, pero no se pudo dar de baja el anterior: {baja.Item1}", false);

                return (null, true);
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(ContratoService), nameof(RenovarContrato));
                return (ex.Message, false);
            }
        }
        public async Task<(string?, int?)> CalcularMesesDeMulta(int idContrato)
        {
            try
            {
                if (idContrato <= 0) return ("El id del contrato debe ser un número positivo.", null);
                (string? error, Contrato? contrato) = await this.GetContratoById(idContrato);
                if (error != null) return (error, null);
                if (contrato == null) return ($"No se encontró un contrato con Id {idContrato}.", null);
                if (contrato.estado == false) return ("No se puede calcular la multa de un contrato que está dado de baja.", null);
                DateTime hoy = DateTime.Now;
                TimeSpan diferenciaEntreFechasDelContrato = contrato.FechaFin - contrato.FechaInicio;
                int diasTotales = (int)diferenciaEntreFechasDelContrato.TotalDays;
                int diasTranscurridos = (int)(hoy - contrato.FechaInicio).TotalDays;
                int mesesMulta = (diasTranscurridos < (diasTotales / 2)) ? 2 : 1;
                int valorMulta =(int)(contrato.Monto * mesesMulta);
                return (null, valorMulta);
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(ContratoService), nameof(CalcularMesesDeMulta));
                return ("Error al calcular la multa: Internal Server Error", null);
            }
        }
    }
}
