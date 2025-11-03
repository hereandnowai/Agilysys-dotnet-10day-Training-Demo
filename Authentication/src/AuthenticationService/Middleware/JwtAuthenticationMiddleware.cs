using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text.Json;

namespace AuthenticationService.Middleware;

public class JwtAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtAuthenticationMiddleware> _logger;

    public JwtAuthenticationMiddleware(RequestDelegate next, ILogger<JwtAuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip authentication for auth endpoints
      if (context.Request.Path.StartsWithSegments("/auth"))
        {
   await _next(context);
    return;
        }

   // Check if endpoint requires authorization
        var endpoint = context.GetEndpoint();
        var requiresAuth = endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.IAuthorizeData>() != null;

        if (requiresAuth)
  {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

        if (string.IsNullOrEmpty(authHeader))
   {
         _logger.LogWarning("Request to {Path} missing Authorization header", context.Request.Path);
            await WriteUnauthorizedResponse(context, "Authorization header missing");
           return;
            }

            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
   {
    _logger.LogWarning("Request to {Path} has invalid Authorization header format", context.Request.Path);
      await WriteUnauthorizedResponse(context, "Invalid authorization header format. Expected 'Bearer <token>'");
     return;
         }

         var token = authHeader.Substring("Bearer ".Length).Trim();

  if (string.IsNullOrEmpty(token))
            {
          _logger.LogWarning("Request to {Path} has empty token", context.Request.Path);
     await WriteUnauthorizedResponse(context, "Token is required");
       return;
    }
        }

        await _next(context);
    }

    private async Task WriteUnauthorizedResponse(HttpContext context, string message)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
   context.Response.ContentType = "application/json";

        var errorResponse = new { error = message };
      var json = JsonSerializer.Serialize(errorResponse);

        await context.Response.WriteAsync(json);
    }
}
