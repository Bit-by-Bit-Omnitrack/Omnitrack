using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UserRoles.Models;

namespace UserRoles.Controllers
{
    public class Tickets1Controller : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public Tickets1Controller(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Tickets1
        public async Task<IActionResult> Index()
        {
            

            return View(await _context.Tickets.ToListAsync());

        }

        // GET: Tickets1/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
              
                return NotFound();
            }

            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets1/Create
        public IActionResult Create()
        {
        
            return View();
        }

        // POST: Tickets1/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Create([Bind("Title,Description,AssignedToUserId,DueDate")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                // Auto-generate TicketID and TaskID
                ticket.TicketID = $"TCK-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
                ticket.TaskID = $"TSK-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

                // Default status: 1 = To Do
                ticket.StatusID = 1;

                // Set CreatedByID to current logged-in username
                ticket.CreatedByID = User.Identity?.Name ?? "System";

                // Set CreatedDate to now
                ticket.CreatedDate = DateTime.UtcNow;

                _context.Add(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Users = new SelectList(await _userManager.Users.Where(u => u.IsActive).ToListAsync(), "Id", "UserName");
            return View(ticket);
        }


        // GET: Tickets1/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            ViewBag.Users = new SelectList(await _userManager.Users.Where(u => u.IsActive).ToListAsync(), "Id", "UserName", ticket.AssignedToUserId);
            return View(ticket);
        }

        // POST: Tickets1/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TicketID,Title,Description,StatusID,TaskID,CreatedByID,CreatedDate,DueDate,UpdatedBy,UpdatedDate")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(ticket);
        }

        // GET: Tickets1/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets1/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
      
        private bool TicketExists(int id)
        {
            return _context.Tickets.Any(e => e.Id == id);
        }
    }
}
