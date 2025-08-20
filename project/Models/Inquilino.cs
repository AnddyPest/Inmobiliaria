using System.ComponentModel.DataAnnotations;

namespace project.Models
{
    public class Inquilino(string nombre, string apellido, int dni, string telefono, string direccion, string email, bool estado, bool estadoInquilino, Persona persona) : Persona(nombre, apellido, dni, telefono, direccion, email, estado)
    {
        [Key]
        public int IdInquilino { get; set; }
        public List<Contrato> Contratos { get; set; } = [];

        public Persona Persona { get; set; } = persona;

        public bool EstadoInquilino { get; set; } = estadoInquilino;

        // Constructor vacío
        public Inquilino() : this(default!, default!, default!, default, default!, default!, default!, default!, default!) { }

        public void AgregarContrato(Contrato contrato)
        {
            if (contrato == null)
            {
                throw new ArgumentNullException(nameof(contrato), "El contrato no puede ser nulo.");
            }
            Contratos.Add(contrato);
        }
    }
    
}
