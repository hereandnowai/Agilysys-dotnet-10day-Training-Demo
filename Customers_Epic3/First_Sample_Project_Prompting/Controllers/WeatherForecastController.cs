using Microsoft.AspNetCore.Mvc;
using First_Sample_Project_Prompting.Models;

namespace First_Sample_Project_Prompting.Controllers;

/// <summary>
/// Controller for weather forecast operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WeatherForecastController"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
   _logger = logger;
    }

    /// <summary>
    /// Gets a 5-day weather forecast.
    /// </summary>
    /// <returns>A collection of weather forecasts.</returns>
    [HttpGet(Name = "GetWeatherForecast")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<WeatherForecast>> Get()
    {
        _logger.LogInformation("Retrieving weather forecast");
        
        var forecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();

        return Ok(forecast);
    }
}
