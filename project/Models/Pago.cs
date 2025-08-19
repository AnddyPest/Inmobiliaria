namespace project.Models
{
    public class Pago(decimal importe, bool abonado)
    {
        public int IdPago { get; set; }
        public static int NumeroPago { get; set; } = 0;
        public DateTime Fecha { get; set; } = DateTime.Now;
        public decimal Importe { get; set; } = importe;
        public bool Abonado { get; set; } = abonado;
        public bool Activo { get; set; } = true;

        public Pago() : this(default, false) { }
    }
}
