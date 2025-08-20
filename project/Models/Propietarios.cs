using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace project.Models
{

    public class Propietario(string nombre, string apellido, int dni, long telefono, string direccion, string email, bool logico, bool logicoProp)
        : Persona(nombre, apellido, dni, telefono, direccion, email, logico)
    {
        [Key]
        public int IdPropietario { get; set; }
        public List<Inmueble> Inmuebles { get; set; } = [];
        
        public bool LogicoProp { get; set; } = logicoProp;

        // Constructor vacío -> por defecto true para los flags lógicos
        public Propietario() : this(default!, default!, default, default!, default!, default!, true, true) { }

        public void AgregarInmueble(Inmueble inmueble)
        {
            if (inmueble == null) throw new ArgumentNullException(nameof(inmueble));
            Inmuebles.Add(inmueble);
        }
    }

    internal class Contratos
    {
    }
}