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
    public class ChatsController : Controller
    {
        private readonly AppDbContext _context;

        public ChatsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var chats = await _context.Chats.ToListAsync();
            return View(chats); // Must match "Views/Chats/Index.cshtml"
        }

        // GET: Chats/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chats = await _context.Chats
                .FirstOrDefaultAsync(m => m.Id == id);
            if (chats == null)
            {
                return NotFound();
            }

            return View(chats);
        }

        // GET: Chats/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Chats/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TicketId,Sender,Message,SentAt")] Chats chats)
        {
            if (ModelState.IsValid)
            {
                _context.Add(chats);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(chats);
        }

        // GET: Chats/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chats = await _context.Chats.FindAsync(id);
            if (chats == null)
            {
                return NotFound();
            }
            return View(chats);
        }

        // POST: Chats/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TicketId,Sender,Message,SentAt")] Chats chats)
        {
            if (id != chats.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chats);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChatsExists(chats.Id))
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
            return View(chats);
        }

        // GET: Chats/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chats = await _context.Chats
                .FirstOrDefaultAsync(m => m.Id == id);
            if (chats == null)
            {
                return NotFound();
            }

            return View(chats);
        }

        // POST: Chats/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chats = await _context.Chats.FindAsync(id);
            if (chats != null)
            {
                _context.Chats.Remove(chats);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ChatsExists(int id)
        {
            return _context.Chats.Any(e => e.Id == id);
        }
    }
}
