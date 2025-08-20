

using MySql.Data.MySqlClient;
using project.Data;
using project.Models;
using project.Models.Interfaces;
using System.Data;

namespace project.Services
{
    public class InquilinoService : IInquilinoService
    {
        private string _connectionString;

        public InquilinoService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("Connection");
        }

        public (string?, bool?) AddInquilino(Inquilino inquilino)
        {
            throw new NotImplementedException();
        }

        public async Task<(string?, List<Inquilino>)> GetAllInquilinos()
        {
            try
            {
                string query = @"Select p.* 
                                from persona p 
                                inner join inquilino i On p.idPersona = i.idPersona";
                using(MySqlConnection conexion = new MySqlConnection(_connectionString))
                {
                    await conexion.OpenAsync();
                    using (MySqlCommand command = new MySqlCommand(query, conexion))
                    {
                        List<Inquilino> inquilinos = new List<Inquilino>();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Inquilino inquilino = new Inquilino();
                                inquilino.IdInquilino = reader.GetInt32("idPersona");
                                inquilino.Nombre = reader.GetString("Nombre");
                                inquilino.Apellido = reader.GetString("Apellido");
                                inquilino.Dni = reader.GetInt32("Dni");
                                inquilino.Telefono = reader.GetInt64("Telefono");
                                inquilino.Direccion = reader.GetString("Direccion");
                                inquilino.Logico = reader.GetBoolean("estado");
                                inquilinos.Add(inquilino);
                            }

                        }
                        Console.WriteLine("Funciono");
                        Console.WriteLine(inquilinos);
                        return (null, inquilinos);
                    }
                }
            }catch (Exception ex)
            {
                Console.WriteLine(ex);
                return ($"Error al obtener los inquilinos: {ex}", null);
            }
        }

        public (string?, Inquilino) GetInquilinoById(int idInquilino)
        {
            throw new NotImplementedException();
        }

        public (string?, bool?) LogicalDeleteInquilino(int idInquilino)
        {
            throw new NotImplementedException();
        }

        public (string?, bool?) UpdateInquilino(Inquilino inquilino)
        {
            throw new NotImplementedException();
        }
    }
}
