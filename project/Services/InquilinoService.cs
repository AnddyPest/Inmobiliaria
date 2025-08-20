

using MySql.Data.MySqlClient;
using project.Data;
using project.Models;
using project.Models.Interfaces;
using System.Data;
using System.Threading.Tasks;

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

        public async Task<(string?, Inquilino?)> GetInquilinoById(int idInquilino)
        {
            try
            {
                string query = @$"select p.*
                                from personas as p
                                inner join inquilino i on p.idPersona = i.idPersona
                                where i.idInquilino = @idInquilino";

                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (MySqlCommand commando = new MySqlCommand(query, connection))
                    {
                        Inquilino inquilinoFromDatabase = new Inquilino();
                        commando.Parameters.AddWithValue("@idInquilino", idInquilino);
                        using (var reader = await commando.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                inquilinoFromDatabase.IdInquilino = reader.GetInt32("idPersona");
                                inquilinoFromDatabase.Nombre = reader.GetString("Nombre");
                                inquilinoFromDatabase.Apellido = reader.GetString("Apellido");
                                inquilinoFromDatabase.Dni = reader.GetInt32("Dni");
                                inquilinoFromDatabase.Telefono = reader.GetInt64("Telefono");
                                inquilinoFromDatabase.Direccion = reader.GetString("Direccion");
                                inquilinoFromDatabase.Logico = reader.GetBoolean("estado");

                            }
                        }
                        if (inquilinoFromDatabase.IdInquilino == 0 || inquilinoFromDatabase == null)
                        {
                            return ($"No se encontró un inquilino con ID {idInquilino}", null);
                        }
                        return (null, inquilinoFromDatabase);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return ($"Error al obtener el inquilino con ID {idInquilino}: {ex}", null);
            }
            
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
