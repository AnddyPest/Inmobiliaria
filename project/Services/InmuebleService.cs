using MySql.Data.MySqlClient;
using project.Helpers;
using project.Models;
using project.Models.Interfaces;

namespace project.Services
{
    public class InmuebleService(IConfiguration configuration) : IInmuebleService
    {
        private readonly string _connectionString = configuration.GetConnectionString("Connection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        public (string?, bool) AgregarInmueble(Inmueble inmueble)
        {
            try
            {

            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message,nameof(InmuebleService),nameof(AgregarInmueble));
                return (ex.Message, false);
            }
        }

        public (string?, bool) ActualizarInmueble(Inmueble inmueble)
        {
            throw new NotImplementedException();
        }

        public (string?, bool) DarDeBajaInmueble(int idInmueble)
        {
            throw new NotImplementedException();
        }

        public (string?, List<Inmueble>?) ObtenerInmueblePorContrato(int idContrato)
        {
            throw new NotImplementedException();
        }

        public (string?, Inmueble?) ObtenerInmueblePorId(int idInmueble)
        {
            try
            {
                if (idInmueble <= 0) return ("El id del inmueble debe ser un valor positivo", null);
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @" SELECT i.* ,ti.*
                                      FROM Inmuebles as i
                                      inner join tipo_inmueble as ti on ti.id_tipo_inmueble = i.id_tipo_inmueble
                                      WHERE IdInmueble = @IdInmueble ";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdInmueble", idInmueble);
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Inmueble inmueble = new Inmueble();
                                Tipo_Inmueble tipo = new Tipo_Inmueble();
                                inmueble.IdInmueble = reader.GetInt32("IdInmueble");
                                inmueble.Uso = reader.GetString("Uso");
                                inmueble.Superficie = reader.GetInt32("Superficie");
                                inmueble.CantAmbientes = reader.GetInt32("CantAmbientes");
                                inmueble.Coordenadas = reader.GetDecimal("Coordenadas");
                                inmueble.Precio = reader.GetDecimal("Precio");
                                inmueble.Direccion = reader.GetString("Direccion");
                                inmueble.ciudad = reader.GetString("ciudad");
                                inmueble.IdPropietario = reader.GetInt32("IdPropietario");
                                inmueble.Disponible = reader.GetBoolean("Disponible");
                                inmueble.estado = reader.GetBoolean("estado");
                                tipo.id_tipo_inmueble = reader.GetInt32("id_tipo_inmueble");
                                tipo.nombre = reader.GetString("nombre");
                                inmueble.Tipo = tipo;
                                return (null, inmueble);
                            }
                            return ($"No se encontró ningún inmueble con el id {idInmueble}", null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HelperFor.imprimirMensajeDeError(ex.Message, nameof(InmuebleService), nameof(ObtenerInmueblePorId));
                return (ex.Message, null);
            }
        }

        public (string?, List<Inmueble>?) ObtenerInmueblesPorPropietario(int dniPropietario)
        {
            throw new NotImplementedException();
        }

        public (string?, List<Inmueble>?) ObtenerTodosLosInmuebles()
        {
            throw new NotImplementedException();
        }

        public (string?, Inmueble?) BuscarInmueblePorDireccion(string direccion)
        {
            throw new NotImplementedException();
        }
    }
}
