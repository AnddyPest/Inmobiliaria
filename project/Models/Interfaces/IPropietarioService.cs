namespace project.Models.Interfaces
{
    public interface IPropietarioService
    {
        Task<(string?, bool)> validarQueNoEsteAgregadoElPropietario(int idPersona);
        Task<(string?, Propietario?)> getPropietarioByIdPersona(int idPersona);
        Task<(string?, Propietario?)> getPropietarioById(int idPropietario);
        Task<int> Alta(int idPersona);
        Task<int> BajaLogica(int idPropietario);
        Task<int> AltaLogica(int idPropietario);
        Task<(string?, Propietario?)> getPropietarioPorDni(int dni);
        Task<IList<Propietario>> ObtenerTodos();
        Task<int> ObtenerCantidad();
    }
}