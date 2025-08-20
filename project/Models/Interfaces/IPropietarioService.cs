namespace project.Models.Interfaces
{
    public interface IPropietarioService
    {
        Task<int> Alta(Propietario propietario);
        Task<int> Baja(int idPropietario);
        Task<int> Reestablecer(int idPropietario);
        Task<int> Editar(Propietario propietario);
        Task<Propietario> ObtenerPorDni(int dni);
        Task<IList<Propietario>> ObtenerTodos();
        Task<IList<Propietario>> ObtenerPorNombre(string nombre);
        Task<int> ObtenerCantidad();
    }
}