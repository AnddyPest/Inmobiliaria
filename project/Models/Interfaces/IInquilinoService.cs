namespace project.Models.Interfaces
{
    public interface IInquilinoService
    {
        Task<(string?, Inquilino?)> GetInquilinoById(int idInquilino);
        Task<(string?, List<Inquilino>)> GetAllInquilinos();
        Task<(string?, Inquilino?)> AddInquilino(Inquilino inquilino);
        Task<(string?, bool?)> LogicalDeleteInquilino(int idInquilino);
    }
}
