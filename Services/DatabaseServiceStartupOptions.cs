namespace OrdemServicoMvc.Services
{
    public class DatabaseServiceStartupOptions
    {
        public bool Enabled { get; set; } = true;

        public string ServiceName { get; set; } = "postgresql-x64-18";

        public int StartupTimeoutSeconds { get; set; } = 15;
    }
}
