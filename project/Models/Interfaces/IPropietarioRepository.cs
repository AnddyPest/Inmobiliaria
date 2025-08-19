namespace project.Models.Interfaces
{
    public interface IPropietarioRepository
    {
        int Alta(Propietario propietario);
        int Baja(int idPropietario);
        int Reestablecer(int idPropietario);
        int Editar(Propietario propietario);
        Propietario ObtenerPorDni(int dni);
        IList<Propietario> ObtenerTodos();
        IList<Propietario> ObtenerPorNombre(string nombre);
        int ObtenerCantidad();
    }
}