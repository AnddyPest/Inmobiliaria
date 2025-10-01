using MySql.Data.MySqlClient;
using project.Helpers;
using project.Models;
using project.Models.Interfaces;
using System.Data.Common;
using System.Data;


namespace project.Models.Interfaces
{
    public class AuditoriaService(IConfiguration configuration, IContratoService contratoService, IPagosService pagosService) : IAuditoriaService
    {
        private readonly string _connectionString = configuration.GetConnectionString("Connection")!;
        private IContratoService _contratoService = contratoService;
        private IPagosService _pagosService = pagosService;
        public async Task<(string?, bool)> CreateAuditoria(Auditoria auditoria)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = @"INSERT INTO auditoria (IdUsuario, IdContrato, IdPago, Fecha, MotivoAuditoria)
                                     VALUES (@IdUsuario, @IdContrato, @IdPago, @Fecha, @MotivoAuditoria);";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdUsuario", auditoria.IdUsuario);
                        command.Parameters.AddWithValue("@IdContrato", (object?)auditoria.IdContrato ?? System.DBNull.Value);
                        command.Parameters.AddWithValue("@IdPago", (object?)auditoria.IdPago ?? System.DBNull.Value);
                        command.Parameters.AddWithValue("@Fecha", DateTime.Now);
                        command.Parameters.AddWithValue("@IdMotivoAuditoria", auditoria.MotivoAuditoria);

                        await connection.OpenAsync();
                        int result = await command.ExecuteNonQueryAsync();
                        await connection.CloseAsync();

                        if (result > 0)
                        {
                            return (null, true);
                        }
                        else
                        {
                            return ("Error al crear auditoría: Database Error", false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return ($"Error al crear auditoría: {ex.Message}", false);
            }
        }

