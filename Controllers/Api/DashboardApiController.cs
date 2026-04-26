using Microsoft.AspNetCore.Mvc;
using OrdemServicoMvc.Models.DTOs;
using OrdemServicoMvc.Services;

namespace OrdemServicoMvc.Controllers.Api
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardApiController : ControllerBase
    {
        private readonly IClienteService _clienteService;

        public DashboardApiController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        [HttpGet("clientes")]
        public async Task<ActionResult<ClientesDashboardDto>> ObterResumoClientes()
        {
            var resumo = await _clienteService.ObterResumoDashboardAsync();
            return Ok(resumo);
        }
    }
}
