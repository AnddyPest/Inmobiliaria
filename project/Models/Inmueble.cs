namespace project.Models
{
    public class Inmueble
    {
        public int IdInmueble { get; set; }
        public string Uso { get; set; }
        public string Tipo { get; set; }
        public int CantAmbientes { get; set; }
        public decimal Coordenadas { get; set; }
        public decimal Precio { get; set; }
        public string Direccion { get; set; }
        public Ciudad Ciudad { get; set; }
        public Propietario Propietario { get; set; }
        public bool Disponible { get; set; }

        public Inmueble(string uso, string tipo, int cantAmbientes, decimal coordenadas, decimal precio, string direccion, Ciudad ciudad, Propietario propietario)
        {
            Uso = uso;
            Tipo = tipo;
            CantAmbientes = cantAmbientes;
            Coordenadas = coordenadas;
            Precio = precio;
            Direccion = direccion;
            Ciudad = ciudad ?? throw new ArgumentNullException(nameof(ciudad), "La ciudad no puede ser nula.");
            Propietario = propietario ?? throw new ArgumentNullException(nameof(propietario), "El propietario no puede ser nulo.");
            Disponible = true; 
        }
    }
}
