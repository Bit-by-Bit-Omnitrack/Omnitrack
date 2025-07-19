using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserRoles.Models;
using UserRoles.ViewModels;

namespace UserRoles.Controllers.Api
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class UserApiController : ControllerBase
    {
        private readonly UserManager<Users> _userManager;

        public UserApiController(UserManager<Users> userManager)
        {
            _userManager = userManager;
        }

        // GET: api/UserApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserWithRolesViewModel>>> GetActiveUsers()
        {
            var users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
            var userRoles = new List<UserWithRolesViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles.Add(new UserWithRolesViewModel
                {
                    User = user,
                    Roles = roles
                });
            }

            return Ok(userRoles);
        }

        // GET: api/UserApi/pending
        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<Users>>> GetPendingUsers()
        {
            var pendingUsers = await _userManager.Users.Where(u => !u.IsActive).ToListAsync();
            return Ok(pendingUsers);
        }

        // GET: api/UserApi/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Users>> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || !user.IsActive)
                return NotFound();

            return Ok(user);
        }

        // PUT: api/UserApi/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] Users model)
        {
            if (id != model.Id)
                return BadRequest();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            user.UserName = model.UserName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return NoContent();
        }

        // DELETE: api/UserApi/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            user.IsActive = false;
            await _userManager.UpdateAsync(user);

            return NoContent();
        }

        // POST: api/UserApi/approve/{id}
        [HttpPost("approve/{id}")]
        public async Task<IActionResult> ApproveUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            user.IsActive = true;
            await _userManager.UpdateAsync(user);

            return Ok(new { message = "User approved successfully." });
        }

        // DELETE: api/UserApi/reject/{id}
        [HttpDelete("reject/{id}")]
        public async Task<IActionResult> RejectUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest("Failed to reject user.");

            return Ok("User rejected and deleted.");
        }
    }
}
