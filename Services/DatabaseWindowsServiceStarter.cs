using System.Diagnostics;
using Microsoft.Extensions.Options;

namespace OrdemServicoMvc.Services
{
    public class DatabaseWindowsServiceStarter
    {
        private readonly DatabaseServiceStartupOptions _options;
        private readonly ILogger<DatabaseWindowsServiceStarter> _logger;

        public DatabaseWindowsServiceStarter(
            IOptions<DatabaseServiceStartupOptions> options,
            ILogger<DatabaseWindowsServiceStarter> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task EnsureStartedAsync(CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
            {
                _logger.LogInformation("Inicializacao automatica do servico do banco desabilitada.");
                return;
            }

            if (!OperatingSystem.IsWindows())
            {
                _logger.LogInformation("Verificacao automatica do servico do banco ignorada fora do Windows.");
                return;
            }

            try
            {
                var status = await QueryServiceStatusAsync(cancellationToken);

                if (string.Equals(status, "RUNNING", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("Servico {ServiceName} ja esta em execucao.", _options.ServiceName);
                    return;
                }

                _logger.LogInformation(
                    "Servico {ServiceName} esta com status {Status}. Tentando iniciar automaticamente.",
                    _options.ServiceName,
                    status ?? "desconhecido");

                await StartServiceAsync(cancellationToken);
                await WaitUntilRunningAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Nao foi possivel garantir a inicializacao do servico {ServiceName}.",
                    _options.ServiceName);
            }
        }

        private async Task<string?> QueryServiceStatusAsync(CancellationToken cancellationToken)
        {
            var output = await RunScCommandAsync($"query \"{_options.ServiceName}\"", cancellationToken);

            using var reader = new StringReader(output);
            string? line;

            while ((line = reader.ReadLine()) is not null)
            {
                if (!line.Contains("STATE", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return parts.LastOrDefault();
            }

            return null;
        }

        private async Task StartServiceAsync(CancellationToken cancellationToken)
        {
            await RunScCommandAsync($"start \"{_options.ServiceName}\"", cancellationToken);
        }

        private async Task WaitUntilRunningAsync(CancellationToken cancellationToken)
        {
            var timeout = TimeSpan.FromSeconds(_options.StartupTimeoutSeconds);
            var startedAt = DateTime.UtcNow;

            while (DateTime.UtcNow - startedAt < timeout)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var status = await QueryServiceStatusAsync(cancellationToken);

                if (string.Equals(status, "RUNNING", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("Servico {ServiceName} iniciado com sucesso.", _options.ServiceName);
                    return;
                }

                await Task.Delay(1000, cancellationToken);
            }

            throw new TimeoutException(
                $"O servico {_options.ServiceName} nao ficou em execucao dentro de {_options.StartupTimeoutSeconds} segundos.");
        }

        private async Task<string> RunScCommandAsync(string arguments, CancellationToken cancellationToken)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "sc.exe",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            var standardOutput = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            var standardError = await process.StandardError.ReadToEndAsync(cancellationToken);

            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"Falha ao executar '{startInfo.FileName} {startInfo.Arguments}'. Saida: {standardOutput} {standardError}".Trim());
            }

            return standardOutput;
        }
    }
}
