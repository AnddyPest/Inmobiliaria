using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace project.Models
{
    public class Auditoria(int idUsuario, int? idContrato, int? idPago, int idMotivoAuditoria)
    {

        [Key]
        public int IdAuditoria { get; set; }
        [ForeignKey("Usuario")]
        public int IdUsuario { get; set; } = idUsuario;
        [ForeignKey("Contrato")]
        public int? IdContrato { get; set; } = idContrato;
        public int? IdPago { get; set; } = idPago;
        public DateTime Fecha { get; set; } = DateTime.Now;
    public int IdMotivoAuditoria { get; set; } = idMotivoAuditoria;

    // Constructor vacío
    public Auditoria() : this(default, null, null, default) { }
    }
}
