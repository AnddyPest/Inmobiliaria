namespace project.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string gmail { get; set; }
        public string contraseña { get; set; }
        public Rol rol { get; set; }
        public Empleado empleado { get; set; }

        public Usuario(string gmail, string contraseña, Rol rol, Empleado empleado)
        {
            this.gmail = gmail;
            this.contraseña = contraseña;
            this.rol = rol;
            this.empleado = empleado;
        }
    }
}
