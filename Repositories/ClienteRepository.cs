using Dapper;
using Microsoft.Extensions.Logging;
using OrdemServicoMvc.Data;
using OrdemServicoMvc.Models.Entities;

namespace OrdemServicoMvc.Repositories
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly DbConnectionFactory _connectionFactory;
        private readonly ILogger<ClienteRepository> _logger;

        public ClienteRepository(
            DbConnectionFactory connectionFactory,
            ILogger<ClienteRepository> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<Cliente>> ObterTodosAsync()
        {
            const string sql = @"
                SELECT id, nome, telefone, email
                FROM clientes
                ORDER BY id;";

            using var connection = _connectionFactory.CreateConnection();

            var start = DateTime.UtcNow;

            var result = await connection.QueryAsync<Cliente>(sql);

            _logger.LogInformation(
                "Query ObterTodosAsync executada em {ms} ms",
                (DateTime.UtcNow - start).TotalMilliseconds);

            return result;
        }

        public async Task<Cliente?> ObterPorIdAsync(int id)
        {
            const string sql = @"
                SELECT id, nome, telefone, email
                FROM clientes
                WHERE id = @Id;";

            using var connection = _connectionFactory.CreateConnection();

            var start = DateTime.UtcNow;

            var result = await connection.QueryFirstOrDefaultAsync<Cliente>(sql, new { Id = id });

            _logger.LogInformation(
                "Query ObterPorIdAsync executada em {ms} ms | Id: {id}",
                (DateTime.UtcNow - start).TotalMilliseconds,
                id);

            return result;
        }

        public async Task<int> AdicionarAsync(Cliente cliente)
        {
            const string sql = @"
                INSERT INTO clientes (nome, telefone, email, criado_em)
                VALUES (@Nome, @Telefone, @Email, NOW())
                RETURNING id;";

            using var connection = _connectionFactory.CreateConnection();

            var start = DateTime.UtcNow;

            var id = await connection.ExecuteScalarAsync<int>(sql, cliente);

            _logger.LogInformation(
                "Query AdicionarAsync executada em {ms} ms | Id gerado: {id}",
                (DateTime.UtcNow - start).TotalMilliseconds,
                id);

            return id;
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

            var start = DateTime.UtcNow;

            await connection.ExecuteAsync(sql, cliente);

            _logger.LogInformation(
                "Query AtualizarAsync executada em {ms} ms | Id: {id}",
                (DateTime.UtcNow - start).TotalMilliseconds,
                cliente.Id);
        }

        public async Task RemoverAsync(int id)
        {
            const string sql = @"
                DELETE FROM clientes
                WHERE id = @Id;";

            using var connection = _connectionFactory.CreateConnection();

            var start = DateTime.UtcNow;

            await connection.ExecuteAsync(sql, new { Id = id });

            _logger.LogInformation(
                "Query RemoverAsync executada em {ms} ms | Id: {id}",
                (DateTime.UtcNow - start).TotalMilliseconds,
                id);
        }

        public async Task<IEnumerable<Cliente>> ObterPaginadoAsync(
            string? termoBusca,
            string ordenarPor,
            string direcao,
            int pagina,
            int tamanhoPagina)
        {
            using var connection = _connectionFactory.CreateConnection();

            var colunasPermitidas = new Dictionary<string, string>
            {
                { "id", "id" },
                { "nome", "nome" },
                { "telefone", "telefone" },
                { "email", "email" }
            };

            var chaveOrdenacao = ordenarPor?.ToLower() ?? "nome";

            var colunaOrdenacao = colunasPermitidas.ContainsKey(chaveOrdenacao)
                ? colunasPermitidas[chaveOrdenacao]
                : "nome";

            var direcaoOrdenacao = direcao?.ToLower() == "desc" ? "DESC" : "ASC";

            if (pagina < 1)
                pagina = 1;

            if (tamanhoPagina < 1)
                tamanhoPagina = 10;

            var offset = (pagina - 1) * tamanhoPagina;

            var sql = $@"
                SELECT id, nome, telefone, email
                FROM clientes
                WHERE 
                    (@TermoBusca IS NULL OR @TermoBusca = ''
                     OR nome ILIKE '%' || @TermoBusca || '%'
                     OR telefone ILIKE '%' || @TermoBusca || '%'
                     OR email ILIKE '%' || @TermoBusca || '%')
                ORDER BY {colunaOrdenacao} {direcaoOrdenacao}
                LIMIT @TamanhoPagina OFFSET @Offset;";

            var start = DateTime.UtcNow;

            var result = await connection.QueryAsync<Cliente>(sql, new
            {
                TermoBusca = termoBusca,
                TamanhoPagina = tamanhoPagina,
                Offset = offset
            });

            _logger.LogInformation(
                "Query ObterPaginadoAsync executada em {ms} ms | Página: {pagina} | Tamanho: {tamanhoPagina} | Busca: {termoBusca}",
                (DateTime.UtcNow - start).TotalMilliseconds,
                pagina,
                tamanhoPagina,
                termoBusca);

            return result;
        }

        public async Task<int> ContarAsync(string? termoBusca)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
                SELECT COUNT(*)
                FROM clientes
                WHERE 
                    (@TermoBusca IS NULL OR @TermoBusca = ''
                     OR nome ILIKE '%' || @TermoBusca || '%'
                     OR telefone ILIKE '%' || @TermoBusca || '%'
                     OR email ILIKE '%' || @TermoBusca || '%');";

            var start = DateTime.UtcNow;

            var total = await connection.ExecuteScalarAsync<int>(sql, new
            {
                TermoBusca = termoBusca
            });

            _logger.LogInformation(
                "Query ContarAsync executada em {ms} ms | Total: {total} | Busca: {termoBusca}",
                (DateTime.UtcNow - start).TotalMilliseconds,
                total,
                termoBusca);

            return total;
        }
    }
}