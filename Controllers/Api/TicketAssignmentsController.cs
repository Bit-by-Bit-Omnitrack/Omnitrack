using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserRoles.Controllers.Api;
using UserRoles.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserRoles.Data;
using UserRoles.Models;

namespace UserRoles.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketAssignmentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TicketAssignmentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/TicketAssignments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketAssignment>>> GetTicketAssignments()
        {
            var data = await _context.TicketAssignments
                .Include(t => t.User)
                .Include(t => t.Ticket)
                .ToListAsync();
            return Ok(data);
        }

        // GET: api/TicketAssignments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TicketAssignment>> GetTicketAssignment(int id)
        {
            var ta = await _context.TicketAssignments
                .Include(t => t.User)
                .Include(t => t.Ticket)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ta == null)
                return NotFound();

            return Ok(ta);
        }

        // POST: api/TicketAssignments
        [HttpPost]
        public async Task<ActionResult<TicketAssignment>> CreateTicketAssignment(TicketAssignment ta)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.TicketAssignments.Add(ta);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTicketAssignment), new { id = ta.Id }, ta);
        }

        // PUT: api/TicketAssignments/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTicketAssignment(int id, TicketAssignment ta)
        {
            if (id != ta.Id)
                return BadRequest();

            _context.Entry(ta).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TicketAssignmentExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/TicketAssignments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicketAssignment(int id)
        {
            var ta = await _context.TicketAssignments.FindAsync(id);
            if (ta == null)
                return NotFound();

            _context.TicketAssignments.Remove(ta);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TicketAssignmentExists(int id)
        {
            return _context.TicketAssignments.Any(e => e.Id == id);
        }
    }
}

