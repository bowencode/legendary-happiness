using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Demo.Tokens.Web.Host.Pages
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            return SignOut("Cookies", "oidc");
        }
    }
}
