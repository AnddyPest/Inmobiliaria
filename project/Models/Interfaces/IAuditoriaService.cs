namespace project.Models.Interfaces
{
    public interface IAuditoriaService
    {
        Task<(string?, bool)> CreateAuditoria(Auditoria auditoria);
        Task<(string?, List<Auditoria>?)> GetAllAuditorias(int? nroPagina, int? registrosPorPagina, DateTime? fechaInicio, DateTime? fechaFin, string? accion, int? idUsuario);
        Task<(string?, Auditoria?)> GetAuditoriaById(int idAuditoria);
        Task<(string?, List<Auditoria>?)> GetAuditoriasByIdUsuario(int idUsuario);
        Task<(string?, List<Auditoria>?)> GetAuditoriasByContrato(int idContrato);
        Task<(string?, List<Auditoria>?)> GetAuditoriasByPago(int idPago);
    }
}