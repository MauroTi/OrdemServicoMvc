using Microsoft.AspNetCore.Mvc;
using OrdemServicoMvc.Models.DTOs;
using OrdemServicoMvc.Services;
using OrdemServicoMvc.Models.ViewModels;

namespace OrdemServicoMvc.Controllers
{
    public class ClientesController : Controller
    {
        private readonly IClienteService _clienteService;

        public ClientesController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        public async Task<IActionResult> Index(
    string? termoBusca,
    string ordenarPor = "nome",
    string direcao = "asc",
    int pagina = 1,
    int tamanhoPagina = 10)
        {
            var clientes = await _clienteService.ObterPaginadoAsync(
                termoBusca,
                ordenarPor,
                direcao,
                pagina,
                tamanhoPagina);

            var totalRegistros = await _clienteService.ContarAsync(termoBusca);

            var viewModel = new ClientesIndexViewModel
            {
                Clientes = clientes,
                TermoBusca = termoBusca,
                OrdenarPor = ordenarPor,
                Direcao = direcao,
                PaginaAtual = pagina,
                TamanhoPagina = tamanhoPagina,
                TotalRegistros = totalRegistros
            };

            return View(viewModel);
        }

        public IActionResult Create()
        {
            return View(new CriarClienteDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CriarClienteDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            await _clienteService.AdicionarAsync(dto);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var cliente = await _clienteService.ObterPorIdAsync(id);

            if (cliente == null)
                return NotFound();

            return View(cliente);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var cliente = await _clienteService.ObterPorIdAsync(id);

            if (cliente == null)
                return NotFound();

            var dto = new EditarClienteDto
            {
                Id = cliente.Id,
                Nome = cliente.Nome,
                Telefone = cliente.Telefone,
                Email = cliente.Email
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditarClienteDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            await _clienteService.AtualizarAsync(dto);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var cliente = await _clienteService.ObterPorIdAsync(id);

            if (cliente == null)
                return NotFound();

            return View(cliente);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _clienteService.RemoverAsync(id);

            return RedirectToAction(nameof(Index));
        }
    }
}