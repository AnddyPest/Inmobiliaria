using System.ComponentModel.DataAnnotations;

namespace project.Models
{
    public class Ciudad(string nombre, string pais, int codigoPostal)
    {
        [Key]
        public int IdCiudad { get; set; }
        [Required(ErrorMessage = "El nombre de la ciudad es requerido")]
        [MinLength(2, ErrorMessage = "El nombre de la ciudad debe tener al menos 2 caracteres")]
        [MaxLength(100, ErrorMessage = "El nombre de la ciudad no puede tener más de 100 caracteres")]
        public string Nombre { get; set; } = nombre;
        [Required(ErrorMessage = "El país es requerido")]
        [MinLength(2, ErrorMessage = "El país debe tener al menos 2 caracteres")]
        [MaxLength(100, ErrorMessage = "El país no puede tener más de 100 caracteres")]
        public string Pais { get; set; } = pais;
        [Required(ErrorMessage = "El código postal es requerido")]
        [Range(1, 99999, ErrorMessage = "El código postal debe estar entre 1 y 99999")]
        public int CodigoPostal { get; set; } = codigoPostal;

    // Constructor vacío
    public Ciudad() : this(default!, default!, default) { }
    }
}
