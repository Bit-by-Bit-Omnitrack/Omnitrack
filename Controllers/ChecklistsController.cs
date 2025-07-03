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
    public class ChecklistsController : Controller
    {
        private readonly AppDbContext _context;

        public ChecklistsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Checklists
        public async Task<IActionResult> Index()
        {
            return View(await _context.Checklists.ToListAsync());
        }

        // GET: Checklists/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var checklists = await _context.Checklists
                .FirstOrDefaultAsync(m => m.Id == id);
            if (checklists == null)
            {
                return NotFound();
            }

            return View(checklists);
        }

        // GET: Checklists/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Checklists/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title")] Checklists checklists)
        {
            if (ModelState.IsValid)
            {
                _context.Add(checklists);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(checklists);
        }

        // GET: Checklists/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var checklists = await _context.Checklists.FindAsync(id);
            if (checklists == null)
            {
                return NotFound();
            }
            return View(checklists);
        }

        // POST: Checklists/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title")] Checklists checklists)
        {
            if (id != checklists.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(checklists);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChecklistsExists(checklists.Id))
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
            return View(checklists);
        }

        // GET: Checklists/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var checklists = await _context.Checklists
                .FirstOrDefaultAsync(m => m.Id == id);
            if (checklists == null)
            {
                return NotFound();
            }

            return View(checklists);
        }

        // POST: Checklists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var checklists = await _context.Checklists.FindAsync(id);
            if (checklists != null)
            {
                _context.Checklists.Remove(checklists);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ChecklistsExists(int id)
        {
            return _context.Checklists.Any(e => e.Id == id);
        }
    }
}
