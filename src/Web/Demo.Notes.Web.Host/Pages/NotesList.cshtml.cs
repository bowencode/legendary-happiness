using Demo.Notes.Common.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Demo.Notes.Web.Host.Pages
{
    [Authorize("User")]
    public class NotesListModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public NotesListModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("userApi");
        }

        public async Task OnGet()
        {
            // this is how to access the User token
            var token = await HttpContext.GetTokenAsync("access_token");

            var list = await _httpClient.GetFromJsonAsync<List<NoteData>>("api/note");
            if (list != null)
                AllNotes = list;
        }

        public List<NoteData> AllNotes { get; set; } = new List<NoteData>();
    }
}
