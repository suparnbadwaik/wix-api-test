using System.Text;
using System.Text;
using System.IO.Pipelines;
using System.Buffers;

namespace WixInstallation
{
    public class ErrorLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorLoggingMiddleware> _logger;

        public ErrorLoggingMiddleware(RequestDelegate next,
                                      ILogger<ErrorLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                string body = await ReadBodySafely(context);

                _logger.LogInformation("Incoming Request {Path}\nBody: {Body}",
                    context.Request.Path, body);

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Exception caught in middleware for request {Path}",
                    context.Request.Path);

                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Internal server error");
            }
        }

        private async Task<string> ReadBodySafely(HttpContext context)
        {
            var reader = context.Request.BodyReader;
            var result = await reader.ReadAsync();
            var buffer = result.Buffer;

            // Convert entire buffer to string (safe for chunked)
            string body = Encoding.UTF8.GetString(buffer.ToArray());

            // Mark buffer as consumed
            context.Request.BodyReader.AdvanceTo(buffer.End);

            // Reset stream for controllers
            var bytes = Encoding.UTF8.GetBytes(body);
            context.Request.Body = new MemoryStream(bytes);

            return body;
        }
    }

}