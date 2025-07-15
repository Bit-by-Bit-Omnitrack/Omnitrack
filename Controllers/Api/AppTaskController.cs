using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserRoles.Data;
using UserRoles.Models;

namespace UserRoles.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppTaskController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AppTaskController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all tasks.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppTask>>> GetAllTasks()
        {
            return await _context.AppTask.ToListAsync();
        }

        /// <summary>
        /// Get a task by ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<AppTask>> GetTask(int id)
        {
            var task = await _context.AppTask.FindAsync(id);
            if (task == null)
                return NotFound();

            return task;
        }

        /// <summary>
        /// Create a new task.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<AppTask>> CreateTask(AppTask task)
        {
            _context.AppTask.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        /// <summary>
        /// Update an existing task.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, AppTask task)
        {
            if (id != task.Id)
                return BadRequest("Task ID mismatch.");

            _context.Entry(task).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.AppTask.Any(t => t.Id == id))
                    return NotFound();

                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Delete a task by ID.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.AppTask.FindAsync(id);
            if (task == null)
                return NotFound();

            _context.AppTask.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
