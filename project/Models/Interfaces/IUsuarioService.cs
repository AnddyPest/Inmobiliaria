

using project.Models;

public interface IUsuarioService
{
    Task<(string?, Usuario?)> GetUsuarioById(int idUsuario);
    Task<(string?, bool)> CreateUsuario(Usuario usuario);
    Task<(string?, bool)> UpdateUsuario(Usuario usuario);
    Task<(string?, bool)> resetearContraseña(int idUsuario);
    
}