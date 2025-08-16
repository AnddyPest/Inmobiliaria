namespace project.Models
{
    public class Contrato
    {
        public int IdContrato { get; set; }
        public Inquilino Inquilino { get; set; }
        public Inmueble Inmueble { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaI { get; set; }
        public DateTime FechaF { get; set; }
        public List<Pago> Pagos { get; set; } = new List<Pago>();

        public Contrato(Inquilino inquilino, Inmueble inmueble, decimal monto, DateTime fechaInicio, DateTime fechaFin)
        {
            this.Inquilino = inquilino;
            this.Inmueble = inmueble;
            this.Monto = monto;
            this.FechaI = fechaInicio;
            this.FechaF = fechaFin;
            if (fechaInicio > fechaFin)
            {
                throw new ArgumentException("La fecha de inicio no puede ser posterior a la fecha de fin.");
            }
        }
        public void AgregarPago(Pago pago)
        {
            if (pago == null)
            {
                throw new ArgumentNullException(nameof(pago), "El pago no puede ser nulo.");
            }
            if (pago.Importe <= 0)
            {
                throw new ArgumentException("El importe del pago debe ser mayor que cero.", nameof(pago));
            }
            Pagos.Add(pago);
        }
    }
}
