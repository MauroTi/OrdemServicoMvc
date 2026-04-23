using System.ComponentModel.DataAnnotations;

namespace OrdemServicoMvc.Models.DTOs
{
    public class CriarClienteDto
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(150, ErrorMessage = "O nome deve ter no máximo 150 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "O telefone deve ter new máximo20 caracteres.")]
        public string? Telefone { get; set; }

        [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
        [StringLength(150, ErrorMessage = "O e-mail deve ter no máximo 150 caracteres.")]
        public string Email { get; set; }  
    }
}
