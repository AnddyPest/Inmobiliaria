using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace project.Models
{
    public class Empleado(string nombre, string apellido, int dni, string telefono, string direccion, string email, bool estado) : Persona(nombre, apellido, dni, telefono, direccion, email, estado)
    {
        [Key]
        public int IdEmpleado { get; set; }
        [ForeignKey("Usuario")]
        public int IdUsuario { get; set; }
        public string nombre { get; set; } = nombre;
        public string apellido { get; set; } = apellido;
    // Constructor vacío la puta madre
        public Empleado() : this(default!, default!, default, default!, default!, default!, default) { }
    }
}
