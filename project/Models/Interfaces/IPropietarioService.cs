namespace project.Models.Interfaces
{
    public interface IPropietarioService
    {
        Task<(string?, bool)> validarQueNoEsteAgregadoElPropietario(int idPersona);
        Task<(string?, Propietario?)> getPropietarioByIdPersona(int idPersona);
        Task<int> Alta(int idPersona);
        Task<int> Baja(int idPropietario);
        Task<int> Reestablecer(int idPropietario);
        Task<int> Editar(Propietario propietario);
        Task<(string?, Propietario?)> getPropietarioPorDni(int dni);
        Task<IList<Propietario>> ObtenerTodos();
        Task<int> ObtenerCantidad();
    }
}