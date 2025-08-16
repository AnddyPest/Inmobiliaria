namespace project.Models
{
    public class Pago
    {
        public int IdPago { get; set; }
        public static int NumeroPago { get; set; } = 0;
        public DateTime Fecha { get; set; }
        public decimal Importe { get; set; }
        public bool Abonado { get; set; } = false;
        public bool Activo { get; set; } = true;

        public Pago(decimal importe, bool Abonado)
        {
            NumeroPago++;
            Fecha = DateTime.Now;
        }
        
    }
}
