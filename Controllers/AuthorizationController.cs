using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserRoles.Services;

namespace UserRoles.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorizationController : Controller
    {
            private readonly JwtTokenService _jwtService;

            public AuthorizationController(JwtTokenService jwtService)
            {
                _jwtService = jwtService;
            }

            [AllowAnonymous]
            [HttpPost("login")]
            public IActionResult Login([FromBody] LoginModel login)
            {
                if (login.Username == "admin" && login.Password == "password") // Replace with real auth
                {
                    var token = _jwtService.GenerateToken(login.Username);
                    return Ok(new { Token = token });
                }

                return Unauthorized();
            }
        }

        public class LoginModel
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }

