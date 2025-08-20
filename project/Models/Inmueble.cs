using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace project.Models
{
    public class Inmueble(string uso, string tipo, int superficie, int cantAmbientes, decimal coordenadas, decimal precio, string direccion, int idCiudad, int idPropietario, bool estado)
    {
        [Key]
        public int IdInmueble { get; set; }

        [Required(ErrorMessage = "El uso es requerido")]
        public string Uso { get; set; } = uso;

        [Required(ErrorMessage = "El tipo es requerido")]
        public string Tipo { get; set; } = tipo;

        [Required(ErrorMessage = "La superficie es requerida")]
        [Range(1, int.MaxValue, ErrorMessage = "La superficie debe ser un valor positivo")]
        public int Superficie { get; set; } = superficie;

        [Required(ErrorMessage = "La cantidad de ambientes es requerida")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad de ambientes debe ser un valor positivo")]
        public int CantAmbientes { get; set; } = cantAmbientes;

        [Required(ErrorMessage = "Las coordenadas son requeridas")]
        public decimal Coordenadas { get; set; } = coordenadas;

        [Required(ErrorMessage = "El precio es requerido")]
        [Range(0, double.MaxValue, ErrorMessage = "El precio debe ser un valor positivo")]
        public decimal Precio { get; set; } = precio;

        [Required(ErrorMessage = "La dirección es requerida")]
        public string Direccion { get; set; } = direccion;

        [ForeignKey("Ciudad")]
        public int IdCiudad { get; set; } = idCiudad;

        [ForeignKey("Propietario")]
        public int IdPropietario { get; set; } = idPropietario;

        [Required(ErrorMessage = "La disponibilidad es requerida")]
        public bool Disponible { get; set; } = true;

        public bool estado { get; set; } = estado;

        // Constructor vacío
        public Inmueble() : this(default!, default!, default, default, default, default, default!, default, default, default) { }
    }
}
