using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OtusPracticum.Models;
using OtusPracticum.Services;
using System.ComponentModel.DataAnnotations;

namespace OtusPracticum.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController(UserService userService) : ControllerBase
    {
        private readonly UserService userService = userService;

        [HttpPost, Route("register")]
        public async Task<ActionResult<UserRegisterResponse>> Register(UserRegisterRequest request)
        {
            return Ok(await userService.RegisterUserAsync(request));
        }

        [HttpGet, Route("get/{id}"), Authorize]
        public async Task<ActionResult<User>> GetUser(Guid id)
        {
            var user = await userService.GetUserAsync(id);
            if (user is null) return NotFound();
            return Ok(user);
        }

        [HttpGet, Route("search"), Authorize]
        public async Task<ActionResult<List<User>>> SearchUser([Required] string first_name, [Required] string second_name)
        {
            var users = await userService.SearchUserAsync(first_name, second_name);
            if (users is null) return NotFound();
            return Ok(users);
        }
    }
}
