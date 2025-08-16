using System.ComponentModel.DataAnnotations;

namespace project.Models
{
    public class Inquilino : Persona
    {
        [Key]
        public int IdInquilino { get; set; }
        public List<Contrato> Contratos { get; set; } = new List<Contrato>();
        public Inquilino(string nombre, string apellido, int dni, string telefono, string direccion)
            : base(nombre, apellido, dni, telefono, direccion)
        {
        }
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
