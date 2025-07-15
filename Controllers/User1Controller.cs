using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserRoles.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserRoles.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace UserRoles.Controllers
{
    public class User1Controller : Controller
    {
        private readonly UserManager<Users> _userManager;

        public User1Controller(UserManager<Users> userManager)
        {
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
            var userRoles = new List<UserWithRolesViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles.Add(new UserWithRolesViewModel
                {
                    User = user,
                    Roles = roles
                });
            }

            return View(userRoles);
        }



        [HttpGet("Users/Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }
        [HttpPost("Users/Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Users model)
        {
            if (id != model.Id) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.UserName = model.UserName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(user);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }

        // POST: Users/Delete/5

        [HttpGet("Users/Details/{id}")]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null || !user.IsActive) return NotFound();

            return View(user);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsActive = false; // soft delete
            await _userManager.UpdateAsync(user);

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "System Administrator")]
        public IActionResult Authenticate()
        {
            var pendingUsers = _userManager.Users.Where(u => !u.IsActive).ToList();
            return View(pendingUsers); // Will be used by Authenticate.cshtml
        }
        [HttpPost]
        public async Task<IActionResult> Approve(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsActive = true;
            await _userManager.UpdateAsync(user);

            TempData["Message"] = "User approved successfully.";

            return RedirectToAction(nameof(Authenticate));
        }
        [HttpPost]
      
        public async Task<IActionResult> Reject(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                TempData["RejectMessage"] = "User has been rejected";
            }
            else
            {
                TempData["RejectMessage"] = "Failed to reject user.";
            }

            return RedirectToAction(nameof(Authenticate));
        }

    }
}
