

using System.Data;
using Microsoft.Identity.Client;
using MySql.Data.MySqlClient;
using project.Helpers;
using project.Models;

public class EmpleadoService : IEmpleadoService
{
    public string connectionString;
    public EmpleadoService(IConfiguration config)
    {
        connectionString = config.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }



    public async Task<(string?, List<Empleado>?, int?)> ObtenerTodos(int? nroPagina, int? registrosPorPagina, int? estado, int? dni)
    {
        try
        {
            int cantidadRegistros = 0;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string query = @"   SELECT SQL_CALC_FOUND_ROWS
                                    empleado.idEmpleado, 
                                    persona.idPersona, 
                                    persona.dni, 
                                    persona.nombre, 
                                    persona.apellido, 
                                    persona.email, 
                                    persona.telefono, 
                                    persona.direccion, 
                                    empleado.estado,
                                    usuario.idUsuario,
                                    usuario.email,
                                    usuario.estado as estadoUsuario,
                                    rol.idRol,
                                    rol.nombre
                                    FROM empleado as empleado
                                    LEFT JOIN persona as persona
                                    ON empleado.idPersona = persona.idPersona
                                    LEFT JOIN usuario as usuario ON empleado.idUsuario = usuario.idUsuario
                                    LEFT JOIN rol as rol ON usuario.idRol = rol.idRol ";
                List<String> parametros = new List<string>();
                
                if (dni != null) parametros.Add($" persona.dni like '%{dni}%' ");
                if (estado != null) parametros.Add($" empleado.estado = {estado} ");
                query += HelperFor.construirSqlWhereAnd(parametros);
                query += $" ORDER BY empleado.idEmpleado LIMIT {registrosPorPagina}\n OFFSET {(nroPagina - 1) * registrosPorPagina} ";
                System.Console.WriteLine(query);
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    await connection.OpenAsync();
                    List<Empleado> empleados = new List<Empleado>();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Empleado empleado = new Empleado();
                            Usuario usuario = new Usuario();
                            Rol rol = new Rol();
                            empleado.IdEmpleado = reader.GetInt32("idEmpleado");
                            empleado.IdPersona = reader.GetInt32("idPersona");
                            empleado.Dni = reader.GetInt32("dni");
                            empleado.Nombre = reader.GetString("nombre");
                            empleado.Apellido = reader.GetString("apellido");
                            empleado.Email = reader.GetString("email");
                            empleado.Telefono = reader.GetString("telefono");
                            empleado.Direccion = reader.GetString("direccion");
                            empleado.Estado = reader.GetBoolean("estado");
                            if (!reader.IsDBNull("idUsuario"))
                            {
                                empleado.IdUsuario = reader.GetInt32("idUsuario");
                                usuario.idUsuario = empleado.IdUsuario;
                                usuario.email = reader.GetString("email");
                                usuario.estado = reader.GetBoolean("estadoUsuario");
                                rol.IdRol = reader.GetInt32("idRol");
                                rol.Nombre = reader.GetString("nombre");
                            }
                            
                            System.Console.WriteLine(empleado.IdUsuario);

                            usuario.Rol = rol;
                            empleado.Usuario = usuario;
                            empleados.Add(empleado);
                        }
                    }
                    using (MySqlCommand countCommand = new MySqlCommand("SELECT FOUND_ROWS()", connection))
                    {
                        cantidadRegistros = Convert.ToInt32(await countCommand.ExecuteScalarAsync());
                    }
                    await connection.CloseAsync();
                    if (cantidadRegistros == 0) return ("No se encontraron registros de empleados", null, cantidadRegistros);
                    return (null, empleados, cantidadRegistros);
                }
            }
        }
        catch (Exception ex)
        {
            HelperFor.imprimirMensajeDeError(ex.Message, nameof(EmpleadoService), nameof(ObtenerTodos));
            return ("Error al obtener todos los empleados: Internal Server Error", null, null);
        }
    }
    public async Task<(string?, bool)> CreateEmpleado(int idPersona, int? idUsuario) //faltaria agregar usuario
    {
        try
        {
            if (await getEmpleadoByIdPersona(idPersona) is (null, Empleado empleado)) return ("El empleado ya se encuentra registrado", false);

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string query = @"INSERT INTO empleado (idPersona) VALUES (@idPersona);";
                //  string query2 = @"INSERT INTO empleado (idPersona, idUsuario) VALUES (@idPersona, @idUsuario);";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@idPersona", idPersona);
                    //command.Parameters.AddWithValue("@idUsuario", idUsuario);
                    await connection.OpenAsync();
                    int result = await command.ExecuteNonQueryAsync();
                    await connection.CloseAsync();
                    if (result == 0) return ("Error al crear empleado: Database Error", false);
                }
                return ("Empleado dado de alta correctamente", true);
            }
        }
        catch (Exception ex)
        {
            HelperFor.imprimirMensajeDeError(ex.Message, nameof(EmpleadoService), nameof(CreateEmpleado));
            return ("Error al crear empleado: Internal Server Error", false);
        }
    }
    public async Task<(string?, Empleado?)> getEmpleadoById(int idEmpleado)
    {
        try
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string query = @"SELECT empleado.idEmpleado, 
                                    persona.idPersona, 
                                    persona.dni, 
                                    persona.nombre, 
                                    persona.apellido, 
                                    persona.email, 
                                    persona.telefono, 
                                    persona.direccion, 
                                    persona.estado
                                    FROM empleado as empleado
                                    INNER JOIN persona as persona
                                    ON empleado.idPersona = persona.idPersona
                                    WHERE empleado.idEmpleado = @idEmpleado";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@idEmpleado", idEmpleado);
                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            Empleado empleado = new Empleado();
                            empleado.IdEmpleado = reader.GetInt32("idEmpleado");
                            empleado.IdPersona = reader.GetInt32("idPersona");
                            empleado.Dni = reader.GetInt32("dni");
                            empleado.Nombre = reader.GetString("nombre");
                            empleado.Apellido = reader.GetString("apellido");
                            empleado.Email = reader.GetString("email");
                            empleado.Telefono = reader.GetString("telefono");
                            empleado.Direccion = reader.GetString("direccion");
                            empleado.Estado = reader.GetBoolean("estado");
                            await connection.CloseAsync();
                            return (null, empleado);
                        }
                    }
                    await connection.CloseAsync();
                    return ("Empleado no encontrado", null);
                }
            }
        }
        catch (Exception ex)
        {
            HelperFor.imprimirMensajeDeError(ex.Message, nameof(EmpleadoService), nameof(getEmpleadoById));
            return ("Error al obtener empleado: Internal Server Error", null);

        }
    }

    public async Task<(string?, Empleado?)> getEmpleadoByIdPersona(int idPersona)
    {
        try
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string query = @"SELECT empleado.idEmpleado, 
                                    persona.idPersona, 
                                    persona.dni, 
                                    persona.nombre, 
                                    persona.apellido, 
                                    persona.email, 
                                    persona.telefono, 
                                    persona.direccion, 
                                    persona.estado
                                    FROM empleado as empleado
                                    INNER JOIN persona as persona
                                    ON empleado.idPersona = persona.idPersona
                                    WHERE persona.idPersona = @idPersona";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@idPersona", idPersona);
                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            Empleado empleado = new Empleado();
                            empleado.IdEmpleado = reader.GetInt32("idEmpleado");
                            empleado.IdPersona = reader.GetInt32("idPersona");
                            empleado.Dni = reader.GetInt32("dni");
                            empleado.Nombre = reader.GetString("nombre");
                            empleado.Apellido = reader.GetString("apellido");
                            empleado.Email = reader.GetString("email");
                            empleado.Telefono = reader.GetString("telefono");
                            empleado.Direccion = reader.GetString("direccion");
                            empleado.Estado = reader.GetBoolean("estado");
                            await connection.CloseAsync();
                            return (null, empleado);
                        }
                    }
                    await connection.CloseAsync();
                    return ("Empleado no encontrado", null);
                }
            }
        }
        catch (Exception ex)
        {
            HelperFor.imprimirMensajeDeError(ex.Message, nameof(EmpleadoService), nameof(getEmpleadoByIdPersona));
            return ("Error al obtener empleado: Internal Server Error", null);
        }
    }
    public async Task<(string?, bool)> AltaLogica(int idEmpleado)
    {
        try
        {
            if (await this.getEmpleadoById(idEmpleado) is (string error, null)) return (error, false);
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string query = @"UPDATE empleado SET estado = 1 WHERE idEmpleado = @idEmpleado;";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@idEmpleado", idEmpleado);
                    await connection.OpenAsync();
                    int result = await command.ExecuteNonQueryAsync();
                    await connection.CloseAsync();
                    if (result == 0) return ("Error al dar de alta empleado: Database Error", false);
                }
                return ("Empleado dado de alta correctamente", true);
            }
        }
        catch (Exception ex)
        {
            HelperFor.imprimirMensajeDeError(ex.Message, nameof(EmpleadoService), nameof(AltaLogica));
            return ("Error al dar de alta empleado: Internal Server Error", false);

        }
    }

    public async Task<(string?, bool)> BajaLogica(int idEmpleado)
    {
        try
        {
            if (await this.getEmpleadoById(idEmpleado) is (string error, null)) return (error, false);
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string query = @"UPDATE empleado SET estado = 0 WHERE idEmpleado = @idEmpleado;";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@idEmpleado", idEmpleado);
                    await connection.OpenAsync();
                    int result = await command.ExecuteNonQueryAsync();
                    await connection.CloseAsync();
                    if (result == 0) return ("Error al dar de alta empleado: Database Error", false);
                }
                return ("Empleado dado de alta correctamente", true);
            }
        }
        catch (Exception ex)
        {
            HelperFor.imprimirMensajeDeError(ex.Message, nameof(EmpleadoService), nameof(BajaLogica));
            return ("Error al dar de baja empleado: Internal Server Error", false);
        }
    }

    public async Task<(string?, Empleado?)> getEmpleadoByDni(int dni)
    {
        try
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string query = @"SELECT empleado.idEmpleado, 
                                    persona.idPersona, 
                                    persona.dni, 
                                    persona.nombre, 
                                    persona.apellido, 
                                    persona.email, 
                                    persona.telefono, 
                                    persona.direccion, 
                                    empleado.estado
                                    FROM empleado as empleado
                                    INNER JOIN persona as persona
                                    ON empleado.idPersona = persona.idPersona
                                    WHERE persona.dni = @dni";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@dni", dni);
                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            Empleado empleado = new Empleado();
                            empleado.IdEmpleado = reader.GetInt32("idEmpleado");
                            empleado.IdPersona = reader.GetInt32("idPersona");
                            empleado.Dni = reader.GetInt32("dni");
                            empleado.Nombre = reader.GetString("nombre");
                            empleado.Apellido = reader.GetString("apellido");
                            empleado.Email = reader.GetString("email");
                            empleado.Telefono = reader.GetString("telefono");
                            empleado.Direccion = reader.GetString("direccion");
                            empleado.Estado = reader.GetBoolean("estado");
                            await connection.CloseAsync();
                            return (null, empleado);
                        }
                    }
                    await connection.CloseAsync();
                    return ("Empleado no encontrado", null);
                }
            }
        }
        catch (Exception ex)
        {
            HelperFor.imprimirMensajeDeError(ex.Message, nameof(EmpleadoService), nameof(getEmpleadoByDni));
            return ("Error al obtener empleado: Internal Server Error", null);
        }
    }
    public async Task<(string?, Boolean)> asignarUsuario(int idUsuario, int idEmpleado)
    {
        try
        {
            if (await this.getEmpleadoById(idEmpleado) is (string error, null)) return (error, false);
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string query = @"UPDATE empleado SET idUsuario = @idUsuario WHERE idEmpleado = @idEmpleado;";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@idEmpleado", idEmpleado);
                    command.Parameters.AddWithValue("@idUsuario", idUsuario);
                    await connection.OpenAsync();
                    int result = await command.ExecuteNonQueryAsync();
                    await connection.CloseAsync();
                    if (result == 0) return ("Error al dar de alta empleado: Database Error", false);
                }
                return ("Empleado dado de alta correctamente", true);
            }
        }
        catch (Exception ex)
        {
            HelperFor.imprimirMensajeDeError(ex.Message, nameof(EmpleadoService), nameof(asignarUsuario));
            return ("Error al dar de alta empleado: Internal Server Error", false);
        }
    }
}