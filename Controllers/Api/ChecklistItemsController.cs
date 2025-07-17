using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserRoles.Data;
using UserRoles.Models;

namespace UserRoles.Controllers.Api
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ChecklistItemsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ChecklistItemsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ChecklistItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChecklistItem>>> GetChecklistItems()
        {
            return await _context.ChecklistItems
                                 .Include(c => c.Checklists)
                                 .ToListAsync();
        }

        // GET: api/ChecklistItems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ChecklistItem>> GetChecklistItem(int id)
        {
            var item = await _context.ChecklistItems.FindAsync(id);

            if (item == null)
                return NotFound();

            return item;
        }

        // POST: api/ChecklistItems
        [HttpPost]
        public async Task<ActionResult<ChecklistItem>> CreateChecklistItem(ChecklistItem item)
        {
            _context.ChecklistItems.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetChecklistItem), new { id = item.Id }, item);
        }

        // PUT: api/ChecklistItems/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateChecklistItem(int id, ChecklistItem item)
        {
            if (id != item.Id)
                return BadRequest();

            _context.Entry(item).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ChecklistItemExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/ChecklistItems/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChecklistItem(int id)
        {
            var item = await _context.ChecklistItems.FindAsync(id);
            if (item == null)
                return NotFound();

            _context.ChecklistItems.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ChecklistItemExists(int id)
        {
            return _context.ChecklistItems.Any(e => e.Id == id);
        }
    }
}