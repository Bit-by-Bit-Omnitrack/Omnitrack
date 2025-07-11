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
            var appDbContext = _context.Tickets
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser); // Include CreatedByUser as well if you want to display it
            return View(await appDbContext.ToListAsync());
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
                .Include(t => t.Status) // If you add a navigation property for status
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
            // --- Set auto-generated and default values BEFORE ModelState.IsValid check ---

            ticket.TicketID = $"TCK-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
            ticket.TaskID = $"TSK-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
            ticket.StatusID = 1; // Default status: 1 = To Do
            ticket.CreatedDate = DateTime.UtcNow;

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                ticket.CreatedByID = currentUser.Id;
            }
            else
            {
                // IMPORTANT: If CreatedByID is [Required] in your model and nullable: false in DB,
                // this "System" string might not be a valid user ID, causing validation issues.
                // If the user MUST be logged in to create a ticket, consider:
                // return Unauthorized(); // Or redirect to login
                // For now, let's assume it can be "System" if the field allows non-GUID string.
                // If CreatedByID MUST be a valid user ID, you cannot allow an unauthenticated user to create.
                ticket.CreatedByID = "System"; // Fallback, but be careful with DB constraints
            }

            // Add dummy values for UpdatedBy and UpdatedDate if they are required and you don't want them null for new tickets
            // Although your model and migration indicate they are nullable.
            ticket.UpdatedBy = null;
            ticket.UpdatedDate = null;


            // --- Now check ModelState.IsValid ---
            if (ModelState.IsValid)
            {
                _context.Add(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // If ModelState is not valid, repopulate ViewBags and return the view
            // IMPORTANT: Use the correct role filtering for ViewBag.Users as discussed before
            string approverRoleName = "Admin"; // Or whatever your role is
            var allUsers = await _userManager.Users.ToListAsync();
            var approverUsers = new List<Users>();
            foreach (var user in allUsers)
            {
                if (await _userManager.IsInRoleAsync(user, approverRoleName))
                {
                    approverUsers.Add(user);
                }
            }
            ViewBag.Users = new SelectList(approverUsers, "Id", "FullName", ticket.AssignedToUserId); // Pass selected value for repopulation

            ViewBag.StatusID = new SelectList(_context.TicketStatuses, "Id", "StatusName", ticket.StatusID); // Use StatusID for selected value
            return View(ticket);
        }


        // GET: Tickets1/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TicketID,Title,Description,StatusID,TaskID,CreatedByID,CreatedDate,DueDate,AssignedToUserId")] Ticket ticket) // Add AssignedToUserId to bind
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Retrieve the existing ticket to preserve CreatedByID and CreatedDate
                    var existingTicket = await _context.Tickets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
                    if (existingTicket == null)
                    {
                        return NotFound();
                    }

                    // Manually set properties that are not bound or should be auto-updated
                    ticket.CreatedByID = existingTicket.CreatedByID;
                    ticket.CreatedDate = existingTicket.CreatedDate;
                    ticket.TicketID = existingTicket.TicketID;
                    ticket.TaskID = existingTicket.TaskID;

                    var currentUser = await _userManager.GetUserAsync(User);
                    if (currentUser != null)
                    {
                        ticket.UpdatedBy = currentUser.Id; // Assuming UpdatedBy is also a UserId
                    }
                    else
                    {
                        ticket.UpdatedBy = "System";
                    }// Set current user as updater
                    ticket.UpdatedDate = DateTime.UtcNow; // Set update date

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
            // Repopulate ViewBag for dropdowns if model state is invalid
            ViewBag.Users = new SelectList(await _userManager.Users.Where(u => u.IsActive).ToListAsync(), "Id", "UserName", ticket.AssignedToUserId);
            ViewBag.Statuses = new SelectList(await _context.TicketStatuses.ToListAsync(), "Id", "Name", ticket.StatusID);
            return View(ticket);
        }

        // POST: Tickets1/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
       

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
