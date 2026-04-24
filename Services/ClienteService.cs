using OrdemServicoMvc.Models.DTOs;
using OrdemServicoMvc.Models.Entities;
using OrdemServicoMvc.Repositories;

namespace OrdemServicoMvc.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _repository;

        public ClienteService(IClienteRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ClienteDto>> ObterTodosAsync()
        {
            var clientes = await _repository.ObterTodosAsync();

            return clientes.Select(cliente => new ClienteDto
            {
                Id = cliente.Id,
                Nome = cliente.Nome,
                Telefone = cliente.Telefone,
                Email = cliente.Email
            });
        }

        public async Task<ClienteDto?> ObterPorIdAsync(int id)
        {
            var cliente = await _repository.ObterPorIdAsync(id);

            if (cliente is null)
                return null;

            return new ClienteDto
            {
                Id = cliente.Id,
                Nome = cliente.Nome,
                Telefone = cliente.Telefone,
                Email = cliente.Email
            };
        }

        public async Task<int> AdicionarAsync(CriarClienteDto dto)
        {
            var cliente = new Cliente
            {
                Nome = dto.Nome,
                Telefone = dto.Telefone,
                Email = dto.Email
            };

            return await _repository.AdicionarAsync(cliente);
        }

        public async Task AtualizarAsync(EditarClienteDto dto)
        {
            var cliente = new Cliente
            {
                Id = dto.Id,
                Nome = dto.Nome,
                Telefone = dto.Telefone,
                Email = dto.Email
            };

            await _repository.AtualizarAsync(cliente);
        }

        public async Task RemoverAsync(int id)
        {
            await _repository.RemoverAsync(id);
        }

        public async Task<IEnumerable<ClienteDto>> ObterPaginadoAsync(
    string? termoBusca,
    string ordenarPor,
    string direcao,
    int pagina,
    int tamanhoPagina)
        {
            pagina = pagina < 1 ? 1 : pagina;
            tamanhoPagina = tamanhoPagina < 1 ? 10 : tamanhoPagina;

            var clientes = await _repository.ObterPaginadoAsync(
                termoBusca,
                ordenarPor,
                direcao,
                pagina,
                tamanhoPagina);

            return clientes.Select(c => new ClienteDto
            {
                Id = c.Id,
                Nome = c.Nome,
                Telefone = c.Telefone,
                Email = c.Email
            });
        }

        public async Task<int> ContarAsync(string? termoBusca)
        {
            return await _repository.ContarAsync(termoBusca);
        }
    }
}