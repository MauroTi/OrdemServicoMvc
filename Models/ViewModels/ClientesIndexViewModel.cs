using OrdemServicoMvc.Models.DTOs;

namespace OrdemServicoMvc.Models.ViewModels
{
    public class ClientesIndexViewModel
    {
        public IEnumerable<ClienteDto> Clientes { get; set; } = [];

        public string? TermoBusca { get; set; }

        public string OrdenarPor { get; set; } = "nome";

        public string Direcao { get; set; } = "asc";

        public int PaginaAtual { get; set; } = 1;

        public int TamanhoPagina { get; set; } = 10;

        public int TotalRegistros { get; set; }

        public int TotalPaginas =>
            (int)Math.Ceiling(TotalRegistros / (double)TamanhoPagina);

        public bool TemPaginaAnterior => PaginaAtual > 1;

        public bool TemProximaPagina => PaginaAtual < TotalPaginas;
    }
}