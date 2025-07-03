using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserRoles.Data;
using UserRoles.Models;

public class TicketAssignmentsController : Controller
{
    private readonly AppDbContext _context;

    public TicketAssignmentsController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var data = await _context.TicketAssignments.Include(t => t.User).Include(t => t.Ticket).ToListAsync();
        return View(data);
    }

    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(TicketAssignment ta)
    {
        if (ModelState.IsValid)
        {
            _context.TicketAssignments.Add(ta);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(ta);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var ta = await _context.TicketAssignments.FindAsync(id);
        return ta == null ? NotFound() : View(ta);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(TicketAssignment ta)
    {
        if (ModelState.IsValid)
        {
            _context.Update(ta);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(ta);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var ta = await _context.TicketAssignments.FindAsync(id);
        return ta == null ? NotFound() : View(ta);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var ta = await _context.TicketAssignments.FindAsync(id);
        _context.TicketAssignments.Remove(ta);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}