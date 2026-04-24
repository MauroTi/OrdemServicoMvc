using OrdemServicoMvc.Models.Entities;

namespace OrdemServicoMvc.Repositories
{
    public interface IClienteRepository
    {
        Task<IEnumerable<Cliente>> ObterTodosAsync();
        Task<Cliente?> ObterPorIdAsync(int id);
        Task<int> AdicionarAsync(Cliente cliente);
        Task<bool> AtualizarAsync(Cliente cliente);
        Task<bool> RemoverAsync(int id);
        Task<IEnumerable<Cliente>> ObterPaginadoAsync(
            string? termoBusca,
            string ordenarPor,
            string direcao,
            int pagina,
            int tamanhoPagina);

        Task<int> ContarAsync(string? termoBusca);
    }
}
