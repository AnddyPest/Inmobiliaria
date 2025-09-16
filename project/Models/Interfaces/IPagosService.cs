namespace project.Models.Interfaces
{
    public interface IPagosService
    {
        Task<(string?, bool)> CreatePago(Pago pago);
        Task<(string?, bool)> UpdatePago(Pago pago);
        Task<(string?, bool)> AsentarPago(Pago pago);
        Task<(string?,bool)> darDeBajaLogicaPago(int idPago);
        Task<(string?, bool)> darAltaLogicaPago(int idPago);
        Task<(string?, List<Pago>?)> GetAllPagos(int? nroPagina, int? registrosPorPagina);
        Task<(string?, List<Pago>?)> GetPagosByIdContrato(int? nroPagina, int? registrosPorPagina, int idContrato);
        Task<(string?, Pago?)> GetPagoById(int idPago);
        Task<(string?, bool)> AnularPago(Pago pago);
        Task<(string?, bool)> ReintegrarPago(int idPago);
        Task<(string?, List<Pago>?)> GetPagosByFecha(DateTime fecha);
        Task<(string?, List<Pago>?)> GetPagosPendientes();
        Task<(string?, List<Pago>?)> GetPagosRealizados();
        Task<(string?, List<Pago>?)> GetPagosAnulados();
        Task<bool> ExistePagoAlquiler(int idContrato, DateOnly fechaConfeccion);
        Task<(string?, bool)> CrearMulta(int idContrato, decimal importe, string detalle);
    }
}