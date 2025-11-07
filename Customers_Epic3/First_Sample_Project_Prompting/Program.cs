using Serilog;
using Serilog.Formatting.Compact;
using Microsoft.EntityFrameworkCore;
using First_Sample_Project_Prompting.Middleware;
using First_Sample_Project_Prompting.Data;
using First_Sample_Project_Prompting.Repositories;
using First_Sample_Project_Prompting.Services;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
  .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        new CompactJsonFormatter(),
        "logs/app.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 31)
    .CreateLogger();

try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog();

// Add DbContext with In-Memory Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
     options.UseInMemoryDatabase("CustomerDatabase"));

    // Register repositories
    builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

    // Register services
    builder.Services.AddScoped<ICustomerService, CustomerService>();

    // Add services to the container.
  builder.Services.AddControllers();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

 var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

// Add custom request logging middleware
    app.UseMiddleware<RequestLoggingMiddleware>();

    app.UseAuthorization();

    app.MapControllers();

    Log.Information("Application started successfully");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
