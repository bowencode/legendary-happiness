using Demo.Tokens.Common.Configuration;
using Demo.Tokens.Common.Extensions;
using Demo.Tokens.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace Demo.Tokens.Api.Host.Controllers
{
    [ApiController]
    [Authorize("ReadNotes")]
    public class NoteController : ControllerBase
    {
        private readonly ILogger<NoteController> _logger;

        private string? CurrentUserId => User.GetUserId();

        public NoteController(ILogger<NoteController> logger)
        {
            _logger = logger;
        }

        [EnableCors("ClientSideSPA")]
        [HttpGet("api/note/")]
        public async Task<IActionResult> Get()
        {
            if (CurrentUserId == null)
                return BadRequest();

            var list = Enumerable.Range(1, 10).ToList().Select(i =>
            {
                var note = new NoteData
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = CurrentUserId,
                    Username = User.Identity?.Name,
                    Updated = DateTime.UtcNow,
                    Text = $"This is the content for note {i} for {User.Identity?.Name}"
                };
                return note;
            });

            return Ok(list);
        }
    }
}