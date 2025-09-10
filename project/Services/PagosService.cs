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
                    command.Parameters.AddWithValue("@fechaConfeccion", DateTime.Now);
                    object result = command.ExecuteScalar();
                    if (result != null && int.TryParse(result.ToString(), out res))
                    {
                        pago.IdPago = res;
                        pago.Numero = numero;
                        return Task.FromResult<(string?, bool)>((null, true));
                    }
                    else
                    {
                        return Task.FromResult<(string?, bool)>(("No se pudo obtener el ID del nuevo pago.", false));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            HelperFor.imprimirMensajeDeError(ex.Message, nameof(PagosService), nameof(CreatePago));
            return Task.FromResult<(string?, bool)>(("Error al crear el pago: " + ex.Message, false));
        }
        throw new NotImplementedException();
    }

    public Task<(string?, bool)> UpdatePago(Pago pago)
    {
        throw new NotImplementedException();
    }

    public Task<(string?, bool)> AsentarPago(Pago pago)
    {
        throw new NotImplementedException();
    }

    public Task<(string?, List<Pago>?)> GetAllPagos(int? nroPagina, int? registrosPorPagina)
    {
        throw new NotImplementedException();
    }

    public Task<(string?, List<Pago>?)> GetPagosByIdContrato(int idContrato)
    {
        throw new NotImplementedException();
    }

    public Task<(string?, Pago?)> GetPagoById(int idPago)
    {
        throw new NotImplementedException();
    }

    public Task<(string?, bool)> AnularPago(int idPago)
    {
        throw new NotImplementedException();
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
                                   AND fechaConfeccion = @fechaConfeccion";

                Console.WriteLine($"[SQL] {query}");
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@idContrato", idContrato);
                    // Enviar la fecha en formato yyyy-MM-dd
                    command.Parameters.AddWithValue("@fechaConfeccion", fechaConfeccion.ToString("yyyy-MM-dd"));
                    Console.WriteLine($"[PARAMS] idContrato={idContrato}, fechaConfeccion={fechaConfeccion:yyyy-MM-dd}");
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
}