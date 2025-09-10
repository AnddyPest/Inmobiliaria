namespace project.Models.ViewModels
{
    public class PagoViewModel
    {
        public int IdPago { get; set; }
        public int IdContrato { get; set; }
        public decimal Importe { get; set; }
        public DateTime FechaPago { get; set; }
        public DateTime FechaConfeccion { get; set; }
        public string? Detalle { get; set; }
        public bool Estado { get; set; }
        // Puedes agregar más propiedades según lo que necesite la vista
    }
}