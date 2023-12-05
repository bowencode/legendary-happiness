using System.Globalization;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Demo.Tokens.Web.Host.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(ILogger<IndexModel> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostAsync()
        {
            var refreshToken = await HttpContext.GetTokenAsync("refresh_token");
            if (refreshToken == null)
            {
                throw new Exception("No refresh token.");
            }

            var client = _httpClientFactory.CreateClient();
            var response = await client.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = "https://localhost:5001/connect/token",
                ClientId = "web-ui",
                ClientSecret = "1f668bf6-5ef5-4e77-ae84-28614dfc9d2d",
                RefreshToken = refreshToken
            });
            if (response.IsError)
            {
                throw new Exception(response.Error);
            }

            var authInfo = await HttpContext.AuthenticateAsync();

            authInfo.Properties?.UpdateTokenValue("access_token", response.AccessToken!);
            authInfo.Properties?.UpdateTokenValue("refresh_token", response.RefreshToken!);
            authInfo.Properties?.UpdateTokenValue("expires_at", DateTime.UtcNow.AddSeconds(response.ExpiresIn).ToString("o", CultureInfo.InvariantCulture));

            await HttpContext.SignInAsync(authInfo.Principal!, authInfo.Properties);

            return RedirectToPage("/Index");
        }
    }
}