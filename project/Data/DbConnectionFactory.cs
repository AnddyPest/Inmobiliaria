using System;
using System.Data;
using System.Threading.Tasks;
using MySqlConnector;
using Microsoft.Extensions.Configuration;

namespace project.Data
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public DbConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string missing.");
        }

        // Devuelve una conexión nueva (sin abrir)
        public IDbConnection CreateConnection() => new MySqlConnection(_connectionString);

        // Devuelve una conexión nueva ya abierta (caller es responsable de Dispose)
        public async Task<IDbConnection> CreateOpenConnectionAsync()
        {
            var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            return conn;
        }
    }
}
