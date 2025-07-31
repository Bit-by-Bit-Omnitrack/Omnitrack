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
using Microsoft.AspNetCore.Authorization;

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

        // ✅ GET: Tasks
        public async Task<IActionResult> Index()
        {
            var tasks = await _context.Tasks
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser)
                .Include(t => t.Project) // Include Project for display
                .ToListAsync();

            return View(tasks);
        }

        // ✅ GET: Tasks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var tasks = await _context.Tasks
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser)
                .Include(t => t.Project)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (tasks == null) return NotFound();

            return View(tasks);
        }

        // ✅ GET: Tasks/Create
        [Authorize(Roles = "Project Leader")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Users = new SelectList(await _userManager.Users.Where(u => u.IsActive).ToListAsync(), "Id", "UserName");
            ViewBag.Projects = new SelectList(await _context.Projects.ToListAsync(), "ProjectId", "ProjectName");
            return View();
        }

        // ✅ POST: Tasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,AssignedToUserId,Details,DueDate,ProjectId")] Tasks tasks)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                tasks.CreatedById = currentUser?.Id;

                _context.Add(tasks);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Users = new SelectList(await _userManager.Users.Where(u => u.IsActive).ToListAsync(), "Id", "UserName", tasks.AssignedToUserId);
            ViewBag.Projects = new SelectList(await _context.Projects.ToListAsync(), "ProjectId", "ProjectName", tasks.ProjectId);
            return View(tasks);
        }

        // ✅ GET: Tasks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var tasks = await _context.Tasks.FindAsync(id);
            if (tasks == null) return NotFound();

            ViewBag.Users = new SelectList(await _userManager.Users.Where(u => u.IsActive).ToListAsync(), "Id", "UserName", tasks.AssignedToUserId);
            ViewBag.Projects = new SelectList(await _context.Projects.ToListAsync(), "ProjectId", "ProjectName", tasks.ProjectId);
            return View(tasks);
        }

        // ✅ POST: Tasks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,AssignedToUserId,Details,DueDate,ProjectId")] Tasks tasks)
        {
            if (id != tasks.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Preserve CreatedById
                    var existingTask = await _context.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
                    if (existingTask == null) return NotFound();

                    tasks.CreatedById = existingTask.CreatedById;

                    _context.Update(tasks);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TasksExists(tasks.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Users = new SelectList(await _userManager.Users.Where(u => u.IsActive).ToListAsync(), "Id", "UserName", tasks.AssignedToUserId);
            ViewBag.Projects = new SelectList(await _context.Projects.ToListAsync(), "ProjectId", "ProjectName", tasks.ProjectId);
            return View(tasks);
        }

        // ✅ GET: Tasks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var tasks = await _context.Tasks
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser)
                .Include(t => t.Project)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (tasks == null) return NotFound();

            return View(tasks);
        }

        // ✅ POST: Tasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tasks = await _context.Tasks.FindAsync(id);
            if (tasks != null)
            {
                _context.Tasks.Remove(tasks);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TasksExists(int id)
        {
            return _context.Tasks.Any(e => e.Id == id);
        }
    }
}
