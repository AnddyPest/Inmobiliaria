namespace project.Models
{
    public class Ciudad
    {
        public int IdCiudad { get; set; }
        public string Nombre { get; set; }
        public string Pais { get; set; }
        public int CodigoPostal { get; set; }

        public Ciudad(string nombre, string pais, int codigoPostal)
        {
            Nombre = nombre;
            Pais = pais;
            CodigoPostal = codigoPostal;
        }
    }
}
