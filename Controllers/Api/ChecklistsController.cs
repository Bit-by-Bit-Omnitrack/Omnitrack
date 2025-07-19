using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserRoles.Models;
using UserRoles.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;


namespace UserRoles.Controllers.Api
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("Api/[controller]")]
    public class ChecklistsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ChecklistsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Checklists
        /// <summary>Get all checklists</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Checklists>>> GetChecklists()
        { 
            return await _context.Checklists.ToListAsync();
        }

        // GET: api/Checklists/5
        /// <summary>Get a checklist by ID</summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Checklists>> GetChecklist(int id)
        {
            var checklist = await _context.Checklists.FindAsync(id);

            if (checklist == null)
                return NotFound();

            return checklist;
        }

        // POST: api/Checklists
        /// <summary>Create a new checklist</summary>
        [HttpPost]
        public async Task<ActionResult<Checklists>> CreateChecklist(Checklists checklist)
        {
            _context.Checklists.Add(checklist);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetChecklist), new { id = checklist.Id }, checklist);
        }

        // PUT: api/Checklists/5
        /// <summary>Update a checklist by ID</summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateChecklist(int id, Checklists checklist)
        {
            if (id != checklist.Id)
                return BadRequest();

            _context.Entry(checklist).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ChecklistExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/Checklists/5
        /// <summary>Delete a checklist by ID</summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChecklist(int id)
        {
            var checklist = await _context.Checklists.FindAsync(id);
            if (checklist == null)
                return NotFound();

            _context.Checklists.Remove(checklist);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ChecklistExists(int id)
        {
            return _context.Checklists.Any(e => e.Id == id);
        }
    }
}
