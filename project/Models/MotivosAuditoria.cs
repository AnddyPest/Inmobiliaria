namespace project.Models
{
    public class MotivosAuditoria(string motivo)
    {
        public int IdMotivoAuditoria { get; set; }
        public string Motivo { get; set; } = motivo;

        // Constructor vacío
        public MotivosAuditoria() : this(default!) { }
    }
}
