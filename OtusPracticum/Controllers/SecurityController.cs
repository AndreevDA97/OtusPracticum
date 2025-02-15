using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OtusPracticum.Helpers;
using OtusPracticum.Models;
using OtusPracticum.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OtusPracticum.Controllers
{
    [ApiController]
    [Route("api/security")]
    public class SecurityController(
        IConfiguration configuration,
        UserService userService
        ) : ControllerBase
    {
        [HttpPost, Route("login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await userService.GetUserAsync(request.Id);

            if (user is null)
                return BadRequest("Bad credentials");

            var verified = PasswordHelper.Check(user.Password, request.Password);
            if (!verified)
            {
                return BadRequest("Bad credentials");
            }

            var claims = new ClaimsIdentity();
            claims.AddClaim(new(ClaimTypes.NameIdentifier, user.User_id.ToString()));
            claims.AddClaim(new(ClaimTypes.Name, user.First_name));

            var expire = TimeSpan.FromMinutes(
                int.Parse(configuration["JwtSettings:TokenExpiryMinutes"]!));
            var secret = configuration["JwtSettings:SecretKey"]!;
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claims,
                Expires = DateTime.UtcNow + expire,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    SecurityAlgorithms.HmacSha256),
                Audience = configuration["JwtSettings:Audience"],
                Issuer = configuration["JwtSettings:Issuer"]
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var access_token = tokenHandler.WriteToken(token);

            return Ok(new LoginResponse { Access_token = access_token, ExpiresIn = (int)expire.TotalMinutes });
        }
    }
}
