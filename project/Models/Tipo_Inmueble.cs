using System.ComponentModel.DataAnnotations;

namespace project.Models
{
    public class Tipo_Inmueble
    {
        [Key]
        public int id_tipo_inmueble { get; set; }

        public string nombre { get; set; }

       
        public Tipo_Inmueble(int id_tipo_inmueble, string nombre)
        {
            this.id_tipo_inmueble = id_tipo_inmueble;
            this.nombre = nombre;
        }
    }
}
