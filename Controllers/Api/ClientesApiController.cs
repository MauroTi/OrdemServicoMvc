using Microsoft.AspNetCore.Mvc;
using OrdemServicoMvc.Models.DTOs;
using OrdemServicoMvc.Services;

namespace OrdemServicoMvc.Controllers.Api
{
    [ApiController]
    [Route("api/clientes")]
    public class ClientesApiController : ControllerBase
    {
        private readonly IClienteService _clienteService;

        public ClientesApiController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        // GET: /api/clientes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClienteDto>>> ObterTodos()
        {
            var clientes = await _clienteService.ObterTodosAsync();
            return Ok(clientes);
        }

        // GET: /api/clientes/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ClienteDto>> ObterPorId(int id)
        {
            var cliente = await _clienteService.ObterPorIdAsync(id);

            if (cliente == null)
                return NotFound(new { mensagem = "Cliente não encontrado." });

            return Ok(cliente);
        }

        // POST: /api/clientes
        [HttpPost]
        public async Task<ActionResult> Criar([FromBody] CriarClienteDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var id = await _clienteService.AdicionarAsync(dto);

            return CreatedAtAction(
                nameof(ObterPorId),
                new { id },
                new { id, mensagem = "Cliente criado com sucesso." }
            );
        }

        // PUT: /api/clientes/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Atualizar(int id, [FromBody] EditarClienteDto dto)
        {
            if (id != dto.Id)
                return BadRequest(new { mensagem = "O ID da URL é diferente do ID enviado no corpo." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var clienteExistente = await _clienteService.ObterPorIdAsync(id);

            if (clienteExistente == null)
                return NotFound(new { mensagem = "Cliente não encontrado." });

            await _clienteService.AtualizarAsync(dto);

            return NoContent();
        }

        // DELETE: /api/clientes/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Remover(int id)
        {
            var clienteExistente = await _clienteService.ObterPorIdAsync(id);

            if (clienteExistente == null)
                return NotFound(new { mensagem = "Cliente não encontrado." });

            await _clienteService.RemoverAsync(id);

            return NoContent();
        }
    }
}