using Demo.Notes.Common.Configuration;
using Demo.Notes.Common.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Demo.Notes.Common.Extensions;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authorization;

namespace Demo.Notes.Web.UserApi.Host.Controllers
{
    [ApiController]
    [Authorize("ReadNotes")]
    public class NoteController : ControllerBase
    {
        private readonly ILogger<NoteController> _logger;
        private readonly HttpClient _httpClient;
        private readonly CosmosOptions _dbOptions;

        private string? CurrentUserId => User.GetUserId();

        public NoteController(ILogger<NoteController> logger, IHttpClientFactory httpClientFactory, IOptions<CosmosOptions> dbOptions)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("adminApiClient");
            _dbOptions = dbOptions.Value;
        }

        [EnableCors("ClientSideSPA")]
        [HttpGet("api/note/")]
        public async Task<IActionResult> Get()
        {
            var container = await NotesDatabase.GetNotesContainerAsync(_dbOptions);

            if (CurrentUserId == null)
                return BadRequest();

            var allNotes = container.GetItemLinqQueryable<NoteData>(
                    allowSynchronousQueryExecution: true)
                .Where(n => n.UserId == CurrentUserId)
                .ToList();

            return Ok(allNotes);
        }

        [HttpGet("api/note/{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var container = await NotesDatabase.GetNotesContainerAsync(_dbOptions);
            var note = container
                .GetItemLinqQueryable<NoteData>(
                    allowSynchronousQueryExecution: true,
                    requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(id) })
                .Where(n => n.UserId == CurrentUserId && n.Id == id)
                .AsEnumerable()
                .FirstOrDefault();

            if (note == null)
            {
                return BadRequest();
            }
            
            return Ok(note);
        }

        [HttpPost("api/note/")]
        [Authorize("WriteNotes")]
        public async Task<IActionResult> Post(NoteData note)
        {
            if (string.IsNullOrWhiteSpace(note.UserId))
            {
                return BadRequest();
            }

            string username = await _httpClient.GetStringAsync($"Username/?id={CurrentUserId}");

            note.Id = Guid.NewGuid().ToString();
            note.UserId = CurrentUserId;
            note.Username = username;
            note.Updated = DateTime.UtcNow;

            var container = await NotesDatabase.GetNotesContainerAsync(_dbOptions);
            NoteData created = await container.CreateItemAsync(note);
            if (created == null)
            {
                return BadRequest();
            }

            return Ok(created);
        }

        [HttpPut("api/note/{id}")]
        [Authorize("WriteNotes")]
        public async Task<IActionResult> Put(string id, NoteData note)
        {
            if (string.IsNullOrWhiteSpace(note.UserId) || id != note.Id || note.UserId != CurrentUserId)
            {
                return BadRequest();
            }

            note.Updated = DateTime.UtcNow;

            var container = await NotesDatabase.GetNotesContainerAsync(_dbOptions);
            NoteData updated = await container.UpsertItemAsync(note);
            if (updated == null)
            {
                return BadRequest();
            }

            return Ok(updated);
        }

        [HttpDelete("api/note/{id}")]
        [Authorize("WriteNotes")]
        public async Task<IActionResult> Delete(string id)
        {
            var container = await NotesDatabase.GetNotesContainerAsync(_dbOptions);
            var note = container
                .GetItemLinqQueryable<NoteData>(
                    allowSynchronousQueryExecution: true,
                    requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(id) })
                .Where(n => n.UserId == CurrentUserId && n.Id == id)
                .AsEnumerable()
                .FirstOrDefault();

            if (note == null)
            {
                return NotFound();
            }

            var response = await container.DeleteItemAsync<NoteData>(id, new PartitionKey(id));
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return NoContent();
            }

            return NotFound();
        }

    }
}