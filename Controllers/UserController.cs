using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using UserRoles.Data;
using UserRoles.Models;


namespace UserRoles.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;

        // injecting the DB context here so I can access the UsersTable
        public UserController(AppDbContext context)
        {
            _context = context;
        }

        // this shows a list of all users in the system
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.ToListAsync(); // changed from UsersTable to Users (DbSet<Users>)
            return View(users);
        }

        // this loads the form to create a new user manually
        public IActionResult Create()
        {
            return View();
        }

        // this saves the new user to the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Users user) //  renamed method to Create and corrected model name

        {
            if (ModelState.IsValid)
            {
                user.CreatedOn = DateTime.Now; // tracking when user was created
                _context.Add(user);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // if something is wrong with the form input
            return View(user);
        }

        // loads the edit page for a selected user
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        // updates the user's info after editing
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Users user)

        {
            if (id != user.Id)
                    return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    user.ModifiedOn = DateTime.Now; // record edit timestamp
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // make sure the user wasn't deleted in the meantime
                    if (!_context.Users.Any(e => e.Id == id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        // shows full info about a specific user
        public async Task<IActionResult> Details(string id)
        {
            var user = await _context.UsersTable.FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        // loads the confirmation page before deleting a user
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _context.UsersTable.FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        // deletes the user after confirmation
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _context.Users.FindAsync(id); // changed from UsersTable
            if (user != null)
            {
                _context.Users.Remove(user); // changed from UsersTable
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // this activates a user after admin approval
        [HttpPost]
        public async Task<IActionResult> Approve(string id)
        {
            var user = await _context.Users.FindAsync(id); // changed from UsersTable
            if (user != null)
            {
                user.IsActive = true; // approving user by setting them active
                await _context.SaveChangesAsync(); // save changes immediately
            }

            return RedirectToAction(nameof(Index));
        }

        // this deactivates a user (used for rejecting them)
        [HttpPost]
        public async Task<IActionResult> Reject(string id) // changed Guid to string
        {
            var user = await _context.Users.FindAsync(id); // changed from UsersTable
            if (user != null)
            {
                user.IsActive = false; // rejecting user by making them inactive
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // this displays all users who have been rejected or are inactive
        [Authorize(Roles = "Admin")] // making sure only admin can see this
        public async Task<IActionResult> Rejected()
        {
            // grabbing all users that are not active (i.e., rejected or pending)
            var rejectedUsers = await _context.UsersTable
                .Where(u => u.IsActive == false)
                .ToListAsync();

            return View(rejectedUsers); // pass them to the view
        }
    }
}
