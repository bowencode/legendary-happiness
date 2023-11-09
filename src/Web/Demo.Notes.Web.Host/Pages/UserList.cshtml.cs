using Demo.Notes.Common.Configuration;
using Demo.Notes.Common.Model;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Demo.Notes.Web.Host.Pages
{
    [Authorize("Admin")]
    public class UserListModel : PageModel
    {
        private readonly AdminApiOptions _options;

        public UserListModel(IOptions<AdminApiOptions> options)
        {
            _options = options.Value;
        }

        public List<UserSummary> Users { get; set; } = new List<UserSummary>();
        public string? Error { get; set; }

        public async Task OnGet()
        {
            var tokenClient = new HttpClient
            {
                BaseAddress = new Uri(_options.Identity.Authority)
            };
            var formValues = new Dictionary<string, string>
            {
                { "client_id", _options.Identity.ClientId },
                { "client_secret", _options.Identity.ClientSecret },
                { "grant_type", "client_credentials" },
            };
            foreach (string scope in _options.Identity.Scopes)
            {
                formValues.Add("scope", scope);
            }
            var response = await tokenClient.PostAsync("connect/token", new FormUrlEncodedContent(formValues));
            if (!response.IsSuccessStatusCode)
            {
                Error = response.ReasonPhrase;
                return;
            }

            var tokenJson = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<BasicTokenResponse>(tokenJson);
            var token = tokenResponse?.AccessToken;

            var dataClient = new HttpClient
            {
                BaseAddress = new Uri(_options.Host)
            };
            dataClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var userSummaries = await dataClient.GetFromJsonAsync<List<UserSummary>>("UserNames");
            if (userSummaries != null)
            {
                Users = userSummaries;
            }
        }
    }
}
