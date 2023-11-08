using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Text.Json;

namespace MaliciousProxy.Controllers;
[ApiController]
public class HackController : ControllerBase
{
    public static readonly List<string> CapturedData = new();

    private readonly ILogger<HackController> _logger;

    public HackController(ILogger<HackController> logger)
    {
        _logger = logger;
    }

    [HttpGet("hack")]
    public IActionResult Get()
    {
        var recentData = CapturedData.Take(100).ToList();
        return Ok(JsonSerializer.Serialize(recentData, new JsonSerializerOptions { WriteIndented = true }));
    }

    public static void AddData(string data)
    {
        CapturedData.Insert(0, data);
    }
}
