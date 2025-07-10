using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UserRoles.Models;

public class ChatsModel : PageModel
{
    private readonly AppDbContext _context;

    public ChatsModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Chats NewChat { get; set; }

    public List<Chats> Chats { get; set; }

    public async Task OnGetAsync()
    {
        Chats = await _context.Chats
            .OrderByDescending(c => c.SentAt)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid) return Page();

        NewChat.SentAt = DateTime.UtcNow;
        _context.Chats.Add(NewChat);
        await _context.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var chat = await _context.Chats.FindAsync(id);
        if (chat != null)
        {
            _context.Chats.Remove(chat);
            await _context.SaveChangesAsync();
        }
        return RedirectToPage();
    }
}
