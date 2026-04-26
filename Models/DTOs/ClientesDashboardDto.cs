namespace OrdemServicoMvc.Models.DTOs
{
    public class ClientesDashboardDto
    {
        public int TotalClientes { get; set; }

        public int ComEmail { get; set; }

        public int ComTelefone { get; set; }

        public int CadastroCompleto { get; set; }

        public int SemContato { get; set; }

        public IEnumerable<DashboardFatiaDto> DistribuicaoContato { get; set; } = [];
    }
}
