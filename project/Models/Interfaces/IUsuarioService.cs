

using project.Models;

public interface IUsuarioService
{
    Task<(string?, Usuario?)> GetUsuarioById(int idUsuario);
    Task<(string?, Usuario?)> GetUsuarioByEmail(string email);
    Task<(string?, bool)> ValidarEmailDisponible(Usuario usuario);
    Task<(string?, bool, int)> CreateUsuario(Usuario usuario);
    Task<(string?, bool)> UpdateUsuario(Usuario usuario);
    Task<(string?, bool)> resetearContraseña(int idUsuario);
    Task<(string?, bool)> BajaLogica(int idUsuario);
    Task<(string?, bool)> BajaLogicaByIdEmpleado(int idEmpleado);
    Task<(string?, bool)> AltaLogica(int idUsuario);
    Task<(string?, bool)> AltaLogicaByIdEmpleado(int idEmpleado);
    Task<(string?, bool)> validarCredenciales(string username, string password);
    Task<(string?, bool)> CambiarRol(int idUsuario, int idRol);
}