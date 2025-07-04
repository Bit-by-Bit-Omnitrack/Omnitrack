using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserRoles.Data;
using UserRoles.Models;

namespace UserRoles.Controllers
{
    public class AppTaskController : Controller
    {
        private readonly AppDbContext _context;

        public AppTaskController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /AppTask
        public async Task<IActionResult> Index()
        {
            return View(await _context.AppTask.ToListAsync());
        }

        // GET: /AppTask/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var task = await _context.AppTask.FindAsync(id);
            if (task == null) return NotFound();

            return View(task);
        }

        // GET: /AppTask/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /AppTask/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppTask task)
        {
            if (ModelState.IsValid)
            {
                _context.AppTask.Add(task);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(task);
        }

        // GET: /AppTask/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var task = await _context.AppTask.FindAsync(id);
            if (task == null) return NotFound();

            return View(task);
        }

        // POST: /AppTask/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AppTask task)
        {
            if (id != task.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(task);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(task);
        }

        // GET: /AppTask/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var task = await _context.AppTask.FindAsync(id);
            if (task == null) return NotFound();

            return View(task);
        }

        // POST: /AppTask/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var task = await _context.AppTask.FindAsync(id);
            if (task != null)
            {
                _context.AppTask.Remove(task);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
