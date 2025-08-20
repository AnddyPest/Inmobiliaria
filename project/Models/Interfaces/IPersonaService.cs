namespace project.Models.Interfaces
{
    public interface IPersonaService
    {
        Task<int> Alta(Persona persona);
        Task<int> Baja(int idPersona);
        Task<int> Editar(Persona? persona);
        Task<int> ObtenerCantidad();
        Task<Persona?> ObtenerPorDni(int dni);
        Task<IList<Persona?>> ObtenerPorNombre(string nombre);
        Task<IList<Persona?>> ObtenerTodos();
        Task<int> Reestablecer(int idPersona);
        Task<(string?, Persona?)> GetPersonaById(int idPersona, bool estado);
    }
}