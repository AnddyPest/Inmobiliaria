using System.ComponentModel.DataAnnotations;

namespace project.Models
{
    public class Rol(string nombre)
    {
        [Key]
        public int IdRol { get; set; }
        public string Nombre { get; set; } = nombre;

        public Rol() : this(default!) { }
    }
}
