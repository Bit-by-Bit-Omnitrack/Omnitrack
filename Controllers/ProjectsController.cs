using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using UserRoles.Data;
using UserRoles.Models;

namespace UserRoles.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly AppDbContext _context;

        public ProjectsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Projects
        public async Task<IActionResult> Index(string searchString, string statusFilter)
        {
            var projects = _context.Projects.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                projects = projects.Where(p => p.ProjectName.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                if (statusFilter == "Active")
                    projects = projects.Where(p => p.IsActive);
                else if (statusFilter == "Inactive")
                    projects = projects.Where(p => !p.IsActive);
            }

            var model = await projects.ToListAsync();
            return View(model);
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects
                .Include(p => p.Members)
                    .ThenInclude(pm => pm.User)
                .Include(p => p.Tickets)
                .FirstOrDefaultAsync(p => p.ProjectId == id);

            if (project == null) return NotFound();

            ViewBag.Users = await _context.UsersTable
                .Where(u => u.IsActive && u.IsApproved)
                .ToListAsync();

            return View(project);
        }

        // GET: Projects/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProjectId,ProjectName,Description,StartDate,EndDate,IsActive")] Project project)
        {
            if (!ModelState.IsValid)
                return View(project);

            _context.Add(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound();

            return View(project);
        }

        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProjectId,ProjectName,Description,StartDate,EndDate,IsActive")] Project project)
        {
            if (id != project.ProjectId) return NotFound();

            if (!ModelState.IsValid) return View(project);

            try
            {
                _context.Update(project);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(project.ProjectId)) return NotFound();
                else throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects
                .FirstOrDefaultAsync(m => m.ProjectId == id);

            if (project == null) return NotFound();

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Projects/AssignMember
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignMember(int projectId, string userId, string role)
        {
            var existing = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);

            if (existing != null)
                return BadRequest("User already assigned to this project.");

            var member = new ProjectMember
            {
                ProjectId = projectId,
                UserId = userId,
                ProjectRole = role
            };

            _context.ProjectMembers.Add(member);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = projectId });
        }

        // POST: Projects/RemoveMember
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(int projectId, string userId)
        {
            var member = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);

            if (member == null)
                return NotFound();

            _context.ProjectMembers.Remove(member);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = projectId });
        }

        // POST: Projects/EditMemberRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMemberRole(int projectId, string userId, string newRole)
        {
            if (string.IsNullOrWhiteSpace(newRole))
                return BadRequest("Role cannot be empty.");

            var member = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);

            if (member == null)
                return NotFound();

            member.ProjectRole = newRole;
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = projectId });
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.ProjectId == id);
        }
    }
}
