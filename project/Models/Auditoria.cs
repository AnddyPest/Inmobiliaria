namespace project.Models
{
    public class Auditoria
    {

        public int IdAuditoria {  get; set; }
        public int IdUsuario { get; set; }
        public int? IdContrato { get; set; }
        public int? IdPago { get; set; }
        public DateTime fecha { get; set; } = DateTime.Now;
        public int IdMotivoAuditoria { get; set; }

        public Auditoria(int idUsuario, int? idContrato, int? idPago, int IdMotivoAuditoria)
        {
            this.IdUsuario = idUsuario;
            this.IdContrato = idContrato;
            this.IdPago = idPago;
            this.IdMotivoAuditoria = IdMotivoAuditoria;
        }
    }
}
