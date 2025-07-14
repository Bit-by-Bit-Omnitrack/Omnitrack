using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UserRoles.Models;

namespace UserRoles.Controllers
{
    public class TickewtsController : Controller
    {
        private readonly AppDbContext _context;

        public TickewtsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Tickewts
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Tickets.Include(t => t.AssignedToUser).Include(t => t.CreatedByUser).Include(t => t.Status);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Tickewts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser)
                .Include(t => t.Status)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickewts/Create
        public IActionResult Create()
        {
            ViewData["AssignedToUserId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["CreatedByID"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["StatusID"] = new SelectList(_context.TicketStatuses, "Id", "StatusName");
            return View();
        }

        // POST: Tickewts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TicketID,Title,Description,DueDate,StatusID,TaskID,CreatedByID,CreatedDate,UpdatedBy,UpdatedDate,AssignedToUserId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AssignedToUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.AssignedToUserId);
            ViewData["CreatedByID"] = new SelectList(_context.Users, "Id", "Id", ticket.CreatedByID);
            ViewData["StatusID"] = new SelectList(_context.TicketStatuses, "Id", "StatusName", ticket.StatusID);
            return View(ticket);
        }

        // GET: Tickewts/Edit/5
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
            ViewData["AssignedToUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.AssignedToUserId);
            ViewData["CreatedByID"] = new SelectList(_context.Users, "Id", "Id", ticket.CreatedByID);
            ViewData["StatusID"] = new SelectList(_context.TicketStatuses, "Id", "StatusName", ticket.StatusID);
            return View(ticket);
        }

        // POST: Tickewts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TicketID,Title,Description,DueDate,StatusID,TaskID,CreatedByID,CreatedDate,UpdatedBy,UpdatedDate,AssignedToUserId")] Ticket ticket)
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
            ViewData["AssignedToUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.AssignedToUserId);
            ViewData["CreatedByID"] = new SelectList(_context.Users, "Id", "Id", ticket.CreatedByID);
            ViewData["StatusID"] = new SelectList(_context.TicketStatuses, "Id", "StatusName", ticket.StatusID);
            return View(ticket);
        }

        // GET: Tickewts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser)
                .Include(t => t.Status)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickewts/Delete/5
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
