using Serilog;
using AuthenticationService.Middleware;
using AuthenticationService.Repositories;
using AuthenticationService.Services;
using AuthenticationService.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Serilog configuration
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "AuthenticationService")
    .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/app.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        shared: true,
   flushToDiskInterval: TimeSpan.FromSeconds(1),
 outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .MinimumLevel.Information()
    .CreateLogger();

builder.Host.UseSerilog();

// Configure JWT settings
var jwtSettings = new JwtSettings();
builder.Configuration.GetSection("Jwt").Bind(jwtSettings);
builder.Services.AddSingleton(jwtSettings);

// Add services to the container.
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
builder.Services.AddSingleton<IJwtService, JwtService>();

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
    ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
    ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
        ClockSkew = TimeSpan.Zero
    };

    // Custom event handlers for better error messages
    options.Events = new JwtBearerEvents
    {
  OnAuthenticationFailed = context =>
        {
            if (context.Exception is SecurityTokenExpiredException)
   {
             context.Response.Headers.Append("Token-Expired", "true");
    Log.Warning("Token expired for request to {Path}", context.Request.Path);
            }
         else
       {
            Log.Warning("Authentication failed for request to {Path}: {Error}", 
           context.Request.Path, context.Exception.Message);
   }
        return Task.CompletedTask;
 },
        OnChallenge = context =>
        {
   // Skip the default logic
 context.HandleResponse();

      context.Response.StatusCode = StatusCodes.Status401Unauthorized;
       context.Response.ContentType = "application/json";

 var errorMessage = "Invalid or missing token";
            if (context.AuthenticateFailure is SecurityTokenExpiredException)
            {
                errorMessage = "Token expired";
}
  else if (context.AuthenticateFailure != null)
         {
                errorMessage = "Invalid token";
     }
            else if (string.IsNullOrEmpty(context.Request.Headers.Authorization))
       {
    errorMessage = "Authorization header missing";
            }

            var result = JsonSerializer.Serialize(new { error = errorMessage });
       return context.Response.WriteAsync(result);
      },
        OnTokenValidated = context =>
      {
       var userId = context.Principal?.FindFirst("sub")?.Value;
  Log.Information("Token validated successfully for user {UserId} accessing {Path}", 
      userId, context.Request.Path);
       return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
  options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Authentication Service API",
  Version = "v1",
 Description = "A comprehensive API for authentication with JWT support",
        Contact = new OpenApiContact
   {
Name = "Authentication Service Support",
  Email = "support@authservice.com"
    }
    });

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
     Type = SecuritySchemeType.Http,
     Scheme = "Bearer",
BearerFormat = "JWT",
        In = ParameterLocation.Header,
Description = "Enter 'Bearer' followed by a space and your JWT token.\n\nExample: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
      {
     new OpenApiSecurityScheme
        {
                Reference = new OpenApiReference
 {
            Type = ReferenceType.SecurityScheme,
        Id = "Bearer"
     }
     },
     Array.Empty<string>()
        }
  });

    // Enable XML comments if available
 var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
options.SwaggerEndpoint("/swagger/v1/swagger.json", "Authentication Service API v1");
     options.RoutePrefix = "swagger";
        options.DocumentTitle = "Authentication Service API Documentation";
        options.DisplayRequestDuration();
        options.EnableDeepLinking();
   options.EnableFilter();
      options.ShowExtensions();
    });
}

app.UseHttpsRedirection();

// Custom JWT middleware for pre-validation checks
app.UseMiddleware<JwtAuthenticationMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// Map attribute-routed controllers
app.MapControllers();

app.Run();
