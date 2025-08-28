using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserRoles.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserRoles.ViewModels;
using UserRoles.Services;
using Microsoft.AspNetCore.Authorization; // added for IEmailService

namespace UserRoles.Controllers
{
    [Authorize(Roles = "System Administrator")] // Ensure only authorized users can access this controller
    public class UserController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly IEmailService _emailService; // added email service

        // updated constructor to inject IEmailService
        
        public UserController(UserManager<Users> userManager, IEmailService emailService)
        {
            _userManager = userManager;
            _emailService = emailService;
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

        [Authorize(Roles = "System Administrator")]
        public IActionResult Authenticate()
        {
            var pendingUsers = _userManager.Users
            .Where(u => !u.IsActive && u.ApprovalStatus != UserApprovalStatus.Rejected)
            .ToList();

            return View(pendingUsers); // Will be used by Authenticate.cshtml
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

        [HttpGet("User/Details/{id}")]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

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
        [HttpPost]
        public async Task<IActionResult> Approve(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (!user.IsApproved)
            {
                user.IsApproved = true;
                user.IsActive = true;
                user.EmailConfirmed = true;

                await _userManager.UpdateAsync(user);

                var subject = "Your OmniTrack Account Has Been Approved";
                var body = $@"
            <p>Hi {user.FullName},</p>
            <p>Your account has been approved by an administrator.</p>
            <p>You may now log in to the system using your email and password.</p>
            <p>Kind regards,<br/>OmniTrack Team</p>";

                await _emailService.SendEmailAsync(user.Email, subject, body);
            }

            TempData["Message"] = "User approved successfully.";
            return RedirectToAction(nameof(Authenticate));
        }

        [HttpPost]
        public async Task<IActionResult> Reject(string id, string reason)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Set rejection data
            user.IsActive = false;
            user.ApprovalStatus = UserApprovalStatus.Rejected;
            user.RejectionReason = reason ?? "Rejected by admin"; // fallback if reason is null

            await _userManager.UpdateAsync(user);

            // Email user the rejection reason
            var subject = "OmniTrack Account Rejected";
            var body = $@"
        <p>Hi {user.FullName},</p>
        <p>Unfortunately, your account registration has been rejected.</p>
        <p><strong>Reason:</strong> {user.RejectionReason}</p>
        <p>If you believe this was in error, please contact the OmniTrack team.</p>
        <p>Kind regards,<br/>OmniTrack Team</p>";

            await _emailService.SendEmailAsync(user.Email, subject, body);

            TempData["RejectMessage"] = "User has been rejected.";
            return RedirectToAction(nameof(Authenticate));
        }


        // NEW METHOD: View rejected users
        [Authorize(Roles = "System Administrator")]
        public IActionResult RejectedUsers()
        {
            var rejectedUsers = _userManager.Users
                .Where(u => u.ApprovalStatus == UserApprovalStatus.Rejected)
                .ToList();

            return View(rejectedUsers); // You’ll create RejectedUsers.cshtml for this
        }

    }
}
