namespace project.Models
{
    public class MotivosAuditoria
    {
        public int IdMotivoAuditoria { get; set; }
        public string motivo {  get; set; }

        public MotivosAuditoria(string motivo)
        {
            this.motivo = motivo;
        }
    }
}
