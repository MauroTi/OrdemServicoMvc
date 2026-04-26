using System.Diagnostics;
using System.Net.Sockets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Npgsql;
using OrdemServicoMvc.Models;

namespace OrdemServicoMvc.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private const string PostgreSqlWindowsServiceName = "postgresql-x64-18";

        private readonly ILogger<GlobalExceptionFilter> _logger;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "Erro nao tratado");

            var isApiRequest =
                context.HttpContext.Request.Path.StartsWithSegments("/api") ||
                context.HttpContext.Request.Headers.Accept.ToString()
                    .Contains("application/json", StringComparison.OrdinalIgnoreCase);

            var databaseServiceStopped = IsPostgreSqlServiceStopped(context.Exception);

            if (isApiRequest)
            {
                context.Result = databaseServiceStopped
                    ? new ObjectResult(new
                    {
                        mensagem = "O servico do PostgreSQL esta parado.",
                        detalhe = "Inicie o servico do banco e tente novamente.",
                        powershell = $"Start-Service {PostgreSqlWindowsServiceName}"
                    })
                    {
                        StatusCode = StatusCodes.Status503ServiceUnavailable
                    }
                    : new ObjectResult(new
                    {
                        mensagem = "Ocorreu um erro interno no servidor."
                    })
                    {
                        StatusCode = StatusCodes.Status500InternalServerError
                    };

                context.ExceptionHandled = true;
                return;
            }

            context.Result = new ViewResult
            {
                ViewName = "Error",
                ViewData = new ViewDataDictionary<ErrorViewModel>(
                    new EmptyModelMetadataProvider(),
                    context.ModelState)
                {
                    Model = new ErrorViewModel
                    {
                        Title = databaseServiceStopped ? "Banco de dados indisponivel." : "Error.",
                        Message = databaseServiceStopped
                            ? "O servico do PostgreSQL esta parado neste momento."
                            : "An error occurred while processing your request.",
                        Detail = databaseServiceStopped
                            ? "Inicie o servico do banco pelo PowerShell e recarregue a pagina."
                            : null,
                        PowerShellCommand = databaseServiceStopped
                            ? $"Start-Service {PostgreSqlWindowsServiceName}"
                            : null,
                        RequestId = context.HttpContext.TraceIdentifier
                    }
                }
            };

            context.ExceptionHandled = true;
        }

        private bool IsPostgreSqlServiceStopped(Exception exception)
        {
            if (!OperatingSystem.IsWindows())
            {
                return false;
            }

            if (!HasLocalPostgreSqlConnectionFailure(exception))
            {
                return false;
            }

            return QueryWindowsServiceStopped();
        }

        private static bool HasLocalPostgreSqlConnectionFailure(Exception exception)
        {
            Exception? current = exception;

            while (current is not null)
            {
                if (current is NpgsqlException || current is SocketException)
                {
                    return true;
                }

                current = current.InnerException;
            }

            return false;
        }

        private bool QueryWindowsServiceStopped()
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "sc.exe",
                    Arguments = $"query \"{PostgreSqlWindowsServiceName}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = startInfo };
                process.Start();
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit(3000);

                return output.Contains("STOPPED", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Nao foi possivel verificar o status do servico do PostgreSQL.");
                return false;
            }
        }
    }
}
