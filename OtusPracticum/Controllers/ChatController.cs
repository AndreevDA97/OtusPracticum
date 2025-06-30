using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OtusPracticum.Entities;
using OtusPracticum.Models;
using OtusPracticum.Services;
using System.Security.Claims;

namespace OtusPracticum.Controllers
{
    [ApiController]
    [Route("api/dialog"), Authorize]
    public class ChatController(ChatService chatService) : ControllerBase
    {
        private readonly ChatService chatService = chatService;

        [HttpGet, Route("{chat_id}/messages")]
        public async Task<ActionResult<MessageEntity[]>> GetChatAsync(Guid chat_id, int limit = 1000, int offset = 0)
        {
            var currentUserId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            return Ok(await chatService.GetChatAsync(chat_id, limit, offset, currentUserId));
        }

        [HttpGet, Route("list")]
        public async Task<ActionResult<Chat[]>> GetUserChatListAsync(int offset = 0, int limit = 200)
        {
            var currentUserId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            return Ok(await chatService.GetUserChatListAsync(currentUserId, limit, offset));
        }

        [HttpPost, Route("{chat_id}/send")]
        public async Task<ActionResult<Guid>> SendMessageToChatAsync([FromRoute] Guid chat_id, [FromBody] SendMessageRequest request)
        {
            var currentUserId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            return Ok(await chatService.SendMessageToChatAsync(chat_id, request, currentUserId));
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> CreateChatAsync(CreateChatRequest request)
        {
            var currentUserId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            return Ok(await chatService.CreateChatAsync(request, currentUserId));
        }
    }
}
