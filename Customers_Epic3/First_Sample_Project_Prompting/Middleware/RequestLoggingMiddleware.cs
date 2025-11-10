using System.Diagnostics;

namespace First_Sample_Project_Prompting.Middleware;

/// <summary>
/// Middleware for logging HTTP request and response details including latency.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestLoggingMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger instance.</param>
    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
  _next = next;
 _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware to log request and response information.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
   var method = context.Request.Method;
var path = context.Request.Path;

        try
        {
          await _next(context);
     }
   finally
{
    stopwatch.Stop();
    var statusCode = context.Response.StatusCode;
         var latency = stopwatch.ElapsedMilliseconds;

        LogRequest(method, path, statusCode, latency);
        }
    }

    /// <summary>
    /// Logs the HTTP request details with structured logging.
    /// </summary>
    /// <param name="method">The HTTP method.</param>
    /// <param name="path">The request path.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="latency">The request latency in milliseconds.</param>
    private void LogRequest(string method, string path, int statusCode, long latency)
    {
      var level = GetLogLevel(statusCode);

    _logger.Log(
      level,
    "HTTP {Method} {Path} responded {StatusCode} in {Latency}ms",
       method,
            path,
     statusCode,
            latency
        );
    }

    /// <summary>
    /// Determines the appropriate log level based on HTTP status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>The appropriate log level.</returns>
    private static LogLevel GetLogLevel(int statusCode)
    {
        return statusCode switch
        {
      >= 500 => LogLevel.Error,
            >= 400 => LogLevel.Warning,
            _ => LogLevel.Information
  };
    }
}
