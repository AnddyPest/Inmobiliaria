namespace project.Models.Interfaces
{
    public interface IContratoService
    {
        Task<(string?, bool, int?)> CreateContrato(Contrato contrato);
        Task<(string?, bool)> UpdateContrato(Contrato contrato);
        Task<(string?, List<Contrato>?)> GetAllContratos(int? nroPagina, int? registrosPorPagina, string? disponibilidad, int? fechaCompare, string? inmueble);

        Task<(string?, Contrato?)> GetContratoById(int idContrato);
        Task<(string?, bool)> DarAltaContrato(int idContrato);
        Task<(string?, bool)> DarBajaContrato(int idContrato);
        Task<(string?, bool)> ComprobarContratoActivoPorIdInmueble(int idInmueble);
        Task<(string?, List<Contrato>?)> GetContratosByIdInquilino(int idCliente);
        Task<(string?, List<Contrato>?)> GetContratoByIdInmueble(int idInmueble);
        Task<(string?, bool)> ValidarNoSuperposicionFechas(int idInmueble, DateTime fechaInicio, DateTime fechaFin);
        Task<(string?, List<Contrato>?)> GetContratosByIdPropietario(int idPropietario);
        Task<(string?, List<Contrato>?)> GetContratosVigentes();
        Task<(string?, List<Contrato>?)> GetContratosAPI();

        Task<(string?, bool)> TerminarContrato(int idContrato);
        Task<(string?, bool)> RenovarContrato(int idContrato, DateTime nuevaFechaInicio, DateTime nuevaFechaFin, decimal nuevoMonto);
        Task<(string?, int?)> CalcularMesesDeMulta(int idContrato);
    }
}
