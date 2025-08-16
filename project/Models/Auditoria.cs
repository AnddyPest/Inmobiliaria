namespace project.Models
{
    public class Auditoria
    {

        public int IdAuditoria {  get; set; }
        public int IdUsuario { get; set; }
        public int? IdContrato { get; set; }
        public int? IdPago { get; set; }
        public DateTime fecha { get; set; } = DateTime.Now;
        public string motivo { get; set; }

        public Auditoria(int idUsuario, int? idContrato, int? idPago, string motivo)
        {
            this.IdUsuario = idUsuario;
            this.IdContrato = idContrato;
            this.IdPago = idPago;
            this.motivo = motivo;
        }
    }
}
