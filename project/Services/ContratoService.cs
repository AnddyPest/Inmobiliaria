using MySql.Data.MySqlClient;
using project.Helpers;
using project.Models;
using project.Models.Interfaces;
using System.Data.Common;
using System.Data;


namespace project.Services
{
    public class ContratoService (IConfiguration configuration, IPropietarioService propietarioService, IInquilinoService inquilinoService) : IContratoService
    {
        private readonly string _connectionString = configuration.GetConnectionString("Connection")!;
        private IPropietarioService _propietarioService = propietarioService;
        private IInquilinoService _inquilinoService = inquilinoService;
        public async Task<(string?, bool)> CreateContrato(Contrato contrato) //testear
        {
            try
            {
                if(contrato == null) return ("El contrato no puede ser nulo.", false);
                if(this.ComprobarContratoActivoPorIdInmueble(contrato.IdInmueble).Result.Item2)
                    return ($"El inmueble con Id {contrato.IdInmueble} ya tiene un contrato activo.", false);
                (string?, Inquilino?) inquilinoFinded = await _inquilinoService.GetInquilinoById(contrato.IdInquilino);
                if ( inquilinoFinded.Item1 != null)
                    return ($"No se encontró un inquilino con Id {contrato.IdInquilino}.", false);
                (string?, Propietario?) propietarioFinded = await _propietarioService.getPropietarioById(contrato.IdPropietario);
                if (propietarioFinded.Item1 != null)
                    return ($"No se encontró un propietario con Id {contrato.IdPropietario}.", false);
                if (inquilinoFinded.Item2!.IdPersona == propietarioFinded.Item2!.IdPersona)
                    return ($"Un propietario no puede alquilar su propia propiedad", false);
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = @"INSERT INTO Contrato (IdInquilino, IdInmueble, IdPropietario, Monto, FechaInicio, FechaFin, estado) 
                                     VALUES (@IdInquilino, @IdInmueble, @IdPropietario, @Monto, @FechaInicio, @FechaFin, 1)";
                    using(MySqlCommand command = new MySqlCommand(query, connection))
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
            if(contrato == null) return ("El contrato no puede ser nulo.", false);
            if(contrato.IdContrato <= 0) return ("El id del contrato debe ser un número positivo.", false);
            (string?, Contrato?) contratoExistente = await GetContratoById(contrato.IdContrato);
            if(contratoExistente.Item1 != null) return (contratoExistente.Item1, false);
            if(contratoExistente.Item2 == null) return ($"No se encontró un contrato con Id {contrato.IdContrato}.", false);
            if(contratoExistente.Item2.estado == false) return ("No se puede actualizar un contrato que está dado de baja.", false);
            if(contratoExistente.Item2.IdInmueble != contrato.IdInmueble)
            {
                if(this.ComprobarContratoActivoPorIdInmueble(contrato.IdInmueble).Result.Item2)
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

            try
            {
                if((await GetContratoById(contrato.IdContrato)).Item2 == null)
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
            if(idContrato <= 0) return ("El id del contrato debe ser un número positivo.", false);
            try
            {
                if((await GetContratoById(idContrato)).Item2 == null)
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
            if(idContrato <= 0) return ("El id del contrato debe ser un número positivo.", false);
            try
            {
                if((await GetContratoById(idContrato)).Item2 == null)
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

        public async Task<(string?, List<Contrato>?)> GetAllContratos() //testear
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
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(ContratoService), nameof(GetAllContratos));
                return (ex.Message, null);
            }
        }

        public async Task<(string?, Contrato?)> GetContratoById(int idContrato) //testear
        {
            if(idContrato <= 0) return ("El id del contrato debe ser un número positivo.", null);
            try
            {
                using(MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = "SELECT * FROM Contrato WHERE IdContrato = @IdContrato";
                    using(MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdContrato", idContrato);
                        await connection.OpenAsync();
                        using(DbDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if(await reader.ReadAsync())
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
            catch ( Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(ContratoService), nameof(GetContratoById));
                return (ex.Message, null);
            }
        }

        public async Task<(string?, List<Contrato>?)> GetContratoByIdInmueble(int idInmueble) //testear
        {
            if(idInmueble <= 0) return ("El id del inmueble debe ser un número positivo.", null);
            try
            {
                if((await _propietarioService.getPropietarioById(idInmueble)).Item2 == null)
                    return ($"No se encontró un inmueble con Id {idInmueble}.", null);
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = "SELECT * FROM Contrato WHERE IdInmueble = @IdInmueble";
                    using(MySqlCommand command = new MySqlCommand(query, connection))
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
                HelperFor.imprimirMensajeDeError(ex.Message,nameof(ContratoService), nameof(GetContratoByIdInmueble));
                return (ex.Message, null);
            }
        }

        public async Task<(string?, List<Contrato>?)> GetContratosByIdInquilino(int idInquilino) //testear
        {
            if(idInquilino <= 0) return ("El id del inquilino debe ser un número positivo.", null);
            try
            {
                if((await _inquilinoService.GetInquilinoById(idInquilino)).Item2 == null)
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
            if(idPropietario <= 0) return ("El id del propietario debe ser un número positivo.", null);
            try
            {
                if((await _propietarioService.getPropietarioById(idPropietario)).Item2 == null)
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
                using(MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT * FROM Contrato WHERE estado = 1";
                    using(MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using(DbDataReader reader = await command.ExecuteReaderAsync())
                        {
                            List<Contrato> contratos = new List<Contrato>();
                            while(await reader.ReadAsync())
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
                            if(contratos.Count == 0) return ("No se encontraron contratos vigentes.", null);
                            
                            return (null, contratos);
                        }
                    }
                }
            }
            catch ( Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(ContratoService), nameof(GetContratosVigentes));
                return (ex.Message, null);
            }
        }

        public async Task<(string?, bool)> ComprobarContratoActivoPorIdInmueble(int idInmueble) //testear
        {
            if(idInmueble <= 0) return ("El id del inmueble debe ser un número positivo.", false);
            try
            {
                if((await _propietarioService.getPropietarioById(idInmueble)).Item2 == null)
                    return ($"No se encontró un inmueble con Id {idInmueble}.", false);
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = "SELECT COUNT(*) FROM Contrato WHERE IdInmueble = @IdInmueble AND FechaFin >= CURDATE() AND Activo = 1";
                    using(MySqlCommand command = new MySqlCommand(query, connection))
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
    }
}
