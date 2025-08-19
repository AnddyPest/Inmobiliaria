namespace project.Models
{
    public class Usuario(string gmail, string contrasena, Rol rol, Empleado empleado)
    {
        public int IdUsuario { get; set; }
        public string Gmail { get; set; } = gmail;
        public string Contrasena { get; set; } = contrasena;
        public Rol Rol { get; set; } = rol;
        public Empleado Empleado { get; set; } = empleado;

        public Usuario() : this(default!, default!, default!, default!) { }
    }
}
