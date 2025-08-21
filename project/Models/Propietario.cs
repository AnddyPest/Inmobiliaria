using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace project.Models
{

    public class Propietario(string nombre, string apellido, int dni, string telefono, string direccion, string email, bool logico, bool EstadoPropietario, Persona persona)
        : Persona(nombre, apellido, dni, telefono, direccion, email, logico)
    {
        [Key]
        public int IdPropietario { get; set; }
        public List<Inmueble> Inmuebles { get; set; } = [];
        public Persona Persona { get; set; } = persona;
        
        public bool EstadoPropietario { get; set; } = EstadoPropietario;

        // Constructor vacío -> por defecto true para los flags lógicos
        public Propietario() : this(default!, default!, default, default!, default!, default!, default!, default!, default!) { }

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