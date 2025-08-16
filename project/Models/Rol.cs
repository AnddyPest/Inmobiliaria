using System.ComponentModel.DataAnnotations;

namespace project.Models
{
    public class Rol
    {
        [Key]
        public int IdRol { get; set; }
        public string Nombre { get; set; }

        public Rol(string nombre)
        {
            this.Nombre = nombre;
        }
    }
}
