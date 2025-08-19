using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace project.Models
{
    public class Contrato(int idInquilino, int idInmueble, decimal monto, DateTime fechaInicio, DateTime fechaFin, int idPropietario)
    {
        [Key]
        public int IdContrato { get; set; }

        [ForeignKey("Inquilino")]
        public int IdInquilino { get; set; } = idInquilino;

        [ForeignKey("Inmueble")]
        public int IdInmueble { get; set; } = idInmueble;

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0, double.MaxValue, ErrorMessage = "El monto debe ser un valor positivo")]
        public decimal Monto { get; set; } = monto;

        [Required(ErrorMessage = "La fecha de inicio es requerida")]
        public DateTime FechaI { get; set; } = fechaInicio;

        [Required(ErrorMessage = "La fecha de fin es requerida")]
        public DateTime FechaF { get; set; } = fechaFin;

    public List<Pago> Pagos { get; set; } = [];

    [ForeignKey("Propietario")]
    public int IdPropietario { get; set; } = idPropietario;

    
    private readonly bool validarFechas = ValidarFechas(fechaInicio, fechaFin);

    // Constructor vacío
    public Contrato() : this(default, default, default, default, default, default) { }

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

        private static bool ValidarFechas(DateTime inicio, DateTime fin)
        {
            
            if (inicio == default || fin == default)
            {
                return true;
            }

            if (inicio > fin)
            {
                throw new ArgumentException("La fecha de inicio no puede ser posterior a la fecha de fin.");
            }
            return true;
        }
    }
}
