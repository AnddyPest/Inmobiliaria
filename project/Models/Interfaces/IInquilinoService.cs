namespace project.Models.Interfaces
{
    public interface IInquilinoService
    {
        Task<(string?, Inquilino?)> GetInquilinoById(int idInquilino);
        Task<(string?, List<Inquilino>)> GetAllInquilinos();
        (string?,bool?) AddInquilino(Inquilino inquilino);
        (string?, bool?) UpdateInquilino(Inquilino inquilino);
        (string?, bool?) LogicalDeleteInquilino(int idInquilino);
    }
}
