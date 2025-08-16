using System.ComponentModel.DataAnnotations;

namespace project.Models
{
    public abstract class Persona
    {
        [Key]
        protected int IdPersona { get; set; }
        [Required(ErrorMessage = "El nombre es requerido")]
        [MinLength(2, ErrorMessage = "El nombre debe tener al menos 2 caracteres")]
        [MaxLength(255, ErrorMessage = "El nombre no puede tener más de 255 caracteres")]
        protected string Nombre { get; set; }
        [Required(ErrorMessage = "El apellido es requerido")]
        [MinLength(2, ErrorMessage = "El apellido debe tener al menos 2 caracteres")]
        [MaxLength(100, ErrorMessage = "El apellido no puede tener más de 100 caracteres")]
        protected string Apellido { get; set; }
        [Required(ErrorMessage = "El DNI es requerido")]
        protected int Dni { get; set; }
        [Required(ErrorMessage = "El telefono es requerido")]
        [Phone(ErrorMessage = "El teléfono debe ser un número de teléfono válido")]
        protected string Telefono { get; set; }
        [Required(ErrorMessage = "La direccion es requerida")]
        [MinLength(5, ErrorMessage = "La dirección debe tener al menos 5 caracteres")]
        [MaxLength(255, ErrorMessage = "La dirección no puede tener más de 255 caracteres")]
        protected string Direccion { get; set; }

        protected Persona(string nombre, string apellido, int dni, string telefono, string direccion)
        {
            this.Nombre= nombre;
            this.Apellido = apellido;
            this.Dni = dni;
            this.Telefono = telefono;
            this.Direccion = direccion;

        }
    }
}
