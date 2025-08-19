using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.Configuration;
using project.Data;
using project.Models.Interfaces;
using project.Models;

namespace project.Models.Repos
{
    public class PropietarioRepository : RepositorioBase, IPropietarioRepository
    {
        private readonly IDbConnectionFactory _dbFactory;

        public PropietarioRepository(IConfiguration configuration, IDbConnectionFactory dbFactory)
            : base(configuration, dbFactory)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        public int Alta(Propietario propietario)
        {
            if (propietario == null) throw new ArgumentNullException(nameof(propietario));

            int res = -1;

            // Obtener una conexión abierta de la fábrica (síncrono)
            using var conn = _dbFactory.CreateOpenConnectionAsync().GetAwaiter().GetResult();
            using var tran = conn.BeginTransaction();
            try
            {
                // 1) Insertar en Persona
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = tran;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"
                        INSERT INTO Personas (Nombre, Apellido, Dni, Telefono, Direccion, Email, Logico)
                        VALUES (@Nombre, @Apellido, @Dni, @Telefono, @Direccion, @Email, @Logico);
                    ";
                    AddParam(cmd, "@Nombre", propietario.Nombre);
                    AddParam(cmd, "@Apellido", propietario.Apellido);
                    AddParam(cmd, "@Dni", propietario.Dni);
                    AddParam(cmd, "@Telefono", propietario.Telefono);
                    AddParam(cmd, "@Direccion", propietario.Direccion);
                    AddParam(cmd, "@Email", propietario.Email);
                    AddParam(cmd, "@Logico", true);

                    cmd.ExecuteNonQuery();

                    // Obtener IdPersona (MySQL)
                    cmd.Parameters.Clear();
                    cmd.CommandText = "SELECT LAST_INSERT_ID();";
                    var personaIdObj = cmd.ExecuteScalar();
                    var personaId = Convert.ToInt32(personaIdObj);

                    // 2) Insertar en Propietarios referenciando Persona
                    cmd.Parameters.Clear();
                    cmd.CommandText = @"
                        INSERT INTO Propietarios (IdPersona, LogicoProp)
                        VALUES (@IdPersona, @LogicoProp);
                    ";
                    AddParam(cmd, "@IdPersona", personaId);
                    AddParam(cmd, "@LogicoProp", true);
                    cmd.ExecuteNonQuery();

                    // Obtener IdPropietario (si es AUTO_INCREMENT)
                    cmd.Parameters.Clear();
                    cmd.CommandText = "SELECT LAST_INSERT_ID();";
                    var propIdObj = cmd.ExecuteScalar();
                    res = Convert.ToInt32(propIdObj);

                    // Commit
                    tran.Commit();

                    // asignar ids al objeto
                    propietario.IdPropietario = res;
                    // si tenés IdPersona en el modelo:
                    // propietario.IdPersona = personaId;
                }
            }
            catch
            {
                try { tran.Rollback(); } catch { }
                throw;
            }

            return res;
        }

        
        private static void AddParam(IDbCommand cmd, string name, object? value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name.StartsWith("@") ? name : "@" + name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        /* AddParam Hace lo siguiente:

        Crea un parámetro para el comando IDbCommand (cmd.CreateParameter()).
        Normaliza el nombre añadiendo "@" si no lo trae (p. ej. "Nombre" → "@Nombre").
        Asigna el valor del parámetro; si es null usa DBNull.Value (necesario para DB).
        Añade el parámetro al comando (cmd.Parameters.Add(p)).
        Para qué sirve en el flujo

        Permite pasar valores seguros a la consulta (evita concatenar strings → previene SQL injection).
        Se usa antes de ExecuteNonQuery/ExecuteScalar/ExecuteReader para que el comando tenga sus parámetros.
        Consideraciones y mejoras posibles

        No fija DbType/Size; en algunos casos conviene establecer p.DbType o p.Size para evitar conversiones o problemas con tipos/longitudes.
        Algunos proveedores aceptan prefijos distintos (MySQL acepta @); si cambias proveedor revisá el prefijo.
        Para rendimiento con ciertos proveedores puedes preferir crear parámetros con tipos explícitos en lugar de confiar en la inferencia.
        */

        public int ObtenerCantidad() => throw new NotImplementedException();
        public Propietario ObtenerPorDni(int dni) => throw new NotImplementedException();
        public IList<Propietario> ObtenerPorNombre(string nombre) => throw new NotImplementedException();
        public IList<Propietario> ObtenerTodos() => throw new NotImplementedException();
    }
}