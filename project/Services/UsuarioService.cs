

using System.Data;
using System.Data.Common;
using MySql.Data.MySqlClient;
using project.Helpers;
using project.Models;

public class UsuarioService : IUsuarioService
{
    private string _connectionString;
    public UsuarioService(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("Connection")!;
    }
    public async Task<(string?, bool, int)> CreateUsuario(Usuario usuario)
    {
        if (await this.GetUsuarioByEmail(usuario.email) is (null, Usuario)) return ("Ya hay un usuario registrado con ese email", false, 0);
        try
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                string query = @"   INSERT INTO usuario (email,contraseña,idRol,estado) 
                                    VALUES (@email,@contrasena,@idRol,@estado);
                                    SELECT LAST_INSERT_ID();";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@email", usuario.email);
                    command.Parameters.AddWithValue("@contrasena", usuario.contrasena);
                    command.Parameters.AddWithValue("@idRol", usuario.IdRol);
                    command.Parameters.AddWithValue("@estado", usuario.estado);
                    await connection.OpenAsync();
                    var result = await command.ExecuteScalarAsync();
                    await connection.CloseAsync();
                    if (result != null && int.TryParse(result.ToString(), out int idUsuario))
                    {
                        return (null, true, idUsuario);
                    }
                    else
                    {
                        return ("Error al crear usuario: Database Error", false, 0);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            HelperFor.imprimirMensajeDeError(ex.Message, nameof(UsuarioService), nameof(CreateUsuario));
            return ("Error al crear usuario: Internal Server Error", false, 0);
        }
    }
    public async Task<(string?, bool)> UpdateUsuario(Usuario usuario)
    {
        if (await this.GetUsuarioById(usuario.idUsuario) is (string error, null)) return (error, false);
        if (await this.ValidarEmailDisponible(usuario) is (string errorValidacion, false)) return (errorValidacion, false);

        try
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                string query = @"UPDATE usuario SET email = @email, contraseña = @contrasena, idRol = @idRol, estado = @estado WHERE idUsuario = @idUsuario;";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@email", usuario.email);
                    command.Parameters.AddWithValue("@contrasena", usuario.contrasena);
                    command.Parameters.AddWithValue("@idRol", usuario.IdRol);
                    command.Parameters.AddWithValue("@estado", usuario.estado);
                    command.Parameters.AddWithValue("@idUsuario", usuario.idUsuario);
                    await connection.OpenAsync();
                    int result = await command.ExecuteNonQueryAsync();
                    await connection.CloseAsync();
                    if (result == 0) return ("Error al actualizar usuario: Database Error", false);
                }
                return ("Usuario actualizado correctamente", true);
            }
        }
        catch (Exception ex)
        {
            HelperFor.imprimirMensajeDeError(ex.Message, nameof(UsuarioService), nameof(UpdateUsuario));
            return ("Error al actualizar usuario: Internal Server Error", false);
        }
    }

    public async Task<(string?, Usuario?)> GetUsuarioById(int idUsuario)
    {
        try
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                string query = @"SELECT 
                                    user.*,
                                    rol.*,
                                    empleado.*,
                                    persona.nombre AS persona_nombre,
                                    persona.apellido AS persona_apellido
                                FROM usuario AS user
                                INNER JOIN empleado ON user.idUsuario = empleado.idUsuario
                                INNER JOIN rol ON user.idRol = rol.idRol
                                INNER JOIN persona ON empleado.idPersona = persona.idPersona
                                WHERE user.idUsuario = @idUsuario;";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@idUsuario", idUsuario);
                    await connection.OpenAsync();
                    using (DbDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            Usuario usuario = new Usuario();
                            Empleado empleado = new Empleado();
                            Rol rol = new Rol();
                            usuario.idUsuario = reader.GetInt32("idUsuario");
                            usuario.email = reader.GetString("email");
                            usuario.contrasena = reader.GetString("contraseña");
                            usuario.IdRol = reader.GetInt32("idRol");
                            usuario.AvatarUrl = reader.IsDBNull(reader.GetOrdinal("AvatarUrl")) ? null : reader.GetString("AvatarUrl");
                            usuario.estado = reader.GetBoolean("estado");

                            empleado.nombre = reader.GetString("persona_nombre");
                            empleado.apellido = reader.GetString("persona_apellido");
                            usuario.Empleado = empleado;

                            rol.IdRol = reader.GetInt32("idRol");
                            rol.Nombre = reader.GetString("nombre");
                            usuario.Rol = rol;

                            await connection.CloseAsync();
                            return (null, usuario);
                        }
                    }

                    await connection.CloseAsync();
                    return ("El usuario no se encuentra registrado", null);
                }
            }
        }
        catch (Exception ex)
        {
            HelperFor.imprimirMensajeDeError(ex.Message, nameof(UsuarioService), nameof(GetUsuarioById));
            return ("Error al obtener el usuario: Internal Server Error", null);
        }
    }
    public async Task<(string?, Usuario?)> GetUsuarioByEmail(string email)
    {
        try
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                string query = @"SELECT 
                                user.*,
                                rol.*,
                                persona.* 
                                FROM usuario as user 
                                INNER JOIN rol ON user.idRol = rol.idRol
                                inner join empleado on user.idUsuario = empleado.idUsuario
                                INNER JOIN persona ON empleado.idPersona = persona.idPersona
                                WHERE user.email = @email;";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@email", email);
                    await connection.OpenAsync();
                    using (DbDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            Usuario usuario = new Usuario();
                            Rol rol = new Rol();
                            Empleado empleado = new Empleado();
                            usuario.idUsuario = reader.GetInt32("idUsuario");
                            usuario.email = reader.GetString("email");
                            usuario.contrasena = reader.GetString("contraseña");
                            usuario.IdRol = reader.GetInt32("idRol");
                            usuario.estado = reader.GetBoolean("estado");
                            usuario.AvatarUrl = reader.IsDBNull(reader.GetOrdinal("AvatarUrl")) ? null : reader.GetString("AvatarUrl");

                            empleado.Nombre = reader.GetString("nombre");
                            empleado.Apellido = reader.GetString("apellido");
                            empleado.Dni = reader.GetInt32("dni");

                            rol.IdRol = reader.GetInt32("idRol");
                            rol.Nombre = reader.GetString("nombre");

                            usuario.Empleado = empleado;
                            usuario.Rol = rol;
                            await connection.CloseAsync();
                            return (null, usuario);
                        }
                    }
                    await connection.CloseAsync();
                    return ("El usuario no se encuentra registrado", null);
                }
            }
        }
        catch (Exception ex)
        {
            HelperFor.imprimirMensajeDeError(ex.Message, nameof(UsuarioService), nameof(GetUsuarioByEmail));
            return ("Error al obtener el usuario: Internal Server Error", null);
        }
    }
    public async Task<(string?, bool)> ValidarEmailDisponible(Usuario usuario)
    {
        if (await this.GetUsuarioByEmail(usuario.email) is (null, Usuario user))
        {
            if (user.idUsuario != usuario.idUsuario) return ("El email ya se encuentra registrado", false);
            return (null, true);
        }
        return (null, true);
    }
    public async Task<(string?, bool)> resetearContraseña(int idUsuario)
    {
        string hashContrasena = AuthHelper.HashContraseña("123456");
        try
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                string query = @"UPDATE usuario SET contraseña = @contrasena WHERE idUsuario = @idUsuario;";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@idUsuario", idUsuario);
                    command.Parameters.AddWithValue("@contrasena", hashContrasena);
                    await connection.OpenAsync();
                    int result = await command.ExecuteNonQueryAsync();
                    await connection.CloseAsync();
                    if (result == 0) return ("Error al resetear la contraseña: Database Error", false);
                }
                return ("Contraseña reseteada correctamente", true);
            }
        }
        catch (Exception ex)
        {
            HelperFor.imprimirMensajeDeError(ex.Message, nameof(UsuarioService), nameof(resetearContraseña));
            return ("Error al resetear la contraseña: Internal Server Error", false);
        }
    }



    public async Task<(string?, bool)> validarCredenciales(string username, string password)
    {
        try
        {
            if (await this.GetUsuarioByEmail(username) is (string error, null)) return (error, false);
            if (await this.GetUsuarioByEmail(username) is (null, Usuario user))
            {
                bool validacion = AuthHelper.VerificarContrasena(password, user.contrasena);
                if (validacion) return (null, true);
            }
            return ("Contraseña incorrecta", false);

        }
        catch (Exception ex)
        {
            HelperFor.imprimirMensajeDeError(ex.Message, nameof(UsuarioService), nameof(validarCredenciales));
            return ("Error al validar credenciales: Internal Server Error", false);
        }
    }
    public async Task<(string?, bool)> AltaLogica(int idUsuario)
    {
        try
        {
            if (await this.GetUsuarioById(idUsuario) is (string error, null)) return (error, false);
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                string query = @"UPDATE usuario SET estado = 1 WHERE idUsuario = @idUsuario;";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@idUsuario", idUsuario);
                    await connection.OpenAsync();
                    int result = await command.ExecuteNonQueryAsync();
                    await connection.CloseAsync();
                    if (result == 0) return ("Error al dar de alta usuario: Database Error", false);
                }
                return ("Usuario dado de alta correctamente", true);
            }
        }
        catch (Exception ex)
        {
            HelperFor.imprimirMensajeDeError(ex.Message, nameof(UsuarioService), nameof(AltaLogica));
            return ("Error al dar de alta usuario: Internal Server Error", false);
        }
    }

    public async Task<(string?, bool)> BajaLogica(int idUsuario)
    {
        try
        {
            if (await this.GetUsuarioById(idUsuario) is (string error, null)) return (error, false);
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                string query = @"UPDATE usuario SET estado = 0 WHERE idUsuario = @idUsuario;";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@idUsuario", idUsuario);
                    await connection.OpenAsync();
                    int result = await command.ExecuteNonQueryAsync();
                    await connection.CloseAsync();
                    if (result == 0) return ("Error al dar de baja usuario: Database Error", false);
                }
                return ("Usuario dado de baja correctamente", true);
            }
        }
        catch (Exception ex)
        {
            HelperFor.imprimirMensajeDeError(ex.Message, nameof(UsuarioService), nameof(BajaLogica));
            return ("Error al dar de baja usuario: Internal Server Error", false);
        }
    }
    public async Task<(string?, bool)> SetearAvatar(int idUsuario, string avatarUrl)
    {
        try
        {
            if (await this.GetUsuarioById(idUsuario) is (string error, null)) return (error, false);
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                string query = @"UPDATE usuario SET AvatarUrl = @avatarUrl WHERE idUsuario = @idUsuario;";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@idUsuario", idUsuario);
                    command.Parameters.AddWithValue("@avatarUrl", avatarUrl);
                    await connection.OpenAsync();
                    int result = await command.ExecuteNonQueryAsync();
                    await connection.CloseAsync();
                    if (result == 0) return ("Error al setear avatar: Database Error", false);
                }
                return ("Avatar seteado correctamente", true);
            }
        }
        catch (Exception ex)
        {
            HelperFor.imprimirMensajeDeError(ex.Message, nameof(UsuarioService), nameof(SetearAvatar));
            return ("Error al setear avatar: Internal Server Error", false);
        }
    }
    public async Task<(string?, string?)> GetAvatarUrl(int idUsuario)
    {
        try
        {
            if (await this.GetUsuarioById(idUsuario) is (string error, null)) return (error, null);
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                string query = @"SELECT AvatarUrl FROM usuario WHERE idUsuario = @idUsuario;";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@idUsuario", idUsuario);
                    await connection.OpenAsync();
                    var result = await command.ExecuteScalarAsync();
                    await connection.CloseAsync();
                    if (result != null)
                    {
                        return (null, result.ToString());
                    }
                    else
                    {
                        return ("El usuario no tiene un avatar seteado", null);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            HelperFor.imprimirMensajeDeError(ex.Message, nameof(UsuarioService), nameof(GetAvatarUrl));
            return ("Error al obtener el avatar: Internal Server Error", null);
        }
    }

    
}