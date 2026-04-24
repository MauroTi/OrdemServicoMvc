using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using OrdemServicoMvc.Models;

namespace OrdemServicoMvc.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "Erro não tratado");

            var isApiRequest =
                context.HttpContext.Request.Path.StartsWithSegments("/api") ||
                context.HttpContext.Request.Headers.Accept.ToString()
                    .Contains("application/json", StringComparison.OrdinalIgnoreCase);

            if (isApiRequest)
            {
                context.Result = new ObjectResult(new
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
                        RequestId = context.HttpContext.TraceIdentifier
                    }
                }
            };

            context.ExceptionHandled = true;
        }
    }
}
