using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserRoles.Data;
using UserRoles.Models;

namespace UserRoles.Controllers
{
    [Authorize]
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
            var chats = await _context.Chats
                .OrderByDescending(c => c.SentAt)
                .ToListAsync();

            return View(chats);
        }

        // GET: Chats/Create
        [Authorize(Roles = "Admin,Support")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Chats/Create
        [Authorize(Roles = "Admin,Support")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TicketId,Message")] Chats chats, IFormFile attachment)
        {
            Console.WriteLine("Create POST triggered");

            if (ModelState.IsValid)
            {
                chats.Sender = User.Identity.Name ?? "Anonymous";
                chats.SentAt = DateTime.Now;
                chats.Status = "New";
                chats.RoleTag = User.IsInRole("Admin") ? "Admin"
                              : User.IsInRole("Support") ? "Support"
                              : "User";

                if (attachment != null && attachment.Length > 0)
                {
                    var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                    Directory.CreateDirectory(uploadsDir);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(attachment.FileName);
                    var filePath = Path.Combine(uploadsDir, fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await attachment.CopyToAsync(stream);

                    chats.AttachmentPath = "/uploads/" + fileName;
                }

                _context.Add(chats);

                try
                {
                    await _context.SaveChangesAsync();
                    Console.WriteLine("Chat saved successfully.");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving chat: {ex.Message}");
                }
            }

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
            if (id == null) return NotFound();

            var chat = await _context.Chats.FirstOrDefaultAsync(c => c.Id == id);
            if (chat == null) return NotFound();

            return View(chat);
        }

        // GET: Chats/Edit/5
        [Authorize(Roles = "Admin,Support")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var chat = await _context.Chats.FindAsync(id);
            if (chat == null) return NotFound();

            return View(chat);
        }

        // POST: Chats/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TicketId,Sender,Message,SentAt,Status,AttachmentPath,RoleTag")] Chats chats)
        {
            if (id != chats.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chats);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChatsExists(chats.Id)) return NotFound();
                    else throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(chats);
        }

        // GET: Chats/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var chat = await _context.Chats.FirstOrDefaultAsync(c => c.Id == id);
            if (chat == null) return NotFound();

            return View(chat);
        }

        // POST: Chats/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chat = await _context.Chats.FindAsync(id);
            if (chat != null) _context.Chats.Remove(chat);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ChatsExists(int id)
        {
            return _context.Chats.Any(c => c.Id == id);
        }
    }
}