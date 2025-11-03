namespace First_Sample_Project_Prompting.Models;

/// <summary>
/// Represents a weather forecast for a specific date.
/// </summary>
public class WeatherForecast
{
    /// <summary>
    /// Gets or sets the date of the forecast.
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// Gets or sets the temperature in Celsius.
    /// </summary>
    public int TemperatureC { get; set; }

    /// <summary>
    /// Gets the temperature in Fahrenheit (calculated from Celsius).
    /// </summary>
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    /// <summary>
    /// Gets or sets the weather summary description.
    /// </summary>
  public string? Summary { get; set; }
}
