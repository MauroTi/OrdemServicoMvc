namespace OrdemServicoMvc.Models
{
    public class ErrorViewModel
    {
        public string Title { get; set; } = "Error.";

        public string Message { get; set; } = "An error occurred while processing your request.";

        public string? Detail { get; set; }

        public string? PowerShellCommand { get; set; }

        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
