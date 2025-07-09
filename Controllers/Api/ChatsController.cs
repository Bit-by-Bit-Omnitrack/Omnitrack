using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserRoles.Models;

namespace UserRoles.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ChatsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>Get all chat messages</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Chats>>> GetChats()
        {
            return await _context.Chats.ToListAsync();
        }

        /// <summary>Get a chat message by ID</summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Chats>> GetChat(int id)
        {
            var chat = await _context.Chats.FindAsync(id);
            if (chat == null) return NotFound();
            return chat;
        }

        /// <summary>Create a new chat message</summary>
        [HttpPost]
        public async Task<ActionResult<Chats>> PostChat(Chats chat)
        {
            chat.SentAt = DateTime.UtcNow;
            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetChat), new { id = chat.Id }, chat);
        }

        /// <summary>Update a chat message</summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutChat(int id, Chats chat)
        {
            if (id != chat.Id) return BadRequest();
            _context.Entry(chat).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>Delete a chat message</summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChat(int id)
        {
            var chat = await _context.Chats.FindAsync(id);
            if (chat == null) return NotFound();
            _context.Chats.Remove(chat);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
