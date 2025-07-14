using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserRoles.Data;
using UserRoles.Models;
using UserRoles.Data;


namespace UserRoles.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketStatusesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TicketStatusesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/TicketStatuses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketStatus>>> GetTicketStatuses()
        {
            return await _context.TicketStatuses.ToListAsync();
        }

        // GET: api/TicketStatuses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TicketStatus>> GetTicketStatus(int id)
        {
            var status = await _context.TicketStatuses.FindAsync(id);

            if (status == null)
                return NotFound();

            return status;
        }

        // POST: api/TicketStatuses
        [HttpPost]
        public async Task<ActionResult<TicketStatus>> CreateTicketStatus(TicketStatus status)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.TicketStatuses.Add(status);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTicketStatus), new { id = status.Id }, status);
        }

        // PUT: api/TicketStatuses/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTicketStatus(int id, TicketStatus status)
        {
            if (id != status.Id)
                return BadRequest();

            _context.Entry(status).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TicketStatusExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/TicketStatuses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicketStatus(int id)
        {
            var status = await _context.TicketStatuses.FindAsync(id);
            if (status == null)
                return NotFound();

            _context.TicketStatuses.Remove(status);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TicketStatusExists(int id)
        {
            return _context.TicketStatuses.Any(e => e.Id == id);
        }
    }
}