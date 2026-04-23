using OrdemServicoMvc.Models.DTOs;

namespace OrdemServicoMvc.Services
{
    public interface IClienteService
    {
        Task<IEnumerable<ClienteDto>> ObterTodosAsync();
        Task<ClienteDto?> ObterPorIdAsync(int id);
        Task<int> AdicionarAsync(CriarClienteDto dto);
        Task AtualizarAsync(EditarClienteDto dto);
        Task RemoverAsync(int id);
    }
}