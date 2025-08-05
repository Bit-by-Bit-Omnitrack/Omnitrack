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

        // GET: Tasks
        public async Task<IActionResult> Index()
        {
            // Fetch all tasks, including related Project, AssignedToUser, and CreatedByUser details
            var tasks = await _context.Tasks
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser)
                .Include(t => t.Project)
                .Include(t => t.Status)
                .ToListAsync();

            // Fetch all task statuses for the kanban board columns
            ViewBag.Statuses = await _context.TaskStatuses.ToListAsync();

            return View(tasks);
        }

        // POST: Tasks/UpdateTaskStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTaskStatus(int taskId, int newStatusId)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
            {
                return Json(new { success = false, message = "Task not found." });
            }

            task.StatusID = newStatusId;
            try
            {
                _context.Update(task);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Task status updated successfully." });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Json(new { success = false, message = "Concurrency error. Task may have been updated by another user." });
            }
        }

        // GET: Tasks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var tasks = await _context.Tasks
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser)
                .Include(t => t.Project)
                .Include(t => t.Status)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (tasks == null) return NotFound();

            return View(tasks);
        }

        // GET: Tasks/Create
        [Authorize(Roles = "Project Leader")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Users = new SelectList(await _userManager.Users.Where(u => u.IsActive).ToListAsync(), "Id", "UserName");
            ViewBag.Projects = new SelectList(await _context.Projects.ToListAsync(), "ProjectId", "ProjectName");
            ViewBag.Statuses = new SelectList(await _context.TaskStatuses.ToListAsync(), "Id", "StatusName");
            return View();
        }

        // POST: Tasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,AssignedToUserId,Details,DueDate,ProjectId,StatusID")] Tasks tasks)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                tasks.CreatedById = currentUser?.Id;
                tasks.StatusID = tasks.StatusID > 0 ? tasks.StatusID : 1; // Default to 'To Do' status if not set.

                _context.Add(tasks);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Users = new SelectList(await _userManager.Users.Where(u => u.IsActive).ToListAsync(), "Id", "UserName", tasks.AssignedToUserId);
            ViewBag.Projects = new SelectList(await _context.Projects.ToListAsync(), "ProjectId", "ProjectName", tasks.ProjectId);
            ViewBag.Statuses = new SelectList(await _context.TaskStatuses.ToListAsync(), "Id", "StatusName", tasks.StatusID);
            return View(tasks);
        }

        // GET: Tasks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var tasks = await _context.Tasks.FindAsync(id);
            if (tasks == null) return NotFound();

            ViewBag.Users = new SelectList(await _userManager.Users.Where(u => u.IsActive).ToListAsync(), "Id", "UserName", tasks.AssignedToUserId);
            ViewBag.Projects = new SelectList(await _context.Projects.ToListAsync(), "ProjectId", "ProjectName", tasks.ProjectId);
            ViewBag.Statuses = new SelectList(await _context.TaskStatuses.ToListAsync(), "Id", "StatusName", tasks.StatusID);
            return View(tasks);
        }

        // POST: Tasks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,AssignedToUserId,Details,DueDate,ProjectId,StatusID")] Tasks tasks)
        {
            if (id != tasks.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
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
            ViewBag.Statuses = new SelectList(await _context.TaskStatuses.ToListAsync(), "Id", "StatusName", tasks.StatusID);
            return View(tasks);
        }

        // GET: Tasks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var tasks = await _context.Tasks
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser)
                .Include(t => t.Project)
                .Include(t => t.Status)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (tasks == null) return NotFound();

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