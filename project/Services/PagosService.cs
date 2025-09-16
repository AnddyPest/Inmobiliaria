using MySql.Data.MySqlClient;
using project.Helpers;
using project.Models;
using project.Models.Interfaces;
using System.Data;

public class PagosService : IPagosService
{
    private readonly string _connectionString;

    public PagosService(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public Task<(string?, bool)> CreatePago(Pago pago)
    {
        int res = -1;
        try
        {
            // Validación de pago duplicado
            var existePago = ExistePagoAlquiler(pago.IdContrato, pago.FechaConfeccion).Result;
            if (existePago)
            {
                return Task.FromResult<(string?, bool)>( ("Ya existe un pago de alquiler para este mes", false) );
            }

            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                // Obtener el último número de pago para el contrato
                int numero = 1;
                using (var cmdNum = new MySqlCommand("SELECT MAX(numero) FROM pagos WHERE idContrato = @idContrato", connection))
                {
                    cmdNum.Parameters.AddWithValue("@idContrato", pago.IdContrato);
                    var resultNum = cmdNum.ExecuteScalar();
                    if (resultNum != DBNull.Value && resultNum != null)
                    {
                        int lastNum;
                        if (int.TryParse(resultNum.ToString(), out lastNum))
                            numero = lastNum + 1;
                    }
                }
                string query = @"INSERT INTO pagos (idContrato, numero, detalle, importe, abonado, alquiler, estado, fechaConfeccion) 
                                 VALUES (@idContrato, @numero, @detalle, @importe, @abonado, @alquiler, @estado, @fechaConfeccion); 
                                 SELECT LAST_INSERT_ID();";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@idContrato", pago.IdContrato);
                    command.Parameters.AddWithValue("@numero", numero);
                    command.Parameters.AddWithValue("@detalle", pago.Detalle);
                    command.Parameters.AddWithValue("@importe", pago.Importe);
                    command.Parameters.AddWithValue("@abonado", pago.Abonado);
                    command.Parameters.AddWithValue("@alquiler", pago.Alquiler);
                    command.Parameters.AddWithValue("@estado", pago.Estado);
                    command.Parameters.AddWithValue("@fechaConfeccion", pago.FechaConfeccion.ToDateTime(TimeOnly.MinValue));
                    object result = command.ExecuteScalar();
                    if (result != null && int.TryParse(result.ToString(), out res))
                    {
                        pago.IdPago = res;
                        pago.Numero = numero;
                        return Task.FromResult<(string?, bool)>((null, true));
                    }
                    else
                    {
                        return Task.FromResult<(string?, bool)>( ("No se pudo obtener el ID del nuevo pago.", false) );
                    }
                }
            }
        }
        catch (Exception ex)
        {
            HelperFor.imprimirMensajeDeError(ex.Message, nameof(PagosService), nameof(CreatePago));
            return Task.FromResult<(string?, bool)>( ("Error al crear el pago: " + ex.Message, false) );
        }
        throw new NotImplementedException();
    }

    public Task<(string?, bool)> UpdatePago(Pago pago)
    {
        throw new NotImplementedException();
    }

    public Task<(string?, bool)> AsentarPago(Pago pago)
    {
        try
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = @"UPDATE pagos 
                                 SET abonado = @abonado, 
                                     fechaPago = @fechaPago 
                                 WHERE idPago = @idPago";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@abonado", true);
                    command.Parameters.AddWithValue("@fechaPago", DateTime.Now);
                    command.Parameters.AddWithValue("@idPago", pago.IdPago);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        return Task.FromResult<(string?, bool)>((null, true));
                    }
                    else
                    {
                        return Task.FromResult<(string?, bool)>(("No se encontró el pago para actualizar.", false));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            HelperFor.imprimirMensajeDeError(ex.Message, nameof(PagosService), nameof(AsentarPago));
            return Task.FromResult<(string?, bool)>((ex.Message, false));
        }
    }

    public Task<(string?, List<Pago>?)> GetAllPagos(int? nroPagina, int? registrosPorPagina)
    {
        throw new NotImplementedException();
    }

    public Task<(string?, List<Pago>?)> GetPagosByIdContrato(int? nroPagina, int? registrosPorPagina, int idContrato)
    {
        
        try
        {
            List<Pago> pagos = new List<Pago>();
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                int page = nroPagina ?? 1;
                int pageSize = registrosPorPagina ?? 10;
                int offset = (page - 1) * pageSize;
                string query = "SELECT * FROM pagos WHERE idContrato = @idContrato  ORDER BY fechaConfeccion DESC LIMIT @limit OFFSET @offset";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@idContrato", idContrato);
                    command.Parameters.AddWithValue("@limit", registrosPorPagina);
                    command.Parameters.AddWithValue("@offset", offset);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Pago pago = new Pago
                            {
                                IdPago = reader.GetInt32("idPago"),
                                IdContrato = reader.GetInt32("idContrato"),
                                Numero = reader.GetInt32("numero"),
                                Detalle = reader.GetString("detalle"),
                                Importe = reader.GetDecimal("importe"),
                                Abonado = reader.GetBoolean("abonado"),
                                Alquiler = reader.GetBoolean("alquiler"),
                                Estado = reader.GetBoolean("estado"),
                                FechaConfeccion = DateOnly.FromDateTime(reader.GetDateTime("fechaConfeccion")),
                                FechaPago = reader.IsDBNull(reader.GetOrdinal("fechaPago")) ? null : DateOnly.FromDateTime(reader.GetDateTime("fechaPago"))
                            };
                            pagos.Add(pago);
                        }
                    }
                }
            }
            return Task.FromResult<(string?, List<Pago>?)>( (null, pagos) );
         }
        catch (Exception ex)
        {
            HelperFor.imprimirMensajeDeError(ex.Message, nameof(PagosService), nameof(GetPagosByIdContrato));
            return Task.FromResult<(string?, List<Pago>?)>((ex.Message, null));
        }
    }

    public Task<(string?, Pago?)> GetPagoById(int idPago)
    {
        try
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM pagos WHERE idPago = @idPago LIMIT 1";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@idPago", idPago);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Pago pago = new Pago
                            {
                                IdPago = reader.GetInt32("idPago"),
                                IdContrato = reader.GetInt32("idContrato"),
                                Numero = reader.GetInt32("numero"),
                                Detalle = reader.GetString("detalle"),
                                Importe = reader.GetDecimal("importe"),
                                Abonado = reader.GetBoolean("abonado"),
                                Alquiler = reader.GetBoolean("alquiler"),
                                Estado = reader.GetBoolean("estado"),
                                FechaConfeccion = DateOnly.FromDateTime(reader.GetDateTime("fechaConfeccion")),
                                FechaPago = reader.IsDBNull(reader.GetOrdinal("fechaPago")) ? null : DateOnly.FromDateTime(reader.GetDateTime("fechaPago"))
                            };
                            return Task.FromResult<(string?, Pago?)>((null, pago));
                        }
                    }
                }
            }
            return Task.FromResult<(string?, Pago?)>(("No se encontró el pago", null));
        }
        catch (Exception ex)
        {
            HelperFor.imprimirMensajeDeError(ex.Message, nameof(PagosService), nameof(GetPagoById));
            return Task.FromResult<(string?, Pago?)>((ex.Message, null));
        }
    }

    public Task<(string?, bool)> AnularPago(Pago pago)
    {
      try
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = @"UPDATE pagos 
                                 SET abonado = @abonado, 
                                     fechaPago = @fechaPago 
                                 WHERE idPago = @idPago";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@abonado", false);
                    command.Parameters.AddWithValue("@fechaPago", null);
                    command.Parameters.AddWithValue("@idPago", pago.IdPago);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        return Task.FromResult<(string?, bool)>((null, true));
                    }
                    else
                    {
                        return Task.FromResult<(string?, bool)>(("No se encontró el pago para actualizar.", false));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            HelperFor.imprimirMensajeDeError(ex.Message, nameof(PagosService), nameof(AsentarPago));
            return Task.FromResult<(string?, bool)>((ex.Message, false));
        }  
    }

    public Task<(string?, bool)> ReintegrarPago(int idPago)
    {
        throw new NotImplementedException();
    }

    public Task<(string?, List<Pago>?)> GetPagosByFecha(DateTime fecha)
    {
        throw new NotImplementedException();
    }

    public Task<(string?, List<Pago>?)> GetPagosPendientes()
    {
        throw new NotImplementedException();
    }

    public Task<(string?, List<Pago>?)> GetPagosRealizados()
    {
        throw new NotImplementedException();
    }

    public Task<(string?, List<Pago>?)> GetPagosAnulados()
    {
        throw new NotImplementedException();
    }

    public async Task<bool> ExistePagoAlquiler(int idContrato, DateOnly fechaConfeccion)
    {
        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = @"SELECT COUNT(*) FROM pagos 
                                 WHERE idContrato = @idContrato 
                                   AND alquiler = 1 
                                   AND estado = 1 
                                   AND MONTH(fechaConfeccion) = @mes 
                                   AND YEAR(fechaConfeccion) = @anio";

                Console.WriteLine($"[SQL] {query}");
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@idContrato", idContrato);
                    command.Parameters.AddWithValue("@mes", fechaConfeccion.Month);
                    command.Parameters.AddWithValue("@anio", fechaConfeccion.Year);
                    Console.WriteLine($"[PARAMS] idContrato={idContrato}, mes={fechaConfeccion.Month}, anio={fechaConfeccion.Year}");
                    var result = await command.ExecuteScalarAsync();
                    Console.WriteLine($"[RESULT] {result}");
                    if (result != null && int.TryParse(result.ToString(), out int count))
                    {
                        Console.WriteLine($"[COUNT] {count}");
                        return count > 0;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {ex.Message}");
            HelperFor.imprimirMensajeDeError(ex.Message, nameof(PagosService), nameof(ExistePagoAlquiler));
        }
        return false;
    }
    public Task<(string?, bool)> CrearMulta(int idContrato, decimal importe, string detalle)
    {
        throw new NotImplementedException();
    }

    public async Task<(string?, bool)> darDeBajaLogicaPago(int idPago)
    {
        try
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = @"UPDATE pagos 
                                 SET estado = 0 
                                 WHERE idPago = @idPago";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@idPago", idPago);
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    if (rowsAffected > 0)
                    {
                        await connection.CloseAsync();
                        return (null, true);
                    }
                    else
                    {
                        await connection.CloseAsync();
                        return ("No se encontró el pago para actualizar.", false);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            HelperFor.imprimirMensajeDeError(ex.Message, nameof(PagosService), nameof(darDeBajaLogicaPago));
            return (ex.Message, false);
            
        }
    }

    public async Task<(string?, bool)> darAltaLogicaPago(int idPago)
    {
        {
        try
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = @"UPDATE pagos 
                                 SET estado = 1
                                 WHERE idPago = @idPago";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@idPago", idPago);
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    if (rowsAffected > 0)
                    {
                        await connection.CloseAsync();
                        return (null, true);
                    }
                    else
                    {
                        await connection.CloseAsync();
                        return ("No se encontró el pago para actualizar.", false);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            HelperFor.imprimirMensajeDeError(ex.Message, nameof(PagosService), nameof(darDeBajaLogicaPago));
            return (ex.Message, false);
            
        }
    }
    }
}