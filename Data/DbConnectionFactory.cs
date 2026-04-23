using System.Data;
using Npgsql;

namespace OrdemServicoMvc.Data
{
    public class DbConnectionFactory
    {
        private readonly IConfiguration _configuration;
        
        public DbConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IDbConnection CreateConnection()
        {
            var connectionString = _configuration.GetConnectionString("PostgreSqlConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("A connection string 'PostgreSqlConnection' não foi configurada.");

            return new NpgsqlConnection(connectionString);
        }
    }
}
