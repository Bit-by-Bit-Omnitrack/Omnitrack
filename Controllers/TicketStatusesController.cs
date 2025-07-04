using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserRoles.Data;
using UserRoles.Models;

public class TicketStatusesController : Controller
{
    private readonly AppDbContext _context;

    public TicketStatusesController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index() => View(await _context.TicketStatuses.ToListAsync());

    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(TicketStatus status)
    {
        if (ModelState.IsValid)
        {
            _context.TicketStatuses.Add(status);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(status);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var status = await _context.TicketStatuses.FindAsync(id);
        return status == null ? NotFound() : View(status);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(TicketStatus status)
    {
        if (ModelState.IsValid)
        {
            _context.Update(status);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(status);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var status = await _context.TicketStatuses.FindAsync(id);
        return status == null ? NotFound() : View(status);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var status = await _context.TicketStatuses.FindAsync(id);
        _context.TicketStatuses.Remove(status);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}