using Demo.Tokens.Common.Model;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Demo.Tokens.Web.Host.Pages
{
    [Authorize("Admin")]
    public class UserListModel : PageModel
    {
        public List<UserData> Users { get; set; } = new();
        public string? Error { get; set; }

        public async Task OnGet()
        {
            var client = new HttpClient();
            string? token = await HttpContext.GetTokenAsync("access_token");
            if (token != null)
                client.SetBearerToken(token);

            var userList = await client.GetFromJsonAsync<List<UserData>>("https://localhost:7274/api/users");
            Users = userList ?? new List<UserData>();
        }
    }
}
