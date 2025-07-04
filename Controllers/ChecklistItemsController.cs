using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserRoles.Data;
using UserRoles.Models;

public class ChecklistItemsController : Controller
{
    private readonly AppDbContext _context;

    public ChecklistItemsController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var items = await _context.ChecklistItems.Include(c => c.Checklist).ToListAsync();
        return View(items);
    }

    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(ChecklistItem item)
    {
        if (ModelState.IsValid)
        {
            _context.ChecklistItems.Add(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(item);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var item = await _context.ChecklistItems.FindAsync(id);
        return item == null ? NotFound() : View(item);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(ChecklistItem item)
    {
        if (ModelState.IsValid)
        {
            _context.Update(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(item);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var item = await _context.ChecklistItems.FindAsync(id);
        return item == null ? NotFound() : View(item);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _context.ChecklistItems.FindAsync(id);
        _context.ChecklistItems.Remove(item);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}