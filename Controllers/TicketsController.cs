using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using UserRoles.Models;
using Microsoft.AspNetCore.Authorization;
using UserRoles.Data; // Gives access to AppDbContext and other data-related classes

namespace UserRoles.Controllers
{
    public class TicketsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public TicketsController(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Tickets1
        public async Task<IActionResult> Index(int? filterTaskId, int? filterProjectId) // Added filterTaskId parameter
        {
            // Start with all tickets, eager load related data
            var ticketsQuery = _context.Tickets
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .Include(t => t.Status)
                .Include(t => t.Priority)
                .Include(t => t.Tasks)
                .Include(t => t.Project) // Include Project navigation property
                .AsQueryable(); // Use AsQueryable to allow further filtering

            // Apply task filter if filterTaskId is provided
            if (filterTaskId.HasValue && filterTaskId.Value > 0)
            {
                ticketsQuery = ticketsQuery.Where(t => t.TasksId == filterTaskId.Value);
            }

            // Apply project filter if filterProjectId is provided
            if (filterProjectId.HasValue && filterProjectId.Value > 0)
            {
                ticketsQuery = ticketsQuery.Where(t => t.ProjectId == filterProjectId.Value);
            }

            // Exclude tickets with StatusID 5 (Completed) from the main dashboard view
            ticketsQuery = ticketsQuery.Where(t => t.StatusID != 5);

            var tickets = await ticketsQuery.ToListAsync();

            ViewBag.Statuses = await _context.TicketStatuses.OrderBy(s => s.Id).ToListAsync();

            // Populate ViewBag for tasks dropdown in the filter
            ViewBag.TasksFilter = new SelectList(await _context.Tasks.ToListAsync(), "Id", "Name", filterTaskId);

            // Populate ViewBag for projects dropdown in the filter
            ViewBag.ProjectsFilter = new SelectList(await _context.Projects.ToListAsync(), "ProjectId", "ProjectName", filterProjectId);

            return View(tickets);
        }

        // GET: Tickets/History
        // GET: Tickets/History
        public async Task<IActionResult> History(int? statusId)
        {
            var completedTicketsQuery = _context.Tickets
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .Include(t => t.Status)
                .Include(t => t.Priority)
                .Include(t => t.Tasks)
                .Include(t => t.Project)
                .Where(t => t.StatusID == 5) // Assuming StatusID 5 is "Completed" or "Archived"
                .AsQueryable();

            // Apply filter if a statusId is provided
            if (statusId.HasValue)
            {
                completedTicketsQuery = completedTicketsQuery.Where(t => t.StatusID == statusId.Value);
            }

            var completedTickets = await completedTicketsQuery
                .OrderByDescending(t => t.UpdatedDate)
                .ToListAsync();

            return View(completedTickets);
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
                .Include(t => t.Tasks)
                .Include(t => t.Project) // Include Project navigation property
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        [Authorize(Roles = "Project Leader, System Administrator")]
        public async Task<IActionResult> Create()
        {
            // Populate ViewBag.Users for the dropdown in the Create view
            ViewBag.Users = new SelectList(await _userManager.Users.Where(u => u.IsActive).ToListAsync(), "Id", "UserName");
            ViewBag.Priorities = new SelectList(await _context.Priorities.ToListAsync(), "Id", "Name");
            ViewBag.Tasks = new SelectList(await _context.Tasks.ToListAsync(), "Id", "Name");
            ViewBag.Projects = new SelectList(await _context.Projects.ToListAsync(), "ProjectId", "ProjectName");
            return View();
        }

        // POST: Tickets1/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ticket ticket)
        {
            ticket.TicketID = $"TCK-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
            ticket.TaskID = $"TSK-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
            ticket.StatusID = 1;
            ticket.CreatedDate = DateTime.UtcNow;
            var currentUser = await _userManager.GetUserAsync(User);
            ticket.CreatedByID = currentUser?.Id ?? "System";

            ModelState.Remove("TicketID");
            ModelState.Remove("TaskID");
            ModelState.Remove("StatusID");
            ModelState.Remove("CreatedByID");
            ModelState.Remove("CreatedDate");
            ModelState.Remove("Priority");

            if (ModelState.IsValid)
            {
                _context.Add(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Users = new SelectList(await _userManager.Users.Where(u => u.IsActive).ToListAsync(), "Id", "UserName", ticket.AssignedToUserId);
            ViewBag.Priorities = new SelectList(await _context.Priorities.ToListAsync(), "Id", "Name", ticket.PriorityId);
            ViewBag.Tasks = new SelectList(await _context.Tasks.ToListAsync(), "Id", "Name", ticket.TasksId);
            ViewBag.Projects = new SelectList(await _context.Projects.ToListAsync(), "ProjectId", "ProjectName", ticket.ProjectId);

            return View(ticket);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTicketStatus(int ticketId, int newStatusId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null)
            {
                return NotFound();
            }

            // Check if the newStatusId is valid (exists in your TicketStatuses table)
            var newStatus = await _context.TicketStatuses.FindAsync(newStatusId);
            if (newStatus == null)
            {
                return BadRequest("Invalid Status ID.");
            }

            ticket.StatusID = newStatusId;

            // Optional: Update metadata
            var currentUser = await _userManager.GetUserAsync(User);
            ticket.UpdatedBy = currentUser?.Id ?? "System";
            ticket.UpdatedDate = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Ticket status updated successfully." });
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest("Failed to update ticket status due to a concurrency issue.");
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., using ILogger)
                Console.WriteLine($"Error updating ticket status: {ex.Message}");
                return StatusCode(500, "An error occurred while updating the ticket status.");
            }
        }

        // New action to archive a ticket by setting its status to 'Completed' (StatusID = 5)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveTicket(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            // Assuming StatusID 5 is "Completed" or "Archived"
            ticket.StatusID = 5;

            // Update metadata
            var currentUser = await _userManager.GetUserAsync(User);
            ticket.UpdatedBy = currentUser?.Id ?? "System";
            ticket.UpdatedDate = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Ticket archived successfully." });
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest("Failed to archive ticket due to a concurrency issue.");
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error archiving ticket: {ex.Message}");
                return StatusCode(500, "An error occurred while archiving the ticket.");
            }
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
            ViewBag.Tasks = new SelectList(await _context.Tasks.ToListAsync(), "Id", "Name", ticket.TasksId);

            return View(ticket);
        }

        // POST: Tickets1/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,AssignedToUserId,DueDate,StatusID,PriorityId,TasksId")] Ticket ticket)
        {
            if (id != ticket.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Users = new SelectList(await _userManager.Users.Where(u => u.IsActive).ToListAsync(), "Id", "UserName", ticket.AssignedToUserId);
                ViewBag.Statuses = new SelectList(await _context.TicketStatuses.ToListAsync(), "Id", "StatusName", ticket.StatusID);
                ViewBag.Priorities = new SelectList(await _context.Priorities.ToListAsync(), "Id", "Name", ticket.PriorityId);
                ViewBag.Tasks = new SelectList(await _context.Tasks.ToListAsync(), "Id", "Name", ticket.TasksId); // Re-populate with selected TaskId
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
                existingTicket.PriorityId = ticket.PriorityId; // Added this line to update priority
                existingTicket.TasksId = ticket.TasksId; // Added this line to update tasks

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

        private bool TicketExists(int id)
        {
            return _context.Tickets.Any(e => e.Id == id);
        }
    }
}