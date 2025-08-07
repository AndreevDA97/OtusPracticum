using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OtusPracticum.Entities;
using OtusPracticum.Models;
using OtusPracticum.Services;
using System.Security.Claims;

namespace OtusPracticum.Controllers
{
    [ApiController]
    [Route("api/post")]
    public class PostController(PostService postService) : ControllerBase
    {
        private readonly PostService postService = postService;

        [HttpGet, Route("get/{post_id}")]
        public async Task<ActionResult<Post>> GetPost(Guid post_id)
        {
            var post = await postService.GetPostAsync(post_id);
            return post is null
                ? NotFound()
                : Ok(post);
        }

        [HttpGet, Route("feed"), Authorize]
        public async Task<ActionResult<Post[]>> GetFeed(int offset = 0, int limit = 50)
        {
            var currentUserId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            var posts = await postService.GetFeedAsync(currentUserId, offset, limit);
            return Ok(posts);
        }

        [HttpPost, Route("create"), Authorize]
        public async Task<ActionResult<Guid>> AddPost([FromBody] AddPostRequest request)
        {
            var currentUserId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            var guid = await postService.AddPostAsync(currentUserId, request.Text);
            return Ok(guid);
        }

        [HttpDelete, Route("delete/{post_id}"), Authorize]
        public async Task<IActionResult> DeletePost(Guid post_id)
        {
            var currentUserId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            await postService.DeletePostAsync(post_id, currentUserId);
            return Ok();
        }

        [HttpPut, Route("update/{post_id}"), Authorize]
        public async Task<IActionResult> UpdatePost(Guid post_id, [FromBody] AddPostRequest request)
        {
            var currentUserId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            await postService.UpdatePostAsync(post_id, request.Text, currentUserId);
            return Ok();
        }
    }
}
