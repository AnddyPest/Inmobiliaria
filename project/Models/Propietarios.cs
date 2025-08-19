using System.ComponentModel.DataAnnotations;


namespace project.Models
{

    public class Propietario(string nombre, string apellido, int dni, string telefono, string direccion)
        : Persona(nombre, apellido, dni, telefono, direccion)
    {
        [Key]
        public int IdPropietario { get; set; }
        public List<Inmueble> Inmuebles { get; set; } = [];
        public List<Contrato> Contratos { get; set; } = [];

        // Constructor vacío
        public Propietario() : this(default!, default!, default, default!, default!) { }

        public void AgregarInmueble(Inmueble inmueble)
        {
            if (inmueble == null)
            {
                throw new ArgumentNullException(nameof(inmueble), "El inmueble no puede ser nulo.");
            }
            Inmuebles.Add(inmueble);
        }
    }

    internal class Contratos
    {
    }
}