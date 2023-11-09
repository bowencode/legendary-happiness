using Demo.Notes.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Demo.Notes.Web.Host.Pages
{
    [Authorize("User")]
    public class NoteDetailModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public NoteDetailModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("userApi");
        }

        public async Task OnGet(string id)
        {
            Note = await _httpClient.GetFromJsonAsync<NoteData>($"api/note/{id}");
        }

        public NoteData? Note { get; set; }
    }
}
