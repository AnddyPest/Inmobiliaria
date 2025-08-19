using System.Data;
using System.Threading.Tasks;

namespace project.Data
{
    
    /// Fábrica para crear conexiones ADO.NET.
    
    public interface IDbConnectionFactory
    {
        
        /// Crea una conexión sin abrir.
        
        IDbConnection CreateConnection();

        /// Crea y abre una conexión de forma asíncrona.
        /// El llamador dispone de la conexión.
        Task<IDbConnection> CreateOpenConnectionAsync();
    }
}
