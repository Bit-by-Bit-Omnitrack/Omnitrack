using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserRoles.Data;
using UserRoles.Models;
using Microsoft.AspNetCore.Authorization; // Required for [Authorize]
using Microsoft.AspNetCore.Authentication.JwtBearer; // Required if specifying scheme explicitly

namespace UserRoles.Controllers
{
    // Apply [Authorize] to the entire controller to protect all its endpoints.
    // This means any request to these endpoints will require a valid JWT.
    // We explicitly specify the JwtBearerDefaults.AuthenticationScheme to ensure it uses JWT authentication.
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RolesController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all roles.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Role>>> GetAllRoles()
        {
            return await _context.Roles.ToListAsync();
        }

        /// <summary>
        /// Get a specific role by ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Role>> GetRole(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
                return NotFound();

            return role;
        }

        /// <summary>
        /// Create a new role.
        /// </summary>
        // You might want to restrict this to specific roles, e.g., [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Role>> CreateRole(Role role)
        {
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRole), new { id = role.Id }, role);
        }

        /// <summary>
        /// Update an existing role.
        /// </summary>
        // You might want to restrict this to specific roles, e.g., [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, Role role)
        {
            if (id != role.Id)
                return BadRequest("ID mismatch");

            _context.Entry(role).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Roles.Any(e => e.Id == id))
                    return NotFound();

                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Delete a role by ID.
        /// </summary>
        // You might want to restrict this to specific roles, e.g., [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
                return NotFound();

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
