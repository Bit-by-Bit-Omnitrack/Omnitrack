using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using UserRoles.Migrations;
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
            // Eager load related data for display in the dashboard
            var tickets = await _context.Tickets
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .Include(t => t.Status)
                .Include(t => t.Priority)
                .ToListAsync();
            return View(tickets);
        }

        // GET: Tickets1/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser) // Include CreatedByUser for details
                .Include(t => t.Status)
                .Include(t => t.Priority)// If you add a navigation property for status
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets1/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PrioritySeeder.SeedAsync(_context);
            // Populate ViewBag.Users for the dropdown in the Create view
            ViewBag.Users = new SelectList(await _userManager.Users.Where(u => u.IsActive).ToListAsync(), "Id", "UserName");
            ViewBag.Priorities = new SelectList(await _context.Priorities.ToListAsync(), "Id", "Name");
            return View();
        }

        // POST: Tickets1/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create([Bind("Title,Description,AssignedToUserId,DueDate,PriorityId")] Ticket ticket)
        {
            // --- Set auto-generated and default values BEFORE ModelState.IsValid check ---

            ticket.TicketID = $"TCK-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
            ticket.TaskID = $"TSK-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
            ticket.StatusID = 1; // Default status: 1 = To Do
            ticket.CreatedDate = DateTime.UtcNow;

            // Default status: 1 = To Do
            ticket.StatusID = 1;

            // Set CreatedByID to current logged-in username
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                ticket.CreatedByID = currentUser.Id;
            }
            else
            {
                ticket.CreatedByID = "System"; // Fallback
            }

            // Set CreatedDate to now
            ticket.CreatedDate = DateTime.UtcNow;

        /*    _context.Add(ticket);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        */
            ViewBag.Users = new SelectList(await _userManager.Users.Where(u => u.IsActive).ToListAsync(), "Id", "UserName");
            ViewBag.Statuses = new SelectList(await _context.TicketStatuses.ToListAsync(), "Id", "Name");
            ViewBag.Priorities = new SelectList(await _context.Priorities.ToListAsync(), "Id", "Name", ticket.PriorityId);
            //   return View(ticket);
            _context.Add(ticket);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Tickets1/Edit/5
        [HttpGet]
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

            ViewBag.Users = new SelectList(await _userManager.Users
                .Where(u => u.IsActive).ToListAsync(), "Id", "UserName", ticket.AssignedToUserId);

            ViewBag.Statuses = new SelectList(await _context.TicketStatuses
                .ToListAsync(), "Id", "StatusName", ticket.StatusID);
            ViewBag.Priorities = new SelectList(await _context.Priorities.ToListAsync(), "Id", "Name", ticket.PriorityId);
            return View(ticket);
            
        }

        // POST: Tickets1/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,AssignedToUserId,DueDate,StatusID,PriorityId")] Ticket ticket)
        {
            if (id != ticket.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Users = new SelectList(await _userManager.Users.Where(u => u.IsActive).ToListAsync(), "Id", "UserName", ticket.AssignedToUserId);
                ViewBag.Statuses = new SelectList(await _context.TicketStatuses.ToListAsync(), "Id", "StatusName", ticket.StatusID);
                ViewBag.Priorities = new SelectList(await _context.Priorities.ToListAsync(), "Id", "Name", ticket.PriorityId);
               
                //return View(ticket);
            }

            var existingTicket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
            if (existingTicket == null)
                return NotFound();

            try
            {
                // Only update fields the user is allowed to change
                existingTicket.Title = ticket.Title;
                existingTicket.Description = ticket.Description;
                existingTicket.AssignedToUserId = ticket.AssignedToUserId;
                existingTicket.DueDate = ticket.DueDate;
                existingTicket.StatusID = ticket.StatusID;

                // Update metadata
                var currentUser = await _userManager.GetUserAsync(User);
                existingTicket.UpdatedBy = currentUser?.Id ?? "System";
                existingTicket.UpdatedDate = DateTime.UtcNow;


                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TicketExists(ticket.Id))
                    return NotFound();
                else
                    throw;
            }
        }


        // POST: Tickets1/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.



        // GET: Tickets1/Delete/5
        [HttpGet]
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
        public static class PrioritySeeder
        {
            public static async Task SeedAsync(AppDbContext context)
            {
                if (!context.Priorities.Any())
                {
                    var defaultPriorities = new List<Priority>
                {
                    new Priority { Level = "Low" },
                    new Priority { Level = "Medium" },
                    new Priority { Level = "High" },
                    new Priority { Level = "Critical" }
                };

                    context.Priorities.AddRange(defaultPriorities);
                    await context.SaveChangesAsync();
                }
            }
        }
        private bool TicketExists(int id)
        {
            return _context.Tickets.Any(e => e.Id == id);
        }
    }
}