using Dapper;
using OrdemServicoMvc.Data;
using OrdemServicoMvc.Models.Entities;

namespace OrdemServicoMvc.Repositories
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly DbConnectionFactory _connectionFactory;

        public ClienteRepository(DbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<Cliente>> ObterTodosAsync()
        {
            const string sql = @"
                SELECT id, nome, telefone, email
                FROM clientes
                ORDER BY id;";

            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryAsync<Cliente>(sql);
        }

        public async Task<Cliente?> ObterPorIdAsync(int id)
        {
            const string sql = @"
                SELECT id, nome, telefone, email
                FROM clientes
                WHERE id = @Id;";

            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Cliente>(sql, new { Id = id });
        }

        public async Task<int> AdicionarAsync(Cliente cliente)
        {
            const string sql = @"
                INSERT INTO clientes (nome, telefone, email)
                VALUES (@Nome, @Telefone, @Email)
                RETURNING id;";

            using var connection = _connectionFactory.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(sql, cliente);
        }

        public async Task AtualizarAsync(Cliente cliente)
        {
            const string sql = @"
                UPDATE clientes
                SET nome = @Nome,
                    telefone = @Telefone,
                    email = @Email
                WHERE id = @Id;";

            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(sql, cliente);
        }

        public async Task RemoverAsync(int id)
        {
            const string sql = @"
                DELETE FROM clientes
                WHERE id = @Id;";

            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(sql, new { Id = id });
        }
    }
}