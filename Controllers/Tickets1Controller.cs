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

        public async Task<IActionResult> Index(string searchTerm)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

            var query = _context.Tickets
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .Include(t => t.Status)
                .Include(t => t.Priority)
                .AsQueryable();

            if (!isAdmin)
            {
                query = query.Where(t =>
                    t.CreatedByID == currentUser.Id ||
                    t.AssignedToUserId == currentUser.Id);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(t =>
                    t.Title.Contains(searchTerm) ||
                    t.Description.Contains(searchTerm));
            }

            ViewBag.SearchTerm = searchTerm;
            var tickets = await query.ToListAsync();
            return View(tickets);
        }

        // GET: Tickets1/Create
        public async Task<IActionResult> Create()
        {
            // Seed priorities if none exist
            await PrioritySeeder.SeedAsync(_context);

            ViewBag.Users = new SelectList(await _userManager.Users.Where(u => u.IsActive).ToListAsync(), "Id", "UserName");
            ViewBag.Priorities = new SelectList(await _context.Priorities.ToListAsync(), "Id", "Name");
            return View();
        }

        // POST: Tickets1/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,AssignedToUser,DueDate,PriorityId")] Ticket ticket)
        {
            ticket.TicketID = $"TCK-{Guid.NewGuid().ToString()[..8].ToUpper()}";
            ticket.TaskID = $"TSK-{Guid.NewGuid().ToString()[..8].ToUpper()}";
            ticket.StatusID = 1;
            ticket.CreatedDate = DateTime.UtcNow;

            var currentUser = await _userManager.GetUserAsync(User);
            ticket.CreatedByID = currentUser?.Id ?? "System";

            _context.Add(ticket);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Tickets1/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return NotFound();

            ViewBag.Users = new SelectList(await _userManager.Users.Where(u => u.IsActive).ToListAsync(), "Id", "UserName", ticket.AssignedToUserId);
            ViewBag.Statuses = new SelectList(await _context.TicketStatuses.ToListAsync(), "Id", "StatusName", ticket.StatusID);
            ViewBag.Priorities = new SelectList(await _context.Priorities.ToListAsync(), "Id", "Name", ticket.PriorityId);
            return View(ticket);
        }

        // POST: Tickets1/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TicketID,Title,Description,StatusID,TaskID,CreatedByID,CreatedDate,DueDate,AssignedToUserId,PriorityId")] Ticket ticket)
        {
            if (id != ticket.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var existingTicket = await _context.Tickets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
                if (existingTicket == null) return NotFound();

                ticket.CreatedByID = existingTicket.CreatedByID;
                ticket.CreatedDate = existingTicket.CreatedDate;
                ticket.TicketID = existingTicket.TicketID;
                ticket.TaskID = existingTicket.TaskID;

                var currentUser = await _userManager.GetUserAsync(User);
                ticket.UpdatedBy = currentUser?.Id ?? "System";
                ticket.UpdatedDate = DateTime.UtcNow;

                _context.Update(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Users = new SelectList(await _userManager.Users.Where(u => u.IsActive).ToListAsync(), "Id", "UserName", ticket.AssignedToUserId);
            ViewBag.Statuses = new SelectList(await _context.TicketStatuses.ToListAsync(), "Id", "StatusName", ticket.StatusID);
            ViewBag.Priorities = new SelectList(await _context.Priorities.ToListAsync(), "Id", "Name", ticket.PriorityId);
            return View(ticket);
        }
    }

    // Seeder class for priorities
    public static class PrioritySeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (!context.Priorities.Any())
            {
                var defaultPriorities = new List<Priority>
                {
                    new Priority { Name = "Low" },
                    new Priority { Name = "Medium" },
                    new Priority { Name = "High" },
                    new Priority { Name = "Critical" }
                };

                context.Priorities.AddRange(defaultPriorities);
                await context.SaveChangesAsync();
            }
        }
    }
}
