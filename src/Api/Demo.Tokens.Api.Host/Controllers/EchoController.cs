using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Tokens.Api.Host.Controllers;
[ApiController]
[Authorize]
public class EchoController : ControllerBase
{
    [HttpGet("api/echo/{message?}")]
    public IActionResult Get(string? message = null)
    {
        var claimsList = User.Claims.Select(c => $"{c.Type} = {c.Value}").ToList();

        return Ok(new
        {
            Message = message ?? "Hello",
            Timestamp = DateTime.UtcNow,
            Authenticated = User.Identity?.IsAuthenticated ?? false,
            Claims = claimsList
        });
    }
}
