using Demo.Tokens.Common.Extensions;
using Demo.Tokens.Common.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Demo.Tokens.Web.Host.Pages
{
    public class AddNoteModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public AddNoteModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("userApi");
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostSubmit(NoteModel note)
        {
            if (note.NoteText == null)
                return BadRequest();

            var data = new NoteData
            {
                Text = note.NoteText,
                UserId = User.GetUserId()
            };

            var responseMessage = await _httpClient.PostAsJsonAsync<NoteData>($"api/note/", data);
            if (responseMessage.IsSuccessStatusCode)
            {
                return Redirect("/NotesList");
            }

            return BadRequest();
        }

    }

    public class NoteModel
    {
        [BindProperty]
        public string? NoteText { get; set; }
    }
}
