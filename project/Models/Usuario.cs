using System.ComponentModel.DataAnnotations.Schema;

namespace project.Models
{
    public class Usuario(string gmail, string contrasena, Rol rol, Empleado empleado)
    {
        public int idUsuario { get; set; }
        public string email { get; set; } = gmail;
        public string contrasena { get; set; } = contrasena;
        [ForeignKey("Rol")]
        public int IdRol { get; set; }
        public Rol Rol { get; set; } = rol;
        public Empleado Empleado { get; set; } = empleado;
        public bool estado { get; set; }

        public Usuario() : this(default!, default!, default!, default!) { }
    }
}
