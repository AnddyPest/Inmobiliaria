using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace project.Models
{
    public class Auditoria(int idUsuario, int? idContrato, int? idPago, string MotivoAuditoria)
    {

        [Key]
        public int IdAuditoria { get; set; }
        [ForeignKey("Usuario")]
        public int IdUsuario { get; set; } = idUsuario;
        [ForeignKey("Contrato")]
        public int? IdContrato { get; set; } = idContrato;
        public int? IdPago { get; set; } = idPago;
        public DateTime Fecha { get; set; } = DateTime.Now;
        public string MotivoAuditoria { get; set; } = MotivoAuditoria;

    // Constructor vacío
    public Auditoria() : this(default, null, null, default!) { }
    }
}
