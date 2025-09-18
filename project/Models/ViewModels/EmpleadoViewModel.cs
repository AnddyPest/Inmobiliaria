using project.Models;

namespace project.Models.ViewModels
{
    public class EmpleadoViewModel
    {
        public EmpleadoViewModel() { }
        public Empleado Empleado { get; set; } = new Empleado();
        public List<Empleado> Empleados { get; set; } = new List<Empleado>();
    }
}