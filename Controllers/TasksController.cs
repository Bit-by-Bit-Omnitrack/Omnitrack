using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UserRoles.Models;
using UserRoles.Data;

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
            // Eager load AssignedToUser AND CreatedByUser
            return View(await _context.Tasks
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser) // Include the CreatedByUser
                .ToListAsync());
        }

        // GET: Tasks/MyTasks
        [HttpGet]
        public async Task<IActionResult> MyTasks()
        {
            // Optional: Check if the user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            // Filter tasks by the current user's username or ID
            var myTasks = await _context.Tasks
                .Where(t => t.CreatedBy == User.Identity.Name) // Or use user ID if applicable
                .ToListAsync();

            return View(myTasks);
        }


        // GET: Tasks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tasks = await _context.Tasks
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser) // Include CreatedByUser for details view
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tasks == null)
            {
                return NotFound();
            }

            return View(tasks);
        }

        // GET: Tasks/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Users = new SelectList(await _userManager.Users.Where(u => u.IsActive).ToListAsync(), "Id", "UserName");
            return View();
        }


        // POST: Tasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,AssignedToUserId,Details,DueDate")] Tasks tasks) // Removed CreatedBy from bind
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    tasks.CreatedById = currentUser.Id; // Assign the CreatedById
                }
                else
                {
                    // Handle cases where there's no current user (e.g., if you have an admin seed task)
                    // You might want to assign a specific "System" user ID here if you have one,
                    // or make CreatedById non-nullable and enforce a user.
                    // For now, let's keep it nullable as per your model.
                    tasks.CreatedById = null; // Or a specific ID for "System" user if you have one
                }

                _context.Add(tasks);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Users = new SelectList(await _userManager.Users.Where(u => u.IsActive).ToListAsync(), "Id", "UserName", tasks.AssignedToUserId);
            return View(tasks);
        }


        // GET: Tasks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tasks = await _context.Tasks.FindAsync(id);
            if (tasks == null)
            {
                return NotFound();
            }
            ViewBag.Users = new SelectList(await _userManager.Users.Where(u => u.IsActive).ToListAsync(), "Id", "UserName", tasks.AssignedToUserId);
            return View(tasks);
        }

        // POST: Tasks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,AssignedToUserId,Details,DueDate")] Tasks tasks) // Removed CreatedBy from bind
        {
            if (id != tasks.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Fetch the original task to preserve CreatedById
                    var existingTask = await _context.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
                    if (existingTask == null)
                    {
                        return NotFound();
                    }

                    // Assign the existing CreatedById to the updated tasks object
                    tasks.CreatedById = existingTask.CreatedById;

                    _context.Update(tasks);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TasksExists(tasks.Id))
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
            ViewBag.Users = new SelectList(await _userManager.Users.Where(u => u.IsActive).ToListAsync(), "Id", "UserName", tasks.AssignedToUserId);
            return View(tasks);
        }

        // GET: Tasks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tasks = await _context.Tasks
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser) // Include CreatedByUser for delete confirmation
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tasks == null)
            {
                return NotFound();
            }

            return View(tasks);
        }

        // POST: Tasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tasks = await _context.Tasks.FindAsync(id);
            if (tasks != null)
            {
                _context.Tasks.Remove(tasks);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TasksExists(int id)
        {
            return _context.Tasks.Any(e => e.Id == id);
        }
    }
}