using OtusPracticum.Models;
using System.Diagnostics;
using System.Text.Json;

namespace OtusPracticum.Middleware
{
    public class RequestLoggingMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;
        private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

        public async Task Invoke(HttpContext context, ILogger<RequestLoggingMiddleware> logger)
        {
            var sw = new Stopwatch();
            sw.Start();
            try
            {
                var request_id = Guid.NewGuid().ToString();
                context.Request.Headers.Append("x-request-id", request_id);

                context.Response.OnStarting(state => {
                    var httpContext = (HttpContext)state;
                    httpContext.Response.Headers.Append("x-request-id", request_id);
                    return Task.CompletedTask;
                }, context);

                await _next(context);
            }
            finally
            {
                sw.Stop();
                var requestLog = new RequestLog
                {
                    Id = context.Request.Headers["x-request-id"]!,
                    Method = context.Request.RouteValues["controller"] + "/" + context.Request.RouteValues["action"],
                    Host = context.Request.Host.ToString(),
                    Path = context.Request.Path.ToString(),
                    QueryParams = !string.IsNullOrWhiteSpace(context.Request.QueryString.Value) ? context.Request.QueryString.Value : null,
                    Type = context.Request.Method,
                    IPAddress = context.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                    ExecutionTimeMs = sw.Elapsed.TotalMilliseconds,
                    RequestHeaders = string.Join("; ", context.Request.Headers),
                    ResponseHeaders = string.Join("; ", context.Response.Headers),
                    ResponseStatus = context.Response.StatusCode
                };
                //context.Response.Headers.Append("x-request-id", requestLog.Id);
                logger.LogInformation("{request}", JsonSerializer.Serialize(requestLog, _serializerOptions));
            }
        }
    }
}
