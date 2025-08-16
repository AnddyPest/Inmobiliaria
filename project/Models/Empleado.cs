namespace project.Models
{
    public class Empleado : Persona
    {
        public int IdEmpleado { get; set; }
        public Usuario usuario { get; set; }

        public Empleado(string nombre, string apellido, int dni, string telefono, string direccion)
            : base(nombre, apellido, dni, telefono, direccion)
        {          
        }
        
    }
}
