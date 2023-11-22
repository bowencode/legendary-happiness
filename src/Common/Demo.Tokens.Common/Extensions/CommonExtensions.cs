using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;

namespace Demo.Tokens.Common.Extensions;

public static class CommonExtensions
{
    public static string GetDisplayName(this ClaimsPrincipal claimsPrincipal)
    {
        var claim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier) ?? claimsPrincipal.FindFirst("name");

        if (claim != null)
        {
            var rawValue = claim.Value;
            try
            {
                var items = JsonSerializer.Deserialize<List<string>>(rawValue);
                if (items != null && items.Any())
                {
                    return items.First();
                }
            }
            catch
            {
                // not valid json
            }

            return rawValue;
        }

        return "Unknown";
    }

    public static string GetUserId(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.FindFirst("sub")?.Value;
    }
}