        public async Task<(string?, List<Auditoria>?)> GetAllAuditorias(int? nroPagina, int? registrosPorPagina, DateTime? fechaInicio, DateTime? fechaFin, string? accion, int? idUsuario)
        {
            try
            {
                var auditorias = new List<Auditoria>();
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = @"SELECT aud.*, c.*, p.*, m.*, usu.idUsuario, e.*, per.*
                                    FROM auditoria aud
                                    LEFT JOIN contrato c ON aud.IdContrato = c.IdContrato
                                    LEFT JOIN pagos p ON aud.IdPago = p.IdPago
                                    LEFT JOIN usuario usu ON aud.IdUsuario = usu.IdUsuario
                                    LEFT JOIN empleado e ON usu.IdUsuario = e.IdUsuario
                                    LEFT JOIN Rol r ON usu.idRol = r.nombre
                                    LEFT JOIN persona per ON e.IdPersona = per.IdPersona
                                    ORDER BY aud.Fecha DESC";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        await connection.OpenAsync();
                        using (DbDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var auditoria = new Auditoria();
                                auditoria.IdAuditoria = reader.GetInt32(reader.GetOrdinal("IdAuditoria"));
                                auditoria.IdUsuario = reader.GetInt32(reader.GetOrdinal("IdUsuario"));
                                auditoria.IdContrato = reader.IsDBNull(reader.GetOrdinal("IdContrato")) ? null : reader.GetInt32(reader.GetOrdinal("IdContrato"));
                                auditoria.IdPago = reader.IsDBNull(reader.GetOrdinal("IdPago")) ? null : reader.GetInt32(reader.GetOrdinal("IdPago"));
                                auditoria.Fecha = reader.GetDateTime(reader.GetOrdinal("Fecha"));
                                auditoria.MotivoAuditoria = reader.GetString(reader.GetOrdinal("MotivoAuditoria"));

                                Contrato? contrato = null;
                                if (!reader.IsDBNull(reader.GetOrdinal("c.IdContrato")))
                                {
                                    contrato = new Contrato
                                    {
                                        IdContrato = reader.GetInt32(reader.GetOrdinal("c.IdContrato")),
                                        // MAXI, que parametros te parece sacar aca?
                                    };
                                }

                                Pago? pago = null;
                                if (!reader.IsDBNull(reader.GetOrdinal("p.IdPago")))
                                {
                                    pago = new Pago
                                    {
                                        IdPago = reader.GetInt32(reader.GetOrdinal("p.IdPago")),
                                        // Y aca?
                                    };
                                }
                                Rol? rol = null;
                                if (!reader.IsDBNull(reader.GetOrdinal("r.idRol")))
                                {
                                    rol = new Rol
                                    {
                                        Nombre = reader.GetString(reader.GetOrdinal("r.nombre"))
                                    };
                                }
                                Persona? persona = null;
                                if (!reader.IsDBNull(reader.GetOrdinal("per.IdPersona")))
                                {
                                    persona = new Persona
                                    {
                                        IdPersona = reader.GetInt32(reader.GetOrdinal("per.IdPersona")),
                                        Nombre = reader.GetString(reader.GetOrdinal("per.Nombre")),
                                        Apellido = reader.GetString(reader.GetOrdinal("per.Apellido")),
                                        Dni = reader.GetInt32(reader.GetOrdinal("per.Dni")),
                                        Telefono = reader.GetString(reader.GetOrdinal("per.Telefono")),
                                        Direccion = reader.GetString(reader.GetOrdinal("per.Direccion")),
                                        Email = reader.GetString(reader.GetOrdinal("per.Email")),
                                        Estado = reader.GetBoolean(reader.GetOrdinal("per.Estado"))
                                    };
                                }
                                MotivosAuditoria? motivo = null;
                                if (!reader.IsDBNull(reader.GetOrdinal("m.IdMotivoAuditoria")))
                                {
                                    motivo = new MotivosAuditoria
                                    {

                                        Motivo = reader.GetString(reader.GetOrdinal("m.motivo"))
                                    };
                                }

                                auditorias.Add(auditoria);
                            }
                        }
                        await connection.CloseAsync();
                    }
                }
                return (null, auditorias);
            }
            catch (Exception ex)
            {
                return ($"Error al obtener auditorías: {ex.Message}", null);
            }
        }

        public async Task<(string?, Auditoria?)> GetAuditoriaById(int idAuditoria)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = @"SELECT aud.*, c.*, p.*, m.*, usu.idUsuario, e.*, per.*
                                    FROM auditoria aud
                                    LEFT JOIN contrato c ON aud.IdContrato = c.IdContrato
                                    LEFT JOIN pagos p ON aud.IdPago = p.IdPago
                                    LEFT JOIN usuario usu ON aud.IdUsuario = usu.IdUsuario
                                    LEFT JOIN empleado e ON usu.IdUsuario = e.IdUsuario
                                    LEFT JOIN Rol r ON usu.idRol = r.nombre
                                    LEFT JOIN persona per ON e.IdPersona = per.IdPersona
                                    WHERE aud.IdAuditoria = @IdAuditoria";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdAuditoria", idAuditoria);
                        await connection.OpenAsync();
                        using (DbDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var auditoria = new Auditoria();
                                auditoria.IdAuditoria = reader.GetInt32(reader.GetOrdinal("IdAuditoria"));
                                auditoria.IdUsuario = reader.GetInt32(reader.GetOrdinal("IdUsuario"));
                                auditoria.IdContrato = reader.IsDBNull(reader.GetOrdinal("IdContrato")) ? null : reader.GetInt32(reader.GetOrdinal("IdContrato"));
                                auditoria.IdPago = reader.IsDBNull(reader.GetOrdinal("IdPago")) ? null : reader.GetInt32(reader.GetOrdinal("IdPago"));
                                auditoria.Fecha = reader.GetDateTime(reader.GetOrdinal("Fecha"));
                                auditoria.MotivoAuditoria = reader.GetString(reader.GetOrdinal("MotivoAuditoria"));

                                Contrato? contrato = null;
                                if (!reader.IsDBNull(reader.GetOrdinal("c.IdContrato")))
                                {
                                    contrato = new Contrato
                                    {
                                        IdContrato = reader.GetInt32(reader.GetOrdinal("c.IdContrato")),
                                        // MAXI, que parametros te parece sacar aca?
                                    };
                                }

                                Pago? pago = null;
                                if (!reader.IsDBNull(reader.GetOrdinal("p.IdPago")))
                                {
                                    pago = new Pago
                                    {
                                        IdPago = reader.GetInt32(reader.GetOrdinal("p.IdPago")),
                                        // Y aca?
                                    };
                                }
                                Rol? rol = null;
                                if (!reader.IsDBNull(reader.GetOrdinal("r.idRol")))
                                {
                                    rol = new Rol
                                    {
                                        Nombre = reader.GetString(reader.GetOrdinal("r.nombre"))
                                    };
                                }
                                Persona? persona = null;
                                if (!reader.IsDBNull(reader.GetOrdinal("per.IdPersona")))
                                {
                                    persona = new Persona
                                    {
                                        IdPersona = reader.GetInt32(reader.GetOrdinal("per.IdPersona")),
                                        Nombre = reader.GetString(reader.GetOrdinal("per.Nombre")),
                                        Apellido = reader.GetString(reader.GetOrdinal("per.Apellido")),
                                        Dni = reader.GetInt32(reader.GetOrdinal("per.Dni")),
                                        Telefono = reader.GetString(reader.GetOrdinal("per.Telefono")),
                                        Direccion = reader.GetString(reader.GetOrdinal("per.Direccion")),
                                        Email = reader.GetString(reader.GetOrdinal("per.Email")),
                                        Estado = reader.GetBoolean(reader.GetOrdinal("per.Estado"))
                                    };
                                }
                                MotivosAuditoria? motivo = null;
                                if (!reader.IsDBNull(reader.GetOrdinal("m.IdMotivoAuditoria")))
                                {
                                    motivo = new MotivosAuditoria
                                    {

                                        Motivo = reader.GetString(reader.GetOrdinal("m.motivo"))
                                    };
                                }


                                return (null, auditoria);
                            }
                            else
                            {
                                return ("Auditoría no encontrada", null);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return ($"Error al obtener auditoría: {ex.Message}", null);
            }
        }

        public async Task<(string?, List<Auditoria>?)> GetAuditoriasByContrato(int idContrato)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = @"SELECT aud.*, c.*, p.*, m.*, usu.idUsuario, e.*, per.*
                                    FROM auditoria aud
                                    LEFT JOIN contrato c ON aud.IdContrato = c.IdContrato
                                    LEFT JOIN pagos p ON aud.IdPago = p.IdPago
                                    LEFT JOIN usuario usu ON aud.IdUsuario = usu.IdUsuario
                                    LEFT JOIN empleado e ON usu.IdUsuario = e.IdUsuario
                                    LEFT JOIN Rol r ON usu.idRol = r.nombre
                                    LEFT JOIN persona per ON e.IdPersona = per.IdPersona
                                    WHERE aud.IdContrato = @IdContrato
                                    ORDER BY aud.Fecha DESC";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdContrato", idContrato);
                        var auditorias = new List<Auditoria>();
                        await connection.OpenAsync();
                        using (DbDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var auditoria = new Auditoria();
                                auditoria.IdAuditoria = reader.GetInt32(reader.GetOrdinal("IdAuditoria"));
                                auditoria.IdUsuario = reader.GetInt32(reader.GetOrdinal("IdUsuario"));
                                auditoria.IdContrato = reader.IsDBNull(reader.GetOrdinal("IdContrato")) ? null : reader.GetInt32(reader.GetOrdinal("IdContrato"));
                                auditoria.IdPago = reader.IsDBNull(reader.GetOrdinal("IdPago")) ? null : reader.GetInt32(reader.GetOrdinal("IdPago"));
                                auditoria.Fecha = reader.GetDateTime(reader.GetOrdinal("Fecha"));
                                auditoria.MotivoAuditoria = reader.GetString(reader.GetOrdinal("MotivoAuditoria"));

                                Contrato? contrato = null;
                                if (!reader.IsDBNull(reader.GetOrdinal("c.IdContrato")))
                                {
                                    contrato = new Contrato
                                    {
                                        IdContrato = reader.GetInt32(reader.GetOrdinal("c.IdContrato")),
                                        // MAXI, que parametros te parece sacar aca?
                                    };
                                }

                                Pago? pago = null;
                                if (!reader.IsDBNull(reader.GetOrdinal("p.IdPago")))
                                {
                                    pago = new Pago
                                    {
                                        IdPago = reader.GetInt32(reader.GetOrdinal("p.IdPago")),
                                        // Y aca?
                                    };
                                }
                                Rol? rol = null;
                                if (!reader.IsDBNull(reader.GetOrdinal("r.idRol")))
                                {
                                    rol = new Rol
                                    {
                                        Nombre = reader.GetString(reader.GetOrdinal("r.nombre"))
                                    };
                                }
                                Persona? persona = null;
                                if (!reader.IsDBNull(reader.GetOrdinal("per.IdPersona")))
                                {
                                    persona = new Persona
                                    {
                                        IdPersona = reader.GetInt32(reader.GetOrdinal("per.IdPersona")),
                                        Nombre = reader.GetString(reader.GetOrdinal("per.Nombre")),
                                        Apellido = reader.GetString(reader.GetOrdinal("per.Apellido")),
                                        Dni = reader.GetInt32(reader.GetOrdinal("per.Dni")),
                                        Telefono = reader.GetString(reader.GetOrdinal("per.Telefono")),
                                        Direccion = reader.GetString(reader.GetOrdinal("per.Direccion")),
                                        Email = reader.GetString(reader.GetOrdinal("per.Email")),
                                        Estado = reader.GetBoolean(reader.GetOrdinal("per.Estado"))
                                    };
                                }
                                MotivosAuditoria? motivo = null;
                                if (!reader.IsDBNull(reader.GetOrdinal("m.IdMotivoAuditoria")))
                                {
                                    motivo = new MotivosAuditoria
                                    {

                                        Motivo = reader.GetString(reader.GetOrdinal("m.motivo"))
                                    };
                                }

                                auditorias.Add(auditoria);
                            }
                        }
                        return (null, auditorias);
                    }
                }
            }
            catch (Exception ex)
            {
                return ($"Error al obtener auditorías por contrato: {ex.Message}", null);
            }
        }

        public async Task<(string?, List<Auditoria>?)> GetAuditoriasByIdUsuario(int idUsuario)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = @"SELECT aud.*, c.*, p.*, m.*, usu.idUsuario, e.*, per.*
                                    FROM auditoria aud
                                    LEFT JOIN contrato c ON aud.IdContrato = c.IdContrato
                                    LEFT JOIN pagos p ON aud.IdPago = p.IdPago
                                    LEFT JOIN usuario usu ON aud.IdUsuario = usu.IdUsuario
                                    LEFT JOIN empleado e ON usu.IdUsuario = e.IdUsuario
                                    LEFT JOIN Rol r ON usu.idRol = r.nombre
                                    LEFT JOIN persona per ON e.IdPersona = per.IdPersona
                                    WHERE aud.IdUsuario = @IdUsuario
                                    ORDER BY aud.Fecha DESC";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdUsuario", idUsuario);
                        var auditorias = new List<Auditoria>();
                        await connection.OpenAsync();
                        using (DbDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var auditoria = new Auditoria();
                                auditoria.IdAuditoria = reader.GetInt32(reader.GetOrdinal("IdAuditoria"));
                                auditoria.IdUsuario = reader.GetInt32(reader.GetOrdinal("IdUsuario"));
                                auditoria.IdContrato = reader.IsDBNull(reader.GetOrdinal("IdContrato")) ? null : reader.GetInt32(reader.GetOrdinal("IdContrato"));
                                auditoria.IdPago = reader.IsDBNull(reader.GetOrdinal("IdPago")) ? null : reader.GetInt32(reader.GetOrdinal("IdPago"));
                                auditoria.Fecha = reader.GetDateTime(reader.GetOrdinal("Fecha"));
                                auditoria.MotivoAuditoria = reader.GetString(reader.GetOrdinal("MotivoAuditoria"));

                                Contrato? contrato = null;
                                if (!reader.IsDBNull(reader.GetOrdinal("c.IdContrato")))
                                {
                                    contrato = new Contrato
                                    {
                                        IdContrato = reader.GetInt32(reader.GetOrdinal("c.IdContrato")),
                                        // MAXI, que parametros te parece sacar aca?
                                    };
                                }

                                Pago? pago = null;
                                if (!reader.IsDBNull(reader.GetOrdinal("p.IdPago")))
                                {
                                    pago = new Pago
                                    {
                                        IdPago = reader.GetInt32(reader.GetOrdinal("p.IdPago")),
                                        // Y aca?
                                    };
                                }
                                Rol? rol = null;
                                if (!reader.IsDBNull(reader.GetOrdinal("r.idRol")))
                                {
                                    rol = new Rol
                                    {
                                        Nombre = reader.GetString(reader.GetOrdinal("r.nombre"))
                                    };
                                }
                                Persona? persona = null;
                                if (!reader.IsDBNull(reader.GetOrdinal("per.IdPersona")))
                                {
                                    persona = new Persona
                                    {
                                        IdPersona = reader.GetInt32(reader.GetOrdinal("per.IdPersona")),
                                        Nombre = reader.GetString(reader.GetOrdinal("per.Nombre")),
                                        Apellido = reader.GetString(reader.GetOrdinal("per.Apellido")),
                                        Dni = reader.GetInt32(reader.GetOrdinal("per.Dni")),
                                        Telefono = reader.GetString(reader.GetOrdinal("per.Telefono")),
                                        Direccion = reader.GetString(reader.GetOrdinal("per.Direccion")),
                                        Email = reader.GetString(reader.GetOrdinal("per.Email")),
                                        Estado = reader.GetBoolean(reader.GetOrdinal("per.Estado"))
                                    };
                                }
                                MotivosAuditoria? motivo = null;
                                if (!reader.IsDBNull(reader.GetOrdinal("m.IdMotivoAuditoria")))
                                {
                                    motivo = new MotivosAuditoria
                                    {

                                        Motivo = reader.GetString(reader.GetOrdinal("m.motivo"))
                                    };
                                }
                                auditorias.Add(auditoria);
                            }
                        }
                        await connection.CloseAsync();
                        return (null, auditorias);
                    }
                }
            }
            catch (Exception ex)
            {
                return ($"Error al obtener auditorías por usuario: {ex.Message}", null);
            }
        }

        public async Task<(string?, List<Auditoria>?)> GetAuditoriasByPago(int idPago)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    string query = @"SELECT aud.*, c.*, p.*, m.*, usu.idUsuario, e.*, per.*
                                    FROM auditoria aud
                                    LEFT JOIN contrato c ON aud.IdContrato = c.IdContrato
                                    LEFT JOIN pagos p ON aud.IdPago = p.IdPago
                                    LEFT JOIN usuario usu ON aud.IdUsuario = usu.IdUsuario
                                    LEFT JOIN empleado e ON usu.IdUsuario = e.IdUsuario
                                    LEFT JOIN Rol r ON usu.idRol = r.nombre
                                    LEFT JOIN persona per ON e.IdPersona = per.IdPersona
                                    WHERE aud.IdPago = @IdPago
                                    ORDER BY aud.Fecha DESC";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdPago", idPago);
                        var auditorias = new List<Auditoria>();
                        await connection.OpenAsync();
                        using (DbDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var auditoria = new Auditoria();
                                auditoria.IdAuditoria = reader.GetInt32(reader.GetOrdinal("IdAuditoria"));
                                auditoria.IdUsuario = reader.GetInt32(reader.GetOrdinal("IdUsuario"));
                                auditoria.IdContrato = reader.IsDBNull(reader.GetOrdinal("IdContrato")) ? null : reader.GetInt32(reader.GetOrdinal("IdContrato"));
                                auditoria.IdPago = reader.IsDBNull(reader.GetOrdinal("IdPago")) ? null : reader.GetInt32(reader.GetOrdinal("IdPago"));
                                auditoria.Fecha = reader.GetDateTime(reader.GetOrdinal("Fecha"));
                                auditoria.MotivoAuditoria = reader.GetString(reader.GetOrdinal("MotivoAuditoria"));

                                Contrato? contrato = null;
                                if (!reader.IsDBNull(reader.GetOrdinal("c.IdContrato")))
                                {
                                    contrato = new Contrato
                                    {
                                        IdContrato = reader.GetInt32(reader.GetOrdinal("c.IdContrato")),
                                        // MAXI, que parametros te parece sacar aca?
                                    };
                                }

                                Pago? pago = null;
                                if (!reader.IsDBNull(reader.GetOrdinal("p.IdPago")))
                                {
                                    pago = new Pago
                                    {
                                        IdPago = reader.GetInt32(reader.GetOrdinal("p.IdPago")),
                                        // Y aca?
                                    };
                                }
                                Rol? rol = null;
                                if (!reader.IsDBNull(reader.GetOrdinal("r.idRol")))
                                {
                                    rol = new Rol
                                    {
                                        Nombre = reader.GetString(reader.GetOrdinal("r.nombre"))
                                    };
                                }
                                Persona? persona = null;
                                if (!reader.IsDBNull(reader.GetOrdinal("per.IdPersona")))
                                {
                                    persona = new Persona
                                    {
                                        IdPersona = reader.GetInt32(reader.GetOrdinal("per.IdPersona")),
                                        Nombre = reader.GetString(reader.GetOrdinal("per.Nombre")),
                                        Apellido = reader.GetString(reader.GetOrdinal("per.Apellido")),
                                        Dni = reader.GetInt32(reader.GetOrdinal("per.Dni")),
                                        Telefono = reader.GetString(reader.GetOrdinal("per.Telefono")),
                                        Direccion = reader.GetString(reader.GetOrdinal("per.Direccion")),
                                        Email = reader.GetString(reader.GetOrdinal("per.Email")),
                                        Estado = reader.GetBoolean(reader.GetOrdinal("per.Estado"))
                                    };
                                }
                                MotivosAuditoria? motivo = null;
                                if (!reader.IsDBNull(reader.GetOrdinal("m.IdMotivoAuditoria")))
                                {
                                    motivo = new MotivosAuditoria
                                    {

                                        Motivo = reader.GetString(reader.GetOrdinal("m.motivo"))
                                    };
                                }

                                auditorias.Add(auditoria);
                            }
                        }
                        await connection.CloseAsync();
                        return (null, auditorias);
                    }
                }
            }
            catch (Exception ex)
            {
                return ($"Error al obtener auditorías por pago: {ex.Message}", null);
            }
        }
    }
}