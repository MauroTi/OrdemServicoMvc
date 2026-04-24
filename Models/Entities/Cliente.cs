using System.ComponentModel.DataAnnotations;

namespace OrdemServicoMvc.Models.Entities
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(150, ErrorMessage = "Máximo de 150 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Telefone inválido.")]
        [StringLength(20, ErrorMessage = "Máximo de 20 caracteres.")]
        public string? Telefone { get; set; }

        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        [StringLength(150, ErrorMessage = "Máximo de 150 caracteres.")]
        public string? Email { get; set; }
    }
}