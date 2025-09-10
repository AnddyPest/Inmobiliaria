namespace project.Models.Interfaces
{
    public interface IContratoService
    {
        Task<(string?, bool)> CreateContrato(Contrato contrato);
        Task<(string?, bool)> UpdateContrato(Contrato contrato);
        Task<(string?, List<Contrato>?)> GetAllContratos(int? nroPagina, int? registrosPorPagina, string? disponibilidad);

        Task<(string?,Contrato?)> GetContratoById(int idContrato);
        Task<(string?,bool)> DarAltaContrato(int idContrato);
        Task<(string?, bool)> DarBajaContrato(int idContrato);
        Task<(string?,bool)> ComprobarContratoActivoPorIdInmueble(int idInmueble);
        Task<(string?, List<Contrato>?)> GetContratosByIdInquilino(int idCliente);
        Task<(string?, List<Contrato>?)> GetContratoByIdInmueble(int idInmueble);
        Task<(string?,List<Contrato>?)> GetContratosByIdPropietario(int idPropietario);
        Task<(string?, List<Contrato>?)> GetContratosVigentes();
    }
}
