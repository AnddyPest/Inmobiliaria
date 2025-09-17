using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace project.Models
{
    public class Inmueble(string uso,Tipo_Inmueble tipo, int superficie, int cantAmbientes, decimal coordenadas, decimal precio, string direccion, string ciudad, int idPropietario, bool estado)
    {
        [Key]
        public int IdInmueble { get; set; }

        [Required(ErrorMessage = "El uso es requerido")]
        public string Uso { get; set; } = uso;

        [Required(ErrorMessage = "El tipo es requerido")]
        [ForeignKey("Tipo")]
        public int idTipo { get; set; }
        public Tipo_Inmueble? Tipo { get; set; } = tipo;

        [Required(ErrorMessage = "La superficie es requerida")]
        [Range(1, int.MaxValue, ErrorMessage = "La superficie debe ser un valor positivo")]
        public int Superficie { get; set; } = superficie;

        [Required(ErrorMessage = "La cantidad de ambientes es requerida")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad de ambientes debe ser un valor positivo")]
        public int CantidadAmbientes { get; set; } = cantAmbientes;

        [Required(ErrorMessage = "Las coordenadas son requeridas")]
        public decimal Coordenadas { get; set; } = coordenadas;

        [Required(ErrorMessage = "El precio es requerido")]
        [Range(0, double.MaxValue, ErrorMessage = "El precio debe ser un valor positivo")]
        public decimal Precio { get; set; } = precio;

        [Required(ErrorMessage = "La dirección es requerida")]
        public string Direccion { get; set; } = direccion;
        [Required(ErrorMessage = "La ciudad es requerida")]
        public string Ciudad { get; set; } = ciudad;

        [ForeignKey("Propietario")]
        public int IdPropietario { get; set; } = idPropietario;
        public Propietario? Propietario { get; set; }
        public Contrato? contrato { get; set; }
        public List<string>? ImagenesUrl { get; set; }

        [Required(ErrorMessage = "La disponibilidad es requerida")]
        public bool Disponible { get; set; } = true;

        public bool Estado { get; set; } = estado;

        // Constructor vacío
        public Inmueble() : this(default!, default!, default, default, default, default, default!, default!, default, default) { }
        public override string ToString()
        {
            return @$"idInmueble: {this.IdInmueble}
                      idPropietario: {this.IdPropietario}
                      Uso: {this.Uso}
                      idTipo: {this.idTipo}
                      Superficie: {this.Superficie}
                      Coordenadas: {this.Coordenadas}
                      Direccion: {this.Direccion}
                      Ciudad: {this.Ciudad}
                      Disponible: {this.Disponible}
                      Estado: {this.Estado}";
        }
    }
}
