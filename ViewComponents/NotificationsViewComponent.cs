using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using UserRoles.Data;
using UserRoles.Models;

namespace UserRoles.ViewComponents
{
    public class NotificationsViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public NotificationsViewComponent(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            int unreadCount = 0;
            if (user != null)
            {
                unreadCount = await _context.Notifications
                    .Where(n => n.UserId == user.Id && !n.IsRead)
                    .CountAsync();
            }

            return View(unreadCount);
        }
    }
}
