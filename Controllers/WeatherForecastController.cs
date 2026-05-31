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

        [HttpGet("{summaryName}", Name = "GetSpecificWeatherByName")]
        public ActionResult<WeatherForecast> GetSpecificWeatherByName(string summaryName)
        {
            if (string.IsNullOrWhiteSpace(summaryName))
            {
                return BadRequest("Weather name is required.");
            }

            var normalizedSummary = summaryName.Trim();
            var matchingSummary = Summaries.FirstOrDefault(summary =>
                summary.Equals(normalizedSummary, StringComparison.OrdinalIgnoreCase));

            if (matchingSummary is null)
            {
                return NotFound($"Weather '{summaryName}' was not found.");
            }

            var seed = normalizedSummary.ToLowerInvariant().GetHashCode();
            var random = new Random(seed);

            _logger.LogInformation("Returning specific weather for summary: {Summary}", matchingSummary);

            var forecast = new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now),
                TemperatureC = random.Next(-20, 55),
                Summary = matchingSummary
            };

            return Ok(forecast);
        }
    }
}
