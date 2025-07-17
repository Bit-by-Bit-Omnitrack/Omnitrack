using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using UserRoles.Data;
using UserRoles.Models;

namespace UserRoles.Controllers
{
    public class TasksController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public TasksController(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Tasks
        public async Task<IActionResult> Index()
        {
            var tasks = await _context.Tasks
                .Include(t => t.Ticket)
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .ToListAsync();

            return View(tasks);
        }

        // GET: Tasks/Create?ticketId=5
        public IActionResult Create(int ticketId)
        {
            var task = new Tasks
            {
                TicketId = ticketId
            };

            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName");
            return View(task);
        }

        // POST: Tasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tasks task)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                task.CreatedById = userId;

                _context.Tasks.Add(task);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Tickets1", new { id = task.TicketId });
            }

            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName", task.UserId);
            return View(task);
        }

        // GET: Tasks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName", task.UserId);
            return View(task);
        }

        // POST: Tasks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tasks task)
        {
            if (id != task.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(task);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", "Tickets1", new { id = task.TicketId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Tasks.Any(t => t.Id == id))
                        return NotFound();
                    else
                        throw;
                }
            }

            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName", task.UserId);
            return View(task);
        }

        // GET: Tasks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var task = await _context.Tasks
                .Include(t => t.Ticket)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (task == null) return NotFound();

            return View(task);
        }

        // POST: Tasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task != null)
            {
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Tickets1", new { id = task.TicketId });
            }

            return NotFound();
        }
    }
}
