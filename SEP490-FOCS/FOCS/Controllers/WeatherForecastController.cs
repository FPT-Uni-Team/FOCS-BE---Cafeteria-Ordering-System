using Microsoft.AspNetCore.Mvc;
using FOCS.Common.Exceptions;
using FOCS.Common.Interfaces;
using FOCS.Common.Utils;
using FOCS.Infrastructure.Identity.Common.UnitOfWorks;
using FOCS.Infrastructure.Identity.Identity.Model;
using Microsoft.AspNetCore.Authorization;
using FOCS.Common.Constants;

namespace FOCS.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : FocsController
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        private readonly IEmailHelper _emailHelper;
        private readonly IUnitOfWork _unitOfWork;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IEmailHelper emailHelper, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _emailHelper = emailHelper;
            _unitOfWork = unitOfWork;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public ActionResult<IEnumerable<WeatherForecast>> Get()
        {

            try
            {
                _logger.LogInformation("Getting weather data for user {UserId}", "user123");

                var weather = Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                })
            .ToArray();

                _logger.LogInformation("Weather data retrieved successfully: {@Weather}", weather);

                return Ok(weather);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting weather data");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("test-middleware")]
        [Authorize(Roles = Roles.User)]
        public int TestMiddleware(int a, int b)
        {
            ConditionCheck.CheckCondition(a > b, "a must > b");

            return a - b;
        }

        [HttpPost("test-unit-of-work")]
        public async Task<int> TestUnit()
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var userRepo = _unitOfWork.Repository<User>();

                var user = await userRepo.FindAsync(x => x.FirstName == "son");

                ConditionCheck.CheckCondition(user.Count() > 0, Errors.SystemError.UnhandledExceptionOccurred);

            }
            catch (Exception ex)
            {
                return -1;
            }

            return 0;
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
