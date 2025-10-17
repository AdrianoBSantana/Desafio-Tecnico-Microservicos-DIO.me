using System.Diagnostics;
using System.Text;

namespace Shared.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestId = Guid.NewGuid().ToString();
            
            // Log da requisição
            await LogRequestAsync(context, requestId);
            
            // Captura a resposta original
            var originalResponseBody = context.Response.Body;
            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante o processamento da requisição {RequestId}", requestId);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                
                // Log da resposta
                await LogResponseAsync(context, requestId, stopwatch.ElapsedMilliseconds);
                
                // Restaura o corpo da resposta original
                await responseBodyStream.CopyToAsync(originalResponseBody);
                context.Response.Body = originalResponseBody;
            }
        }

        private async Task LogRequestAsync(HttpContext context, string requestId)
        {
            var request = context.Request;
            var requestBody = await ReadRequestBodyAsync(request);

            var logMessage = new StringBuilder();
            logMessage.AppendLine($"Request {requestId}:");
            logMessage.AppendLine($"  Method: {request.Method}");
            logMessage.AppendLine($"  Path: {request.Path}");
            logMessage.AppendLine($"  QueryString: {request.QueryString}");
            logMessage.AppendLine($"  Headers: {string.Join(", ", request.Headers.Select(h => $"{h.Key}={h.Value}"))}");
            
            if (!string.IsNullOrEmpty(requestBody))
            {
                logMessage.AppendLine($"  Body: {requestBody}");
            }

            _logger.LogInformation(logMessage.ToString());
        }

        private async Task LogResponseAsync(HttpContext context, string requestId, long elapsedMilliseconds)
        {
            var response = context.Response;
            var responseBody = await ReadResponseBodyAsync(response);

            var logMessage = new StringBuilder();
            logMessage.AppendLine($"Response {requestId}:");
            logMessage.AppendLine($"  StatusCode: {response.StatusCode}");
            logMessage.AppendLine($"  ElapsedTime: {elapsedMilliseconds}ms");
            logMessage.AppendLine($"  Headers: {string.Join(", ", response.Headers.Select(h => $"{h.Key}={h.Value}"))}");
            
            if (!string.IsNullOrEmpty(responseBody) && responseBody.Length < 1000) // Log apenas se for pequeno
            {
                logMessage.AppendLine($"  Body: {responseBody}");
            }

            _logger.LogInformation(logMessage.ToString());
        }

        private async Task<string> ReadRequestBodyAsync(HttpRequest request)
        {
            if (request.ContentLength > 0 && request.ContentType?.Contains("application/json") == true)
            {
                request.EnableBuffering();
                request.Body.Position = 0;
                
                using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                request.Body.Position = 0;
                
                return body;
            }
            
            return string.Empty;
        }

        private async Task<string> ReadResponseBodyAsync(HttpResponse response)
        {
            if (response.ContentLength > 0 && response.ContentType?.Contains("application/json") == true)
            {
                response.Body.Seek(0, SeekOrigin.Begin);
                using var reader = new StreamReader(response.Body, Encoding.UTF8, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                response.Body.Seek(0, SeekOrigin.Begin);
                
                return body;
            }
            
            return string.Empty;
        }
    }
}


