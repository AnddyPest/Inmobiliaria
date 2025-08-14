using System.ComponentModel.DataAnnotations;


namespace project.Models
{

    public class Propietario
    {

        [Key]
        [Display(Name = "Código Interno")]
        public int PropietarioId { get; set; }
        [Required, StringLength(100)]
        public string Nombre { get; set; }
        [Required, StringLength(100)]
        public string Apellido { get; set; }
        [Required, StringLength(10)]
        public string Dni { get; set; }
        [Required, Phone]
        [Display(Name = "Teléfono")]
        [StringLength(15)]
        public string Telefono { get; set; }
        [Required, EmailAddress]
        [StringLength(200)]
        public string Email { get; set; }
        //USAR HASH PARA ENVIAR A LA BD - HASHEAR AL MOMENTO DE GUARDAR EN REPO
        [Required(ErrorMessage = "La clave es obligatoria"), DataType(DataType.Password), MinLength(8)]
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