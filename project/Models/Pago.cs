namespace project.Models
{
    public class Pago(int numero, string detalle, bool abonado, decimal importe, Contrato? contrato)
    {
        public int IdPago { get; set; }
        public int Numero { get; set; } = numero;
        public string Detalle { get; set; } = detalle;
        public decimal Importe { get; set; } = importe;

        public bool Abonado { get; set; } = abonado;
        public bool Activo { get; set; } = true;
        public bool Estado { get; set; } = true;
        public DateTime Fecha { get; set; } = DateTime.Now;

        public Contrato? Contrato { get; set; } = contrato;
        public Pago() : this(0,"",false,0,null) { }
    }
}
