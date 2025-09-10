using System.ComponentModel.DataAnnotations.Schema;

namespace project.Models
{
    public class Pago(int numero, string detalle, bool abonado, bool alquiler, decimal importe, DateOnly fechaConfeccion, int idContrato)
    {
        public int IdPago { get; set; }
        public int Numero { get; set; } = numero;
        public string Detalle { get; set; } = detalle;
        public decimal Importe { get; set; } = importe;

        public bool Abonado { get; set; } = abonado;
        public bool Alquiler { get; set; } = alquiler;
        public bool Estado { get; set; } = true;
        public DateOnly FechaConfeccion { get; set; } = fechaConfeccion;
        public DateOnly? FechaPago { get; set; } = null;
        [ForeignKey("Contrato")]
        public int IdContrato { get; set; } = idContrato;
        public Pago() : this(0, "", false, true, 0, DateOnly.MinValue, 0) { }
    }
}
