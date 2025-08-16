using System.ComponentModel.DataAnnotations;


namespace project.Models
{

    public class Propietario : Persona
    {
        [Key]
        public int IdPropietario { get; set; }
        public List<Inmueble> Inmuebles { get; set; } = new List<Inmueble>();
        public Propietario( string nombre, string apellido, int dni, string telefono, string direccion)
        : base(nombre, apellido, dni, telefono, direccion)
        {}
        public void AgregarInmueble(Inmueble inmueble)
        {
            if (inmueble == null)
            {
                throw new ArgumentNullException(nameof(inmueble), "El inmueble no puede ser nulo.");
            }
            Inmuebles.Add(inmueble);
        }

    }

}