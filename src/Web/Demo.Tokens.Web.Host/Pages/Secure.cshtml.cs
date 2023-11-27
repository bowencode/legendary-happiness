using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Demo.Tokens.Web.Host.Pages
{
    [Authorize]
    public class SecureModel : PageModel
    {
        public async Task OnGet()
        {
            var client = new HttpClient();
            string? token = await HttpContext.GetTokenAsync("access_token");
            if (token != null)
                client.SetBearerToken(token);

            var response = await client.GetAsync("https://localhost:7274/api/echo");
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Unable to call API: " + response.StatusCode);
            }

            var content = await response.Content.ReadAsStringAsync();
            ViewData["ApiResponse"] = content;
        }
    }
}
