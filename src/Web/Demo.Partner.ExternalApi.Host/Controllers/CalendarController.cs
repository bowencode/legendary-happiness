using Demo.Tokens.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Partner.ExternalApi.Host.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize("ReadCalendar")]
    public class CalendarController : ControllerBase
    {
        private static readonly string[] EventTypes = new[]
        {
            "Meeting", "Flight", "Appointment", "Standup Meeting", "Party", "Game"
        };

        private readonly ILogger<CalendarController> _logger;

        public CalendarController(ILogger<CalendarController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetCalendarItems")]
        public IEnumerable<CalendarEvent> Get()
        {
            for (int i = 0; i < 10; i++)
            {
                yield return new CalendarEvent
                {
                    Date = DateTime.UtcNow.AddDays(Random.Shared.Next(1, 30)),
                    Description = EventTypes[Random.Shared.Next(EventTypes.Length)]
                };
            }
        }
    }
}