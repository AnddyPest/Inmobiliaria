using System.ComponentModel.DataAnnotations;


namespace project.Models
{

    public class Propietario
    {

        [Key]
        [Display(Name = "Código Interno")]
        public int PropietarioId { get; set; }
        [Required]
        public string Nombre { get; set; }
        [Required]
        public string Apellido { get; set; }
        [Required]
        public string Dni { get; set; }
        [Required]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required(ErrorMessage = "La clave es obligatoria"), DataType(DataType.Password)]
        public string Clave { get; set; }

        public override string ToString()
        {
            
                var res = $"{Nombre} {Apellido}";
            if (!string.IsNullOrEmpty(Dni))
            {
                res += $", DNI: {Dni}";
            }
            return res;

        }


    }

}