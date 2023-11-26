using Demo.Tokens.Api.Host.Model;
using Demo.Tokens.Common.Configuration;
using Demo.Tokens.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace Demo.Tokens.Api.Host.Controllers
{
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly ILogger<MessagesController> _logger;

        public MessagesController(ILogger<MessagesController> logger)
        {
            _logger = logger;
        }

        [HttpGet("api/messages")]
        [Authorize("AllMessages")]
        public IActionResult GetAll()
        {
            var messages = GetMessages(MessageData.AllMessages);

            return Ok(messages);
        }

        private static List<UserMessage> GetMessages(IEnumerable<MessageItem> messageData)
        {
            var messages = messageData.Select(m => new UserMessage
            {
                Id = m.Id,
                Content = m.Content,
                Sent = m.Sent,
                From = TestUsers.Users.FirstOrDefault(u => u.SubjectId == m.From.ToString())?.Username,
                To = TestUsers.Users.FirstOrDefault(u => u.SubjectId == m.To.ToString())?.Username
            }).ToList();
            return messages;
        }

        [HttpGet("api/messages/{userId}/from")]
        [Authorize("SentMessages")]
        public IActionResult GetByFromUser(string userId)
        {
            var messages = GetMessages(MessageData.AllMessages.Where(m => m.From.ToString() == userId));

            return Ok(messages);
        }

        [HttpGet("api/messages/{userId}/to")]
        [Authorize("ReceivedMessages")]
        public IActionResult GetByToUser(string userId)
        {
            var messages = GetMessages(MessageData.AllMessages.Where(m => m.To.ToString() == userId));

            return Ok(messages);
        }
    }
}