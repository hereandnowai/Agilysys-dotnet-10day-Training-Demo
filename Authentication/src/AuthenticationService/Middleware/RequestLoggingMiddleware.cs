using Serilog;

namespace AuthenticationService.Middleware;

public class RequestLoggingMiddleware
{
 private readonly RequestDelegate _next;

 public RequestLoggingMiddleware(RequestDelegate next)
 {
 _next = next;
 }

 public async Task InvokeAsync(HttpContext context)
 {
 var sw = System.Diagnostics.Stopwatch.StartNew();
 var method = context.Request.Method;
 var path = context.Request.Path;

 try
 {
 await _next(context);
 sw.Stop();
 var status = context.Response.StatusCode;
 Log.Information("HTTP {Method} {Path} responded {Status} in {Elapsed}ms", method, path, status, sw.ElapsedMilliseconds);
 }
 catch (Exception ex)
 {
 sw.Stop();
 Log.Error(ex, "HTTP {Method} {Path} failed after {Elapsed}ms", method, path, sw.ElapsedMilliseconds);
 throw;
 }
 }
}

public static class RequestLoggingMiddlewareExtensions
{
 public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
 {
 return app.UseMiddleware<RequestLoggingMiddleware>();
 }
}
