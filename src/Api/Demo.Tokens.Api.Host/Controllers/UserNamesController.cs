using Demo.Tokens.Common.Extensions;
using Demo.Tokens.Common.Model;
using Demo.Tokens.Web.AdminApi.Host.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Tokens.Web.AdminApi.Host.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize("ReadUsers")]
    public class UserNamesController : ControllerBase
    {
        private readonly ILogger _logger;

        public UserNamesController(ILogger<UserNamesController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetUserNames")]
        public IEnumerable<UserSummary> Get()
        {
            var currentUserId = User.GetUserId();
            _logger.LogInformation("User {userId} accessing user name list", currentUserId);

            return TestUsers.Users.Where(u => u.IsActive).Select(u => new UserSummary
            {
                Id = u.SubjectId,
                Username = u.Username,
            });
        }

    }
}