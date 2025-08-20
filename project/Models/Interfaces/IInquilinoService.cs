namespace project.Models.Interfaces
{
    public interface IInquilinoService
    {
        (string?, Inquilino) GetInquilinoById(int idInquilino);
        Task<(string?, List<Inquilino>)> GetAllInquilinos();
        (string?,bool?) AddInquilino(Inquilino inquilino);
        (string?, bool?) UpdateInquilino(Inquilino inquilino);
        (string?, bool?) LogicalDeleteInquilino(int idInquilino);
    }
}
