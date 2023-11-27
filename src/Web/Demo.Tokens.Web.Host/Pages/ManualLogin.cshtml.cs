using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Demo.Tokens.Web.Host.Pages
{
    public class ManualLoginModel : PageModel
    {
        [BindProperty]
        public string? Username { get; set; }
        [BindProperty]
        public string? Password { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost()
        {
            if (Username == null)
            {
                ModelState.AddModelError("username", "Username is required");
                return Page();
            }

            var client = new HttpClient();
            var response = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = "https://localhost:5001/connect/token",
                ClientId = "password-login",
                ClientSecret = "84c4d8ef-2fe6-4acc-8cf2-eb15b51fba0d",
                UserName = Username,
                Password = Password,
                Scope = "openid profile offline_access api1 api2"
            });

            if (response.IsError)
            {
                ModelState.AddModelError("error", response.Error ?? "Login failed");
                return Page();
            }

            var userInformation = await client.GetUserInfoAsync(new UserInfoRequest
            {
                Address = "https://localhost:5001/connect/userinfo",
                Token = response.AccessToken
            });
            var claims = new List<Claim>();

            claims.AddRange(userInformation.Claims);

            var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "password", JwtClaimTypes.Name, JwtClaimTypes.Role));

            var properties = new AuthenticationProperties();
            List<AuthenticationToken> tokens = new() { new AuthenticationToken { Name = "expires_at", Value = DateTime.UtcNow.AddSeconds(response.ExpiresIn).ToString("o") } };
            if (response.AccessToken != null) tokens.Add(new AuthenticationToken{ Name ="access_token", Value = response.AccessToken });
            if (response.RefreshToken != null) tokens.Add(new AuthenticationToken { Name ="refresh_token", Value = response.RefreshToken });
            if (response.IdentityToken != null) tokens.Add(new AuthenticationToken { Name ="id_token", Value = response.IdentityToken });
            properties.StoreTokens(tokens);

            await HttpContext.SignInAsync(user, properties);

            return RedirectToPage("Index");
        }
    }
}
