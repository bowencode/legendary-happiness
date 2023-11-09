using Demo.Notes.Common.Configuration;
using Demo.Notes.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace Demo.Notes.Web.UserApi.Host.Controllers
{
    [ApiController]
    [Authorize("ListNotes")]
    public class NotesController : ControllerBase
    {
        private readonly ILogger<NotesController> _logger;
        private readonly CosmosOptions _options;
        private static CosmosLinqSerializerOptions SerializerOptions => new CosmosLinqSerializerOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase };

        public NotesController(ILogger<NotesController> logger, IOptions<CosmosOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        [HttpGet("api/notes/")]
        public async Task<IActionResult> Get()
        {
            var container = await NotesDatabase.GetNotesContainerAsync(_options);

            var allNotes = container
                .GetItemLinqQueryable<NoteData>(allowSynchronousQueryExecution: true)
                .ToList();

            return Ok(allNotes);
        }

        [HttpGet("api/notes/{userId}/")]
        public async Task<IActionResult> GetByUser(string userId)
        {
            var container = await NotesDatabase.GetNotesContainerAsync(_options);

            var allNotes = container.GetItemLinqQueryable<NoteData>(
                    allowSynchronousQueryExecution: true,
                    linqSerializerOptions: SerializerOptions)
                .Where(n => n.UserId == userId)
                .ToList();
            return Ok(allNotes);
        }
    }
}