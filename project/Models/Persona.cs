using System.ComponentModel.DataAnnotations;

namespace project.Models
{
    public abstract class Persona(string nombre, string apellido, int dni, long telefono, string direccion, string email, bool estado)
    {
        
        
        [Key]
        public int IdPersona { get; set; }
        [Required(ErrorMessage = "El nombre es requerido")]
        [MinLength(2, ErrorMessage = "El nombre debe tener al menos 2 caracteres")]
        [MaxLength(255, ErrorMessage = "El nombre no puede tener más de 255 caracteres")]
        public string Nombre { get; set; } = nombre;
        [Required(ErrorMessage = "El apellido es requerido")]
        [MinLength(2, ErrorMessage = "El apellido debe tener al menos 2 caracteres")]
        [MaxLength(100, ErrorMessage = "El apellido no puede tener más de 100 caracteres")]
        public string Apellido { get; set; } = apellido;
        [Required(ErrorMessage = "El DNI es requerido")]
        public int Dni { get; set; } = dni;
        [Required(ErrorMessage = "El telefono es requerido")]
        [Phone(ErrorMessage = "El teléfono debe ser un número de teléfono válido")]
        public long Telefono { get; set; } = telefono;
        [Required(ErrorMessage = "La direccion es requerida")]
        [MinLength(5, ErrorMessage = "La dirección debe tener al menos 5 caracteres")]
        [MaxLength(255, ErrorMessage = "La dirección no puede tener más de 255 caracteres")]
        public string Direccion { get; set; } = direccion;
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El email debe ser una dirección de correo electrónico válida")]
        public string Email { get; set; } = email;

        public Boolean estado { get; set; } = estado;

    public Persona() : this(default!, default!, default, default!, default!, default!, default!) { }

    }
    
}
