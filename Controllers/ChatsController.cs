using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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

        // GET: Chats
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!_context.Chats.Any())
            {
                _context.Chats.AddRange(new List<Chats>
                {
                    new Chats {
                        TicketId = 101,
                        Sender = "SupportBot",
                        Message = "Hi there! How can I assist you today?",
                        SentAt = DateTime.Now
                    },
                    new Chats {
                        TicketId = 101,
                        Sender = "UserEN",
                        Message = "Hey, just trying to test my view.",
                        SentAt = DateTime.Now.AddMinutes(-2)
                    }
                });

                await _context.SaveChangesAsync();
            }

            var chats = await _context.Chats.ToListAsync();
            return View(chats);
        }

        // GET: Chats/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Chats/Create (Inline replies and full form)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TicketId,Message")] Chats chats)
        {
            if (ModelState.IsValid)
            {
                chats.Sender = User.Identity.Name;
                chats.SentAt = DateTime.Now;

                _context.Add(chats);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Log model validation errors (useful in development)
            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    Console.WriteLine($"Validation error in '{state.Key}': {error.ErrorMessage}");
                }
            }

            return View(chats);
        }

        // GET: Chats/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var chats = await _context.Chats.FirstOrDefaultAsync(m => m.Id == id);
            if (chats == null)
                return NotFound();

            return View(chats);
        }

        // GET: Chats/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var chats = await _context.Chats.FindAsync(id);
            if (chats == null)
                return NotFound();

            return View(chats);
        }

        // POST: Chats/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TicketId,Sender,Message,SentAt")] Chats chats)
        {
            if (id != chats.Id)
                return NotFound();

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
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(chats);
        }

        // GET: Chats/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var chats = await _context.Chats.FirstOrDefaultAsync(m => m.Id == id);
            if (chats == null)
                return NotFound();

            return View(chats);
        }

        // POST: Chats/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chats = await _context.Chats.FindAsync(id);
            if (chats != null)
                _context.Chats.Remove(chats);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ChatsExists(int id)
        {
            return _context.Chats.Any(e => e.Id == id);
        }
    }
}