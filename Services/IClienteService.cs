using OrdemServicoMvc.Models.DTOs;

namespace OrdemServicoMvc.Services
{
    public interface IClienteService
    {
        Task<IEnumerable<ClienteDto>> ObterTodosAsync();

        Task<ClienteDto?> ObterPorIdAsync(int id);

        Task<int> AdicionarAsync(CriarClienteDto dto);

        Task<bool> AtualizarAsync(EditarClienteDto dto);

        Task<bool> RemoverAsync(int id);

        Task<IEnumerable<ClienteDto>> ObterPaginadoAsync(
            string? termoBusca,
            string ordenarPor,
            string direcao,
            int pagina,
            int tamanhoPagina);

        Task<int> ContarAsync(string? termoBusca);

        Task<ClientesDashboardDto> ObterResumoDashboardAsync();
    }
}
