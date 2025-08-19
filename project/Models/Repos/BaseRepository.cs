using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using project.Data;

namespace project.Models.Repos
{
	public abstract class RepositorioBase
    {
        protected readonly IConfiguration configuration;
		protected readonly string connectionString;
        protected readonly IDbConnectionFactory DbFactory;

        protected RepositorioBase(IConfiguration configuration, IDbConnectionFactory dbFactory)
        {
			this.configuration = configuration;
			connectionString = configuration["ConnectionStrings:DefaultConnection"]!;
            DbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        protected async Task<int> ExecuteAsync(string sql, IEnumerable<(string Name, object? Value)>? parameters = null, CancellationToken ct = default)
        {
            using var conn = await DbFactory.CreateOpenConnectionAsync();
            using var cmd = CreateCommand(conn, sql, parameters);
            return await Task.Run(() => cmd.ExecuteNonQuery(), ct);
        }

        protected async Task<List<T>> QueryAsync<T>(string sql, Func<IDataRecord, T> map, IEnumerable<(string Name, object? Value)>? parameters = null, CancellationToken ct = default)
        {
            var list = new List<T>();
            using var conn = await DbFactory.CreateOpenConnectionAsync();
            using var cmd = CreateCommand(conn, sql, parameters);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(map(reader));
            return list;
        }

        private static IDbCommand CreateCommand(IDbConnection conn, string sql, IEnumerable<(string Name, object? Value)>? parameters)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            if (parameters != null)
            {
                foreach (var (Name, Value) in parameters)
                {
                    var p = cmd.CreateParameter();
                    p.ParameterName = Name.StartsWith("@") ? Name : "@" + Name;
                    p.Value = Value ?? DBNull.Value;
                    cmd.Parameters.Add(p);
                }
            }
            return cmd;
        }
    }
}