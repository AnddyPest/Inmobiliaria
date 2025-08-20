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
    // Constructor vacío
    public Empleado() : this(default!, default!, default, default!, default!, default!, default) { }
    }
}
