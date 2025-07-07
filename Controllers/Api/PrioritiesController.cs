using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserRoles.Models;

namespace UserRoles.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrioritiesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PrioritiesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Priorities
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Priority>>> GetPriorities()
        {
            return await _context.Priorities.ToListAsync();
        }

        // GET: api/Priorities/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Priority>> GetPriority(int id)
        {
            var priority = await _context.Priorities.FindAsync(id);

            if (priority == null)
                return NotFound();

            return priority;
        }

        // POST: api/Priorities
        [HttpPost]
        public async Task<ActionResult<Priority>> CreatePriority(Priority priority)
        {
            _context.Priorities.Add(priority);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPriority), new { id = priority.Id }, priority);
        }

        // PUT: api/Priorities/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePriority(int id, Priority priority)
        {
            if (id != priority.Id)
                return BadRequest();

            _context.Entry(priority).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PriorityExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/Priorities/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePriority(int id)
        {
            var priority = await _context.Priorities.FindAsync(id);
            if (priority == null)
                return NotFound();

            _context.Priorities.Remove(priority);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PriorityExists(int id) => _context.Priorities.Any(e => e.Id == id);
    }
}