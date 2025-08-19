namespace project.Models.Interfaces
{
    public interface IPropietarioRepository
    {
        int Alta(Propietario propietario);
        
        Propietario ObtenerPorDni(int dni);
        IList<Propietario> ObtenerTodos();
        IList<Propietario> ObtenerPorNombre(string nombre);
        int ObtenerCantidad();
    }
}