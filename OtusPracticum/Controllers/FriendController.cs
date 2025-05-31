using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OtusPracticum.Services;
using System.Security.Claims;

namespace OtusPracticum.Controllers
{
    [ApiController]
    [Route("api/friend")]
    public class FriendController(FriendService friendService) : ControllerBase
    {
        private readonly FriendService friendService = friendService;

        [HttpPut, Route("set/{friend_id}"), Authorize]
        public async Task<IActionResult> AddFriend(Guid friend_id)
        {
            var currentUserId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            if (currentUserId == friend_id)
            {
                return BadRequest();
            }
            await friendService.AddFriendAsync(currentUserId, friend_id);
            return Ok();
        }

        [HttpPut, Route("delete/{friend_id}"), Authorize]
        public async Task<IActionResult> DeleteFriend(Guid friend_id)
        {
            var currentUserId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            if (currentUserId == friend_id)
            {
                return BadRequest();
            }
            await friendService.DeleteFriendAsync(currentUserId, friend_id);
            return Ok();
        }
    }
}
