using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        
        private static readonly Dictionary<string, double> CityTemperatures = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Warsaw"] = 21.5,
            ["Krakow"] = 19.0,
            ["Gdansk"] = 17.3,
            ["Wroclaw"] = 20.1,
            ["Poznan"] = 18.7
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet("city/{cityName}")]
        public ActionResult<double> GetWeatherByCity(string cityName)
        {
            if (string.IsNullOrWhiteSpace(cityName))
            {
                return BadRequest("City name is required.");
            }

            var normalizedCityName = cityName.Trim();

            if (!CityTemperatures.TryGetValue(normalizedCityName, out var temperature))
            {
                _logger.LogWarning("Weather lookup failed for unknown city: {CityName}", normalizedCityName);
                return NotFound($"City '{cityName}' was not found.");
            }

            return Ok(temperature);
        }
    }
}
