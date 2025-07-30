using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using UserRoles.Data;
using UserRoles.Models;

namespace UserRoles.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProjectController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all projects with optional search and status filter.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProjects([FromQuery] string searchString, [FromQuery] string statusFilter)
        {
            var projects = _context.Projects.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
                projects = projects.Where(p => p.ProjectName.Contains(searchString));

            if (!string.IsNullOrEmpty(statusFilter))
            {
                if (statusFilter == "Active")
                    projects = projects.Where(p => p.IsActive);
                else if (statusFilter == "Inactive")
                    projects = projects.Where(p => !p.IsActive);
            }

            return Ok(await projects.ToListAsync());
        }

        /// <summary>
        /// Get project details by ID (including members).
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProject(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Members)
                    .ThenInclude(pm => pm.User)
                .FirstOrDefaultAsync(p => p.ProjectId == id);

            if (project == null)
                return NotFound();

            return Ok(project);
        }

        /// <summary>
        /// Create a new project.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] Project project)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Add(project);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProject), new { id = project.ProjectId }, project);
        }

        /// <summary>
        /// Update an existing project.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> EditProject(int id, [FromBody] Project project)
        {
            if (id != project.ProjectId)
                return BadRequest();

            _context.Entry(project).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Projects.Any(e => e.ProjectId == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Delete a project by ID.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return NotFound();

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Assign a user to a project with a role.
        /// </summary>
        [HttpPost("{projectId}/assign")]
        public async Task<IActionResult> AssignMember(int projectId, [FromQuery] string userId, [FromQuery] string role)
        {
            var existing = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);

            if (existing != null)
                return BadRequest("User already assigned to this project.");

            var member = new ProjectMember
            {
                ProjectId = projectId,
                UserId = userId,
                ProjectRole = role
            };

            _context.ProjectMembers.Add(member);
            await _context.SaveChangesAsync();

            return Ok(member);
        }

        /// <summary>
        /// Remove a user from a project.
        /// </summary>
        [HttpDelete("{projectId}/remove")]
        public async Task<IActionResult> RemoveMember(int projectId, [FromQuery] string userId)
        {
            var member = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);

            if (member == null)
                return NotFound();

            _context.ProjectMembers.Remove(member);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Edit a project member's role.
        /// </summary>
        [HttpPut("{projectId}/edit-role")]
        public async Task<IActionResult> EditMemberRole(int projectId, [FromQuery] string userId, [FromQuery] string newRole)
        {
            if (string.IsNullOrWhiteSpace(newRole))
                return BadRequest("Role cannot be empty.");

            var member = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);

            if (member == null)
                return NotFound();

            member.ProjectRole = newRole;
            await _context.SaveChangesAsync();

            return Ok(member);
        }
    }
}
