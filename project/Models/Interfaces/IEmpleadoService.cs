

using project.Models;

public interface IEmpleadoService
{
    Task<(string?, List<Empleado>?,int?)> ObtenerTodos(int? nroPagina, int? registrosPorPagina, int? estado, int? dni);
    Task<(string?, bool)> CreateEmpleado(int idPersona,int? idUsuario);
    Task<(string?, Empleado?)> getEmpleadoById(int idEmpleado);
    Task<(string?, Empleado?)> getEmpleadoByIdPersona(int idPersona);
    Task<(string?, Boolean)> BajaLogica(int idEmpleado);
    Task<(string?, Boolean)> AltaLogica(int idEmpleado);


}