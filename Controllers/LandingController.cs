using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Security.Claims;
using UserRoles.Data; // Access to the database context
using UserRoles.Models; // Access to the models

namespace UserRoles.Controllers
{
    // This controller manages the landing page after login
    [Authorize] // Only logged-in users can access
    public class LandingController : Controller
    {
        private readonly AppDbContext _context;

        // This constructor provides access to the database
        public LandingController(AppDbContext context)
        {
            _context = context;
        }

        // This method loads the landing page with welcome message, stats, and quote
        public IActionResult Index()
        {
            // Get the current logged-in user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // If user is not logged in (just in case), redirect to login page
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Fetch user details from the database
            var user = _context.Users.SingleOrDefault(u => u.Id == userId);

            // Count how many tickets are assigned to this user
            var ticketCount = _context.Tickets.Count(t => t.AssignedToUserId == userId);

            // Count how many tasks are assigned to this user
            var taskCount = _context.Tasks.Count(t => t.AssignedTo == userId);

            // List of motivational quotes
            var quotes = new[]
            {
        "You are capable of amazing things",
        "Progress is better than perfection",
        "Stay positive, work hard, make it happen",
        "Small steps every day lead to big results",
        "Believe in yourself and all that you are",
        "Success is the sum of small efforts repeated daily",
        "Your only limit is your mind",
        "Do something today that your future self will thank you for",
        "Push yourself because no one else is going to do it for you",
        "Dream it. Wish it. Do it",
        "Work hard in silence. Let success make the noise",
        "The harder you work for something, the greater you’ll feel when you achieve it",
        "It always seems impossible until it is done",
        "Don’t wait for opportunity. Create it",
        "Discipline is doing it even when you don’t feel like it"
    };

            // Select a random quote
            var random = new Random();
            var quote = quotes[random.Next(quotes.Length)];

            // Pass data to the view
            ViewBag.UserName = user?.FullName ?? "User";
            ViewBag.Today = DateTime.Now.ToString("dddd, dd MMMM yyyy");
            ViewBag.TicketCount = ticketCount;
            ViewBag.TaskCount = taskCount;
            ViewBag.Quote = quote;

            return View();
        }

    }
}

