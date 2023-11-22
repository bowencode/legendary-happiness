using System.Net.Http.Json;
using Demo.Tokens.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace Demo.Tokens.Web.Blazor.Client.Pages
{
    [Authorize]
    public partial class UserList
    {
        protected override async Task OnInitializedAsync()
        {
            var response = await Http.GetAsync("api/admin/UserNames");
            
            var users = await response.Content.ReadFromJsonAsync<List<UserNotesSummary>>();
            if (users != null)
            {
                Users = users;

                foreach (var userSummary in Users)
                {
                    var noteResponse = await Http.GetAsync($"user/api/notes/{userSummary.Id}");
                    var notes = await noteResponse.Content.ReadFromJsonAsync<List<NoteData>>();
                    if (notes != null)
                    {
                        userSummary.Notes = notes;
                    }
                }
            }
        }

        public List<UserNotesSummary> Users { get; set; } = new List<UserNotesSummary>();

        [Inject]
        public HttpClient Http { get; set; } = null!;
    }
}