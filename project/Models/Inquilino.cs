using System.ComponentModel.DataAnnotations;

namespace project.Models
{
    public class Inquilino(string nombre, string apellido, int dni, string telefono, string direccion) : Persona(nombre, apellido, dni, telefono, direccion)
    {
        [Key]
        public int IdInquilino { get; set; }
        public List<Contrato> Contratos { get; set; } = new List<Contrato>();

        // Constructor vacío
        public Inquilino() : this(default!, default!, default, default!, default!) { }

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
