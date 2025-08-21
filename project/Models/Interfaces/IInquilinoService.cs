namespace project.Models.Interfaces
{
    public interface IInquilinoService
    {
        Task<(string?, bool)> validarQueNoEsteAgregadoElInquilino(int idPersona);
        Task<(string?,Inquilino?)> getInquilinoByIdPersona(int idPersona);
        Task<(string?, Inquilino?)> GetInquilinoById(int idInquilino);
        Task<(string?, List<Inquilino>)> GetAllInquilinos();
        Task<(string?, Inquilino?)> AddInquilino(Persona persona);
        Task<(string?, bool?)> LogicalDeleteInquilino(int idInquilino);
        Task<(string?, bool?)> AltaLogicaInquilino(int idInquilino);
    }
}
