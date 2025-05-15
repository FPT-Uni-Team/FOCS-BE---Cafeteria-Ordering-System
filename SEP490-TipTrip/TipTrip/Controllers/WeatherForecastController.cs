using Microsoft.AspNetCore.Mvc;
using TipTrip.Common.Interfaces;
using TipTrip.Common.Utils;

namespace TipTrip.Controllers
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

        private readonly IEmailHelper _emailHelper;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IEmailHelper emailHelper)
        {
            _logger = logger;
            _emailHelper = emailHelper;
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

        [HttpPost]
        public int TestMiddleware(int a, int b)
        {
            ConditionCheck.CheckCondition(a > b, "a must > b");

            return a - b;
        }

        [HttpPost("send-email")]
        public async Task<IActionResult> SendTestEmail(string toEmail)
        {
            try
            {
                string subject = "Welcome to TipTrip!";
                string body = "<h3>Hello from TipTrip 🚀</h3><p>This is a test email sent from the backend.</p>";

                await _emailHelper.SendEmailAsync(toEmail, subject, body);

                return Ok("Email sent successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email.");
                return StatusCode(500, "Failed to send email.");
            }
        }

    }
}
