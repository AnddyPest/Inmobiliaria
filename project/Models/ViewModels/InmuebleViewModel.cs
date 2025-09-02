namespace project.Models.ViewModels
{
    public class InmuebleViewModel()
    {
        public List<Propietario>? propietarios { get; set; } = [];
        public Inquilino? inquilino { get; set; }
        public Inmueble? inmueble { get; set; }
        public Contrato? contrato { get; set; }
        public List<Tipo_Inmueble>? tipo_Inmueble { get; set; } = new List<Tipo_Inmueble>();
        public List<string> ciudades { get; set; } = ["Buenos Aires", "Córdoba", "Santa Fe", "Mendoza", "Tucumán", "Salta", "Jujuy", "La Rioja", "San Juan", "San Luis", "Neuquén", "Tierra del fuego", "Chaco", "Corrientes", "Entre Ríos", "Formosa", "Misiones"];

    }
}